using ClientsRipe.LirResources;

namespace ClientsRipe.IntegrationTests.LirResources;

/// <summary>
/// Integration tests for LirResourcesClient using real RIPE NCC LIR Portal API
/// These tests require a valid API key to be set in the LIR_RESOURCES_API_KEY environment variable
/// Tests will be skipped if the API key is not available
/// </summary>
public class LirResourcesClientIntegrationTests
{
    private readonly LirResourcesClient? _client;
    private readonly string? _apiKey;
    private readonly bool _skipTests;

    public LirResourcesClientIntegrationTests()
    {
        _apiKey = Environment.GetEnvironmentVariable("LIR_RESOURCES_API_KEY");

        if (string.IsNullOrEmpty(_apiKey))
        {
            _skipTests = true;
            return;
        }

        _client = new LirResourcesClient(new LirResourcesProductionLocation());
    }

    [Fact]
    public async Task GetIpv4_WithValidApiKey_ShouldReturnAllocationsAndAssignments()
    {
        // Skip if no API key
        if (_skipTests)
        {
            // Test skipped - no API key available
            return;
        }

        // Arrange & Act
        var ipv4 = await _client!.GetIpv4(_apiKey!);

        // Assert
        ipv4.Should().NotBeNull();
        ipv4.Ipv4Allocations.Should().NotBeNull();
        ipv4.Ipv4Assignments.Should().NotBeNull();

        // At least one of allocations or assignments should have data
        var hasData = ipv4.Ipv4Allocations.Any() || ipv4.Ipv4Assignments.Any();
        hasData.Should().BeTrue("Expected at least some IPv4 allocations or assignments from LIR API");
    }

    [Fact]
    public async Task GetIpv6_WithValidApiKey_ShouldReturnAllocations()
    {
        // Skip if no API key
        if (_skipTests)
        {
            // Test skipped - no API key available
            return;
        }

        // Arrange & Act
        var ipv6 = await _client!.GetIpv6(_apiKey!);

        // Assert
        ipv6.Should().NotBeNull();
        ipv6.Ipv6Allocations.Should().NotBeNull();

        // Should have at least some IPv6 allocations
        ipv6.Ipv6Allocations.Should().NotBeEmpty("Expected IPv6 allocations from LIR API");
    }

    [Fact]
    public async Task GetResources_WithValidApiKey_ShouldReturnAllResources()
    {
        // Skip if no API key
        if (_skipTests)
        {
            // Test skipped - no API key available
            return;
        }

        // Arrange & Act
        var resources = await _client!.GetResources(_apiKey!);

        // Assert
        resources.Should().NotBeNull();
        resources.Ipv4Allocations.Should().NotBeNull();
        resources.Ipv4Assignments.Should().NotBeNull();
        resources.Ipv6Allocations.Should().NotBeNull();

        // Should have at least some data
        var hasIpv4 = resources.Ipv4Allocations.Any() || resources.Ipv4Assignments.Any();
        var hasIpv6 = resources.Ipv6Allocations.Any();

        (hasIpv4 || hasIpv6).Should().BeTrue("Expected at least some resources from LIR API");
    }

    [Fact]
    public async Task GetIpv4_ShouldReturnConsistentData()
    {
        // Skip if no API key
        if (_skipTests)
        {
            // Test skipped - no API key available
            return;
        }

        // Arrange & Act
        var ipv4First = await _client!.GetIpv4(_apiKey!);
        var ipv4Second = await _client!.GetIpv4(_apiKey!);

        // Assert - consecutive calls should return same data
        ipv4First.Ipv4Allocations.Count().Should().Be(ipv4Second.Ipv4Allocations.Count());
        ipv4First.Ipv4Assignments.Count().Should().Be(ipv4Second.Ipv4Assignments.Count());
    }

    [Fact]
    public async Task GetIpv6_ShouldReturnConsistentData()
    {
        // Skip if no API key
        if (_skipTests)
        {
            // Test skipped - no API key available
            return;
        }

        // Arrange & Act
        var ipv6First = await _client!.GetIpv6(_apiKey!);
        var ipv6Second = await _client!.GetIpv6(_apiKey!);

        // Assert - consecutive calls should return same data
        ipv6First.Ipv6Allocations.Count().Should().Be(ipv6Second.Ipv6Allocations.Count());
    }

    [Fact]
    public void LirResourcesProductionLocation_ShouldHaveCorrectUrl()
    {
        // Arrange & Act
        var location = new LirResourcesProductionLocation();

        // Assert
        location.Url.Should().NotBeNullOrEmpty();
        location.Url.Should().Contain("ripe.net");
    }

    [Fact]
    public void LirResourcesClient_ShouldBeInstantiable()
    {
        // Arrange & Act
        var location = new LirResourcesProductionLocation();
        var client = new LirResourcesClient(location);

        // Assert
        client.Should().NotBeNull();
        client.Debug.Should().BeFalse(); // Default value
    }

    [Fact]
    public void LirResourcesClient_DebugProperty_ShouldBeSettable()
    {
        // Arrange
        var location = new LirResourcesProductionLocation();
        var client = new LirResourcesClient(location);

        // Act
        client.Debug = true;

        // Assert
        client.Debug.Should().BeTrue();
    }
}
