using ClientsRipe.LirResources;
using Xunit;

namespace LirResourcesClientTests;

public class LirResourcesClientIntegrationUnitTests
{
    private readonly LirResourcesClient? _client;

    private readonly string? _apiKey;
    
    public LirResourcesClientIntegrationUnitTests()
    {
        _apiKey = Environment.GetEnvironmentVariable("LIR_RESOURCES_API_KEY");

        if (!string.IsNullOrWhiteSpace(_apiKey))
        {
            _client = new LirResourcesClient(new LirResourcesProductionLocation());
            _client.Debug = true;
        }
    }

    [Fact]
    public async Task RequestIpv4()
    {
        if (string.IsNullOrWhiteSpace(_apiKey) || _client == null)
        {
            // Skip test if API key is not provided
            return;
        }

        var ipv4 = await _client.GetIpv4(_apiKey);
        Assert.True(ipv4.Ipv4Allocations.Any(), "No Ipv4 PA networks from LIR API.");
        Assert.True(ipv4.Ipv4Assignments.Any(), "No Ipv4 PI networks from LIR API.");
    }
    
    [Fact]
    public async Task RequestIpv6()
    {
        if (string.IsNullOrWhiteSpace(_apiKey) || _client == null)
        {
            // Skip test if API key is not provided
            return;
        }

        var ipv6 = await _client.GetIpv6(_apiKey);
        Assert.True(ipv6.Ipv6Allocations.Any(), "No Ipv6 networks from LIR API.");
    }
    
    [Fact]
    public async Task RequestAll()
    {
        if (string.IsNullOrWhiteSpace(_apiKey) || _client == null)
        {
            // Skip test if API key is not provided
            return;
        }

        var resources = await _client.GetResources(_apiKey);

        Assert.True(resources.Ipv4Allocations.Any(), "No Ipv4 PA networks from LIR API.");
        Assert.True(resources.Ipv4Assignments.Any(), "No Ipv4 PI networks from LIR API.");
        Assert.True(resources.Ipv6Allocations.Any(), "No Ipv6 networks from LIR API.");
    }
}



