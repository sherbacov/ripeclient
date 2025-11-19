namespace ClientsRipe.UnitTests.Helpers;

/// <summary>
/// Common constants used across unit tests
/// </summary>
public static class TestConstants
{
    // Test IP ranges (documentation ranges from RFC 5737)
    public const string TestIpV4Range = "192.0.2.0/24";
    public const string TestIpV4Range2 = "198.51.100.0/24";
    public const string TestIpV6Range = "2001:db8::/32";

    // Test ASN
    public const string TestAsn = "AS64512";

    // Test authentication data
    public const string TestPassword = "test-password-123";
    public const string TestApiKey = "test-api-key-abc123";
    public const string TestUsername = "test-user";
    public const string TestBasicAuthEncoded = "dGVzdC11c2VyOnRlc3QtcGFzc3dvcmQtMTIz"; // base64(test-user:test-password-123)

    // Test source
    public const string TestSource = "RIPE";
    public const string TestSourceTest = "TEST-RIPE";

    // Test maintainer
    public const string TestMaintainer = "TEST-MNT";

    // Test person
    public const string TestPersonNicHdl = "TEST-RIPE";
    public const string TestPersonName = "Test Person";

    // Test route
    public const string TestRoutePrefix = "192.0.2.0/24";
    public const string TestRouteOrigin = "AS64512";

    // RIPE endpoints
    public const string RipeProductionUrl = "https://rest.db.ripe.net/";
    public const string RipeTestUrl = "https://rest-test.db.ripe.net/";
    public const string LirResourcesUrl = "https://lirportal.ripe.net/myresources/v1/resources";
    public const string RpkiProductionUrl = "https://my.ripe.net/api/rpki";
    public const string RpkiTestUrl = "https://localcert.ripe.net/api/rpki";
}
