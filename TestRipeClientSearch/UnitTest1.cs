using ClientsRipe;
using TypeFilter = RipeDatabaseObjects.TypeFilter;

namespace TestRipeClientSearch;

public class Tests
{
    private RipeClient _client;
    
    [SetUp]
    public void Setup()
    {
        _client = new RipeClient(new RipeSecureLocation(), new RipeClientAuthAnonymous());
    }

    [Test]
    public void Test1()
    {
        var req = new RipeSearchRequest
        {
            QueryString = "5.181.20.0/22"
        };
        req.AddFilter(ClientsRipe.TypeFilter.Route);
        req.AddFilter(ClientsRipe.TypeFilter.Route6);
        req.Flags = RipeSearchRequestFlags.AllMore;

        var result = _client.SearchSync(req);
        
        Assert.Pass();
    }
}