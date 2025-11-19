namespace ClientsRipe;

public interface IRipeLocation
{
    public string Url { get; set; }
}

public class RipeNonSecureLocation : IRipeLocation
{
    public string Url { get; set; } = "http://rest.db.ripe.net/";
}

public class RipeSecureLocation : IRipeLocation
{
    public string Url { get; set; } = "https://rest.db.ripe.net/";
}

/// <summary>
/// RIPE Test Database location for testing purposes
/// Use this location for integration tests and development
/// The test database is reset daily and allows create/update/delete operations
/// </summary>
public class RipeTestLocation : IRipeLocation
{
    public string Url { get; set; } = "https://rest-test.db.ripe.net/";
}