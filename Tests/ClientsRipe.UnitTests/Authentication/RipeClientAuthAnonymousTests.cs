using ClientsRipe.UnitTests.Helpers;

namespace ClientsRipe.UnitTests.Authentication;

public class RipeClientAuthAnonymousTests
{
    [Fact]
    public async Task GetSecret_ShouldReturnEmptyString()
    {
        // Arrange
        var auth = new RipeClientAuthAnonymous();

        // Act
        var secret = await auth.GetSecret();

        // Assert
        secret.Should().BeEmpty();
    }

    [Fact]
    public async Task ApplyAuthentication_ShouldNotAddAnyParameters()
    {
        // Arrange
        var auth = new RipeClientAuthAnonymous();
        var request = new RestRequest();

        // Act
        await auth.ApplyAuthentication(request);

        // Assert
        request.Parameters.Should().BeEmpty();
    }

    [Fact]
    public void ShouldImplementIRipeClientAuthEnhanced()
    {
        // Arrange & Act
        var auth = new RipeClientAuthAnonymous();

        // Assert
        auth.Should().BeAssignableTo<IRipeClientAuthEnhanced>();
        auth.Should().BeAssignableTo<IRipeClientAuth>();
    }
}
