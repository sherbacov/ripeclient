using ClientsRpki;
using ClientsRipe.RpkiClient.Models;

namespace ClientsRipe.IntegrationTests.Rpki;

/// <summary>
/// Integration tests for RipeRpkiClient using RIPE RPKI API
/// These tests require a valid API key to be set in the RPKI_API_KEY environment variable
/// Tests will be skipped if the API key is not available
/// </summary>
public class RipeRpkiClientIntegrationTests
{
    private readonly RipeRpkiClient? _client;
    private readonly string? _apiKey;
    private readonly bool _skipTests;

    public RipeRpkiClientIntegrationTests()
    {
        _apiKey = Environment.GetEnvironmentVariable("RPKI_API_KEY");

        if (string.IsNullOrEmpty(_apiKey))
        {
            _skipTests = true;
            return;
        }

        // Use test location for integration tests
        _client = new RipeRpkiClient(new RipeRpkiTestLocation());
    }

    [Fact]
    public async Task GetResourcesAsync_WithValidApiKey_ShouldReturnResources()
    {
        // Skip if no API key
        if (_skipTests)
        {
            // Test skipped - no API key available
            return;
        }

        // Arrange & Act
        var resources = await _client!.GetResourcesAsync(_apiKey!);

        // Assert
        resources.Should().NotBeNull();
        resources.Resources.Should().NotBeNull();
    }

    [Fact]
    public async Task GetRoasAsync_WithValidApiKey_ShouldReturnRoas()
    {
        // Skip if no API key
        if (_skipTests)
        {
            // Test skipped - no API key available
            return;
        }

        // Arrange & Act
        var roas = await _client!.GetRoasAsync(_apiKey!);

        // Assert
        roas.Should().NotBeNull();
        // ROAs collection may be empty if no ROAs are configured
    }

    [Fact]
    public void GetResourcesSync_OnInterface_ShouldBeMarkedAsObsolete()
    {
        // Arrange & Act
        // Obsolete attribute is on the interface method
        var method = typeof(IRipeRpkiClient).GetMethod("GetResources");

        // Assert
        method.Should().NotBeNull();
        method!.Should().BeDecoratedWith<ObsoleteAttribute>();

        var obsoleteAttr = method.GetCustomAttributes(typeof(ObsoleteAttribute), false)
            .Cast<ObsoleteAttribute>()
            .FirstOrDefault();

        obsoleteAttr.Should().NotBeNull();
        obsoleteAttr!.Message.Should().Contain("GetResourcesAsync");
        obsoleteAttr.Message.Should().Contain("sync-over-async");
    }

    [Fact]
    public void GetRoasSync_OnInterface_ShouldBeMarkedAsObsolete()
    {
        // Arrange & Act
        // Obsolete attribute is on the interface method
        var method = typeof(IRipeRpkiClient).GetMethod("GetRoas");

        // Assert
        method.Should().NotBeNull();
        method!.Should().BeDecoratedWith<ObsoleteAttribute>();

        var obsoleteAttr = method.GetCustomAttributes(typeof(ObsoleteAttribute), false)
            .Cast<ObsoleteAttribute>()
            .FirstOrDefault();

        obsoleteAttr.Should().NotBeNull();
        obsoleteAttr!.Message.Should().Contain("GetRoasAsync");
    }

    [Fact]
    public void RipeRpkiTestLocation_ShouldHaveCorrectUrl()
    {
        // Arrange & Act
        var location = new RipeRpkiTestLocation();

        // Assert
        location.Url.Should().Be("https://localcert.ripe.net/api/rpki");
    }

    [Fact]
    public void RipeRpkiProductionLocation_ShouldHaveCorrectUrl()
    {
        // Arrange & Act
        var location = new RipeRpkiProductionLocation();

        // Assert
        location.Url.Should().Be("https://my.ripe.net/api/rpki");
    }

    [Fact]
    public void RipeRpkiClient_ShouldBeInstantiable()
    {
        // Arrange & Act
        var location = new RipeRpkiTestLocation();
        var client = new RipeRpkiClient(location);

        // Assert
        client.Should().NotBeNull();
        client.Debug.Should().BeFalse(); // Default value
    }

    [Fact]
    public void RipeRpkiClient_DebugProperty_ShouldBeSettable()
    {
        // Arrange
        var location = new RipeRpkiTestLocation();
        var client = new RipeRpkiClient(location);

        // Act
        client.Debug = true;

        // Assert
        client.Debug.Should().BeTrue();
    }

    [Fact]
    public void RipeRpkiClient_ShouldImplementInterface()
    {
        // Arrange
        var location = new RipeRpkiTestLocation();

        // Act
        var client = new RipeRpkiClient(location);

        // Assert
        client.Should().BeAssignableTo<IRipeRpkiClient>();
    }

    [Fact]
    public async Task RpkiOperationAdd_WithNullApiKey_ShouldThrowArgumentException()
    {
        // Arrange
        var location = new RipeRpkiTestLocation();
        var client = new RipeRpkiClient(location);
        var operation = new PublishRpkiRoaPlain
        {
            Prefix = "192.0.2.0/24",
            Asn = "AS65001",
            MaximalLength = "24"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await client.RpkiOperationAdd(null!, operation)
        );
    }

    [Fact]
    public async Task RpkiOperationDelete_WithNullApiKey_ShouldThrowArgumentException()
    {
        // Arrange
        var location = new RipeRpkiTestLocation();
        var client = new RipeRpkiClient(location);
        var operation = new PublishRpkiRoaPlain
        {
            Prefix = "192.0.2.0/24",
            Asn = "AS65001",
            MaximalLength = "24"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await client.RpkiOperationDelete(null!, operation)
        );
    }

    [Fact]
    public async Task GetResourcesAsync_ShouldReturnConsistentData()
    {
        // Skip if no API key
        if (_skipTests)
        {
            // Test skipped - no API key available
            return;
        }

        // Arrange & Act
        var resourcesFirst = await _client!.GetResourcesAsync(_apiKey!);
        var resourcesSecond = await _client!.GetResourcesAsync(_apiKey!);

        // Assert - consecutive calls should return same data
        resourcesFirst.Resources.Should().BeEquivalentTo(resourcesSecond.Resources);
    }

    [Fact]
    public void RpkiOperations_ShouldAllowFluentApi()
    {
        // Arrange
        var operation1 = new PublishRpkiRoaPlain
        {
            Prefix = "192.0.2.0/24",
            Asn = "AS65001",
            MaximalLength = "24"
        };

        var operation2 = new PublishRpkiRoaPlain
        {
            Prefix = "198.51.100.0/24",
            Asn = "AS65002",
            MaximalLength = "24"
        };

        // Act
        var operations = new RpkiOperations()
            .Add(operation1)
            .Delete(operation2);

        // Assert
        operations.Should().NotBeNull();
        // RpkiOperations should support fluent API pattern
    }
}
