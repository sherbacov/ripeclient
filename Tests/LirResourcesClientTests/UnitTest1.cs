using ClientsRipe.LirResources;

namespace LirResourcesClientTests;

public class LirResourcesClientIntegrationUnitTests
{
    private readonly LirResourcesClient _client;

    private readonly string? _apiKey;
    
    public LirResourcesClientIntegrationUnitTests()
    {
        _apiKey = Environment.GetEnvironmentVariable("LIR_RESOURCES_API_KEY");

        if (_apiKey == null)
            throw new ArgumentException("LIR_RESOURCES_API_KEY is not provided for tests");
        
        _client = new LirResourcesClient(new LirResourcesProductionLocation());

        _client.Debug = true;
    }

    private string? GetApiKey()
    {
        return _apiKey;
    }
    
    [Fact]
    public async Task RequestIpv4()
    {
        var ipv4 = await _client.GetIpv4(GetApiKey(), CancellationToken.None);
        Assert.True(ipv4.Ipv4Allocations.Any(), "No Ipv4 PA networks from LIR API.");
        Assert.True(ipv4.Ipv4Assignments.Any(), "No Ipv4 PI networks from LIR API.");
    }
    
    [Fact]
    public async Task RequestIpv6()
    {
        var ipv6 = await _client.GetIpv6(GetApiKey(), CancellationToken.None);
        Assert.True(ipv6.Ipv6Allocations.Any(), "No Ipv6 networks from LIR API.");
    }
    
    [Fact]
    public async Task RequestAll()
    {
        var resources = await _client.GetResources(GetApiKey(), CancellationToken.None);
        
        Assert.True(resources.Ipv4Allocations.Any(), "No Ipv4 PA networks from LIR API.");
        Assert.True(resources.Ipv4Assignments.Any(), "No Ipv4 PI networks from LIR API.");
        Assert.True(resources.Ipv6Allocations.Any(), "No Ipv6 networks from LIR API.");
    }
}



