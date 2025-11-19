using ClientsRipe.UnitTests.Helpers;
using System.Text;

namespace ClientsRipe.UnitTests.Helpers;

public class RipeClientAuthBasicTests
{
    [Fact]
    public async Task GetSecret_ShouldReturnBase64EncodedCredentials()
    {
        // Arrange
        var username = TestConstants.TestUsername;
        var password = TestConstants.TestPassword;
        var auth = new RipeClientAuthBasic(username, password);

        var expectedEncoded = Convert.ToBase64String(
            Encoding.ASCII.GetBytes($"{username}:{password}")
        );

        // Act
        var secret = await auth.GetSecret();

        // Assert
        secret.Should().Be(expectedEncoded);
    }

    [Fact]
    public async Task ApplyAuthentication_ShouldAddAuthorizationHeader()
    {
        // Arrange
        var username = TestConstants.TestUsername;
        var password = TestConstants.TestPassword;
        var auth = new RipeClientAuthBasic(username, password);
        var request = new RestRequest();

        var expectedEncoded = Convert.ToBase64String(
            Encoding.ASCII.GetBytes($"{username}:{password}")
        );

        // Act
        await auth.ApplyAuthentication(request);

        // Assert
        request.Parameters.Should().ContainSingle();
        var param = request.Parameters.First();
        param.Name.Should().Be("Authorization");
        param.Value.Should().Be($"Basic {expectedEncoded}");
        param.Type.Should().Be(ParameterType.HttpHeader);
    }

    [Fact]
    public void ShouldImplementIRipeClientAuthEnhanced()
    {
        // Arrange & Act
        var auth = new RipeClientAuthBasic(TestConstants.TestUsername, TestConstants.TestPassword);

        // Assert
        auth.Should().BeAssignableTo<IRipeClientAuthEnhanced>();
        auth.Should().BeAssignableTo<IRipeClientAuth>();
    }

    [Theory]
    [InlineData("user1", "pass1")]
    [InlineData("admin@example.com", "complex-P@ss!")]
    [InlineData("simple", "123")]
    public async Task ApplyAuthentication_ShouldHandleVariousCredentials(string username, string password)
    {
        // Arrange
        var auth = new RipeClientAuthBasic(username, password);
        var request = new RestRequest();

        var expectedEncoded = Convert.ToBase64String(
            Encoding.ASCII.GetBytes($"{username}:{password}")
        );

        // Act
        await auth.ApplyAuthentication(request);

        // Assert
        var param = request.Parameters.First();
        param.Value.Should().Be($"Basic {expectedEncoded}");
    }

    [Fact]
    public async Task ApplyAuthentication_ShouldUseBasicScheme()
    {
        // Arrange
        var auth = new RipeClientAuthBasic(TestConstants.TestUsername, TestConstants.TestPassword);
        var request = new RestRequest();

        // Act
        await auth.ApplyAuthentication(request);

        // Assert
        var authHeader = request.Parameters.First();
        authHeader.Value.ToString().Should().StartWith("Basic ");
    }

    [Fact]
    public async Task GetSecret_ShouldProduceValidBase64()
    {
        // Arrange
        var username = "testuser";
        var password = "testpass";
        var auth = new RipeClientAuthBasic(username, password);

        // Act
        var secret = await auth.GetSecret();

        // Assert
        // Should be able to decode back
        var decoded = Encoding.ASCII.GetString(Convert.FromBase64String(secret));
        decoded.Should().Be($"{username}:{password}");
    }
}
