using ClientsRipe.UnitTests.Helpers;

namespace ClientsRipe.UnitTests.Authentication;

public class RipeClientAuthPasswordTests
{
    [Fact]
    public async Task GetSecret_ShouldReturnPassword()
    {
        // Arrange
        var password = TestConstants.TestPassword;
        var auth = new RipeClientAuthPassword(password);

        // Act
        var secret = await auth.GetSecret();

        // Assert
        secret.Should().Be(password);
    }

    [Fact]
    public async Task ApplyAuthentication_ShouldAddPasswordAsQueryParameter()
    {
        // Arrange
        var password = TestConstants.TestPassword;
        var auth = new RipeClientAuthPassword(password);
        var request = new RestRequest();

        // Act
        await auth.ApplyAuthentication(request);

        // Assert
        request.Parameters.Should().ContainSingle();
        var param = request.Parameters.First();
        param.Name.Should().Be("password");
        param.Value.Should().Be(password);
        param.Type.Should().Be(ParameterType.QueryString);
    }

    [Fact]
    public void ShouldImplementIRipeClientAuthEnhanced()
    {
        // Arrange & Act
        var auth = new RipeClientAuthPassword(TestConstants.TestPassword);

        // Assert
        auth.Should().BeAssignableTo<IRipeClientAuthEnhanced>();
        auth.Should().BeAssignableTo<IRipeClientAuth>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("simple-password")]
    [InlineData("complex-P@ssw0rd!#")]
    public async Task ApplyAuthentication_ShouldHandleVariousPasswords(string password)
    {
        // Arrange
        var auth = new RipeClientAuthPassword(password);
        var request = new RestRequest();

        // Act
        await auth.ApplyAuthentication(request);

        // Assert
        request.Parameters.Should().ContainSingle();
        request.Parameters.First().Value.Should().Be(password);
    }
}
