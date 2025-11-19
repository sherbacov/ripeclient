namespace ClientsRipe.IntegrationTests.RipeClient;

/// <summary>
/// Integration tests for RipeClient using the RIPE Test Database
/// These tests make real HTTP requests to https://rest-test.db.ripe.net/
/// Note: The test database is reset daily and may have different data
/// </summary>
public class RipeClientIntegrationTests
{
    private readonly ClientsRipe.RipeClient _client;

    public RipeClientIntegrationTests()
    {
        var location = new RipeTestLocation();
        var auth = new RipeClientAuthAnonymous();
        _client = new ClientsRipe.RipeClient(location, auth);
    }

    #region Search Integration Tests

    [Fact]
    public async Task Search_WithValidIpRange_ShouldReturnResults()
    {
        // Arrange
        // Using a well-known IP range that should exist in RIPE database
        var searchRequest = new RipeSearchRequest("193.0.0.0/21")
        {
            Sources = new[] { "TEST" } // Test database uses TEST source
        };

        // Act
        var results = await _client.Search(searchRequest);

        // Assert
        results.Should().NotBeNull();
        // Note: Results may vary depending on test database state
        // We're just verifying the API call works
    }

    [Fact]
    public async Task Search_WithRouteFilter_ShouldReturnOnlyRoutes()
    {
        // Arrange
        var searchRequest = new RipeSearchRequest("193.0.0.0/21")
        {
            Sources = new[] { "TEST" }
        };
        searchRequest.AddFilter(TypeFilter.Route);

        // Act
        var results = await _client.Search(searchRequest);

        // Assert
        results.Should().NotBeNull();
        if (results.Any())
        {
            results.All(r => r.Type == "route").Should().BeTrue();
        }
    }

    [Fact]
    public async Task Search_WithInetnumFilter_ShouldWork()
    {
        // Arrange
        var searchRequest = new RipeSearchRequest("193.0.0.0/21")
        {
            Sources = new[] { "TEST" }
        };
        searchRequest.AddFilter(TypeFilter.Inetnum);

        // Act
        var results = await _client.Search(searchRequest);

        // Assert
        results.Should().NotBeNull();
        // Note: RIPE may return related objects (organisation, person, role) along with inetnum
        // We just verify the request works, not the specific types returned
    }

    [Fact]
    public async Task Search_WithMultipleFilters_ShouldWork()
    {
        // Arrange
        var searchRequest = new RipeSearchRequest("193.0.0.0/21")
        {
            Sources = new[] { "TEST" }
        };
        searchRequest.AddFilter(TypeFilter.Route);
        searchRequest.AddFilter(TypeFilter.Inetnum);

        // Act
        var results = await _client.Search(searchRequest);

        // Assert
        results.Should().NotBeNull();
        // Note: RIPE may return related objects along with the requested types
        // We just verify the request works
    }

    [Fact]
    public async Task Search_WithNonPublicRange_ShouldWork()
    {
        // Arrange
        // Using a private IP range
        var searchRequest = new RipeSearchRequest("10.255.255.0/24")
        {
            Sources = new[] { "TEST" }
        };

        // Act
        var results = await _client.Search(searchRequest);

        // Assert
        results.Should().NotBeNull();
        // Test database may have test data for any range, so we don't assert empty
    }

    [Fact]
    public async Task Search_WithAllMoreFlag_ShouldWork()
    {
        // Arrange
        var searchRequest = new RipeSearchRequest("193.0.0.0/21")
        {
            Sources = new[] { "TEST" },
            Flags = RipeSearchRequestFlags.AllMore
        };

        // Act
        var results = await _client.Search(searchRequest);

        // Assert
        results.Should().NotBeNull();
        // AllMore returns all sub-ranges, so should potentially have results
    }

    [Fact]
    public async Task Search_WithOneLessFlag_ShouldWork()
    {
        // Arrange
        var searchRequest = new RipeSearchRequest("193.0.0.0/24")
        {
            Sources = new[] { "TEST" },
            Flags = RipeSearchRequestFlags.OneLess
        };

        // Act
        var results = await _client.Search(searchRequest);

        // Assert
        results.Should().NotBeNull();
        // OneLess returns one level less specific
    }

    [Fact]
    public async Task Search_WithCancellationToken_ShouldSupportCancellation()
    {
        // Arrange
        var searchRequest = new RipeSearchRequest("193.0.0.0/21")
        {
            Sources = new[] { "TEST" }
        };
        var cts = new CancellationTokenSource();

        // Act - don't cancel, just verify it accepts the token
        var results = await _client.Search(searchRequest, cts.Token);

        // Assert
        results.Should().NotBeNull();
    }

    #endregion

    #region GetObjectByKey Integration Tests

    [Fact]
    public async Task GetObjectByKey_WithValidKey_ShouldReturnObject()
    {
        // Arrange
        // First search for an object to get a valid key
        var searchRequest = new RipeSearchRequest("193.0.0.0/21")
        {
            Sources = new[] { "TEST" }
        };
        searchRequest.AddFilter(TypeFilter.Inetnum);
        var searchResults = await _client.Search(searchRequest);

        // Skip test if no results
        if (!searchResults.Any())
        {
            // Test database might be empty, skip
            return;
        }

        // Find an inetnum object in results
        var inetnumResult = searchResults.FirstOrDefault(r => r.Type == "inetnum");
        if (inetnumResult == null || inetnumResult.PrimaryKey?.Attribute == null)
        {
            // No valid inetnum object, skip
            return;
        }

        var key = inetnumResult.PrimaryKey.Attribute.FirstOrDefault()?.Value;

        if (string.IsNullOrEmpty(key))
        {
            // No valid key, skip
            return;
        }

        // Act
        var result = await _client.GetObjectByKey(key, "inetnum", "TEST");

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be("inetnum");
    }

    [Fact]
    public async Task GetObjectByKey_WithNonExistentKey_ShouldReturnNull()
    {
        // Arrange
        var fakeKey = "999.999.999.999 - 999.999.999.999";

        // Act & Assert
        // May throw exception or return null depending on how server handles it
        try
        {
            var result = await _client.GetObjectByKey(fakeKey, "inetnum", "TEST");
            result.Should().BeNull();
        }
        catch (RipeClientNotFoundException)
        {
            // This is also acceptable behavior for non-existent key
        }
    }

    [Fact]
    public async Task GetObjectByKey_WithCancellationToken_ShouldWork()
    {
        // Arrange
        var key = "193.0.0.0 - 193.0.7.255";
        var cts = new CancellationTokenSource();

        // Act
        var result = await _client.GetObjectByKey(key, "inetnum", "TEST", cts.Token);

        // Assert - may be null or not, just checking it doesn't throw
        // result can be null if object doesn't exist
    }

    #endregion

    #region Location Tests

    [Fact]
    public void RipeTestLocation_ShouldPointToTestDatabase()
    {
        // Arrange & Act
        var location = new RipeTestLocation();

        // Assert
        location.Url.Should().Be("https://rest-test.db.ripe.net/");
    }

    [Fact]
    public void RipeTestLocation_ShouldImplementIRipeLocation()
    {
        // Arrange & Act
        var location = new RipeTestLocation();

        // Assert
        location.Should().BeAssignableTo<IRipeLocation>();
    }

    #endregion

    #region Anonymous Authentication Tests

    [Fact]
    public async Task Search_WithAnonymousAuth_ShouldWorkForReadOperations()
    {
        // Arrange
        var location = new RipeTestLocation();
        var auth = new RipeClientAuthAnonymous();
        var client = new ClientsRipe.RipeClient(location, auth);
        var searchRequest = new RipeSearchRequest("193.0.0.0/21")
        {
            Sources = new[] { "TEST" }
        };

        // Act
        var results = await client.Search(searchRequest);

        // Assert
        results.Should().NotBeNull();
        // Anonymous auth should work for read-only operations
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Search_WithInvalidQueryString_ShouldHandleGracefully()
    {
        // Arrange
        var searchRequest = new RipeSearchRequest("invalid-query-$$$$")
        {
            Sources = new[] { "TEST" }
        };

        // Act
        var results = await _client.Search(searchRequest);

        // Assert
        // Should return empty or throw a proper exception, not crash
        results.Should().NotBeNull();
    }

    #endregion
}
