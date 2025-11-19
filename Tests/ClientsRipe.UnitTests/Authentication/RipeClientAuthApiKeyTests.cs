using ClientsRipe.UnitTests.Helpers;

namespace ClientsRipe.UnitTests.Authentication;

public class RipeClientAuthApiKeyTests
{
    [Fact]
    public async Task GetSecret_ShouldReturnApiKey()
    {
        // Arrange
        var apiKey = TestConstants.TestApiKey;
        var auth = new RipeClientAuthApiKey(apiKey);

        // Act
        var secret = await auth.GetSecret();

        // Assert
        secret.Should().Be(apiKey);
    }

    [Fact]
    public async Task ApplyAuthentication_ShouldAddApiKeyAsHeader()
    {
        // Arrange
        var apiKey = TestConstants.TestApiKey;
        var auth = new RipeClientAuthApiKey(apiKey);
        var request = new RestRequest();

        // Act
        await auth.ApplyAuthentication(request);

        // Assert
        request.Parameters.Should().ContainSingle();
        var param = request.Parameters.First();
        param.Name.Should().Be("X-API-Key");
        param.Value.Should().Be(apiKey);
        param.Type.Should().Be(ParameterType.HttpHeader);
    }

    [Fact]
    public void ShouldImplementIRipeClientAuthEnhanced()
    {
        // Arrange & Act
        var auth = new RipeClientAuthApiKey(TestConstants.TestApiKey);

        // Assert
        auth.Should().BeAssignableTo<IRipeClientAuthEnhanced>();
        auth.Should().BeAssignableTo<IRipeClientAuth>();
    }

    [Theory]
    [InlineData("abc123")]
    [InlineData("very-long-api-key-with-dashes-and-numbers-12345678")]
    [InlineData("SHORT")]
    public async Task ApplyAuthentication_ShouldHandleVariousApiKeys(string apiKey)
    {
        // Arrange
        var auth = new RipeClientAuthApiKey(apiKey);
        var request = new RestRequest();

        // Act
        await auth.ApplyAuthentication(request);

        // Assert
        request.Parameters.Should().ContainSingle();
        var param = request.Parameters.First();
        param.Name.Should().Be("X-API-Key");
        param.Value.Should().Be(apiKey);
    }

    [Fact]
    public async Task ApplyAuthentication_HeaderNameShouldBeCorrect()
    {
        // Arrange
        // This test verifies that we use X-API-Key (for RIPE Database)
        // not ncc-api-authorization (which is for LIR/RPKI services)
        var auth = new RipeClientAuthApiKey(TestConstants.TestApiKey);
        var request = new RestRequest();

        // Act
        await auth.ApplyAuthentication(request);

        // Assert
        var header = request.Parameters.FirstOrDefault(p => p.Name == "X-API-Key");
        header.Should().NotBeNull("X-API-Key header should be present");

        var wrongHeader = request.Parameters.FirstOrDefault(p => p.Name == "ncc-api-authorization");
        wrongHeader.Should().BeNull("ncc-api-authorization header should not be present");
    }
}
