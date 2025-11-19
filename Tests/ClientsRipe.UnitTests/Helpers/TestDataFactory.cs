using RipeDatabaseObjects;

namespace ClientsRipe.UnitTests.Helpers;

/// <summary>
/// Factory for creating test data objects
/// </summary>
public static class TestDataFactory
{
    /// <summary>
    /// Creates a sample RipeObjects search response with one inetnum
    /// </summary>
    public static RipeObjects CreateSearchResponse(string prefix = TestConstants.TestIpV4Range)
    {
        return new RipeObjects
        {
            Objects = new Objects
            {
                Object = new List<DatabaseObject>
                {
                    CreateInetnumObject(prefix)
                }
            }
        };
    }

    /// <summary>
    /// Creates a sample inetnum DatabaseObject
    /// </summary>
    public static DatabaseObject CreateInetnumObject(string prefix = TestConstants.TestIpV4Range)
    {
        return new DatabaseObject
        {
            Type = "inetnum",
            PrimaryKey = new PrimaryKey { Attribute = new List<RipeDatabaseObjects.Attribute>
            {
                new() { Name = "inetnum", Value = prefix }
            }},
            Attributes = new Attributes
            {
                Attribute = new List<RipeDatabaseObjects.Attribute>
                {
                    new() { Name = "inetnum", Value = prefix },
                    new() { Name = "netname", Value = "TEST-NET" },
                    new() { Name = "descr", Value = "Test network" },
                    new() { Name = "country", Value = "NL" },
                    new() { Name = "admin-c", Value = TestConstants.TestPersonNicHdl },
                    new() { Name = "tech-c", Value = TestConstants.TestPersonNicHdl },
                    new() { Name = "status", Value = "ASSIGNED PA" },
                    new() { Name = "mnt-by", Value = TestConstants.TestMaintainer },
                    new() { Name = "source", Value = TestConstants.TestSource }
                }
            }
        };
    }

    /// <summary>
    /// Creates a sample route DatabaseObject
    /// </summary>
    public static DatabaseObject CreateRouteObject(string prefix = TestConstants.TestRoutePrefix, string origin = TestConstants.TestRouteOrigin)
    {
        return new DatabaseObject
        {
            Type = "route",
            PrimaryKey = new PrimaryKey { Attribute = new List<RipeDatabaseObjects.Attribute>
            {
                new() { Name = "route", Value = prefix },
                new() { Name = "origin", Value = origin }
            }},
            Attributes = new Attributes
            {
                Attribute = new List<RipeDatabaseObjects.Attribute>
                {
                    new() { Name = "route", Value = prefix },
                    new() { Name = "descr", Value = "Test route" },
                    new() { Name = "origin", Value = origin },
                    new() { Name = "mnt-by", Value = TestConstants.TestMaintainer },
                    new() { Name = "source", Value = TestConstants.TestSource }
                }
            }
        };
    }

    /// <summary>
    /// Creates an empty RipeObjects response (for 404 scenarios)
    /// </summary>
    public static RipeObjects CreateEmptyResponse()
    {
        return new RipeObjects
        {
            Objects = new Objects
            {
                Object = new List<DatabaseObject>()
            }
        };
    }

    /// <summary>
    /// Creates an error response with ErrorMessages
    /// </summary>
    public static RipeObjects CreateErrorResponse(string errorMessage = "Test error")
    {
        return new RipeObjects
        {
            ErrorMessages = new ErrorMessages
            {
                ErrorMessage = new List<ErrorMessage>
                {
                    new() { Text = errorMessage, Severity = "Error" }
                }
            }
        };
    }

    /// <summary>
    /// Creates a RipeObject for testing (Route)
    /// </summary>
    public static RipeObject CreateRipeRoute(string prefix = TestConstants.TestRoutePrefix, string origin = TestConstants.TestRouteOrigin)
    {
        var route = new RipeObject();
        route.Add("route", prefix);
        route.Add("descr", "Test route description");
        route.Add("origin", origin);
        route.Add("mnt-by", TestConstants.TestMaintainer);
        route.Add("source", TestConstants.TestSource);
        return route;
    }

    /// <summary>
    /// Creates a WhoisResources XML request object
    /// </summary>
    public static WhoisResources CreateWhoisResourcesRequest(RipeObject ripeObject)
    {
        var whoisResource = new WhoisResources
        {
            Objects = new WhoisObjects
            {
                Object = new List<DatabaseWhoisObject>
                {
                    new()
                    {
                        Type = ripeObject.GetRipeObjectType(),
                        Source = new Source { Id = ripeObject["source"] },
                        Attributes = new WhoisAttributes
                        {
                            Attribute = ripeObject.Select(kvp => new RipeDatabaseObjects.Attribute
                            {
                                Name = kvp.Key,
                                Value = kvp.Value
                            }).ToList()
                        }
                    }
                }
            }
        };
        return whoisResource;
    }
}
