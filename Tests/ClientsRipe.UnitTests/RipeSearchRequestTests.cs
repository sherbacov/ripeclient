using ClientsRipe.UnitTests.Helpers;

namespace ClientsRipe.UnitTests;

public class RipeSearchRequestTests
{
    [Fact]
    public void DefaultConstructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var request = new RipeSearchRequest();

        // Assert
        request.QueryString.Should().BeNull();
        request.Filter.Should().Be(TypeFilter.None);
        request.Flags.Should().Be(RipeSearchRequestFlags.None);
        request.Sources.Should().Equal("ripe");
    }

    [Fact]
    public void ConstructorWithQueryString_ShouldSetQueryString()
    {
        // Arrange
        var queryString = "192.0.2.0/24";

        // Act
        var request = new RipeSearchRequest(queryString);

        // Assert
        request.QueryString.Should().Be(queryString);
        request.Filter.Should().Be(TypeFilter.None);
        request.Sources.Should().Equal("ripe");
    }

    [Fact]
    public void ConstructorWithQueryStringAndFilter_ShouldSetBothProperties()
    {
        // Arrange
        var queryString = "192.0.2.0/24";
        var filter = TypeFilter.Route | TypeFilter.Route6;

        // Act
        var request = new RipeSearchRequest(queryString, filter);

        // Assert
        request.QueryString.Should().Be(queryString);
        request.Filter.Should().Be(filter);
    }

    [Theory]
    [InlineData(TypeFilter.Route)]
    [InlineData(TypeFilter.Route6)]
    [InlineData(TypeFilter.Inetnum)]
    [InlineData(TypeFilter.Inetnum6)]
    [InlineData(TypeFilter.Autnum)]
    [InlineData(TypeFilter.Person)]
    public void AddFilter_ShouldAddSingleFilter(TypeFilter filter)
    {
        // Arrange
        var request = new RipeSearchRequest();

        // Act
        request.AddFilter(filter);

        // Assert
        request.Filter.Should().HaveFlag(filter);
    }

    [Fact]
    public void AddFilter_ShouldCombineMultipleFilters()
    {
        // Arrange
        var request = new RipeSearchRequest();

        // Act
        request.AddFilter(TypeFilter.Route);
        request.AddFilter(TypeFilter.Route6);
        request.AddFilter(TypeFilter.Inetnum);

        // Assert
        request.Filter.Should().HaveFlag(TypeFilter.Route);
        request.Filter.Should().HaveFlag(TypeFilter.Route6);
        request.Filter.Should().HaveFlag(TypeFilter.Inetnum);
    }

    [Fact]
    public void AddFilter_CalledTwiceWithSameFilter_ShouldBeDempotent()
    {
        // Arrange
        var request = new RipeSearchRequest();

        // Act
        request.AddFilter(TypeFilter.Route);
        request.AddFilter(TypeFilter.Route);

        // Assert
        request.Filter.Should().HaveFlag(TypeFilter.Route);
        request.Filter.Should().Be(TypeFilter.Route);
    }

    [Fact]
    public void GetRequest_ShouldAddQueryStringParameter()
    {
        // Arrange
        var queryString = "192.0.2.0/24";
        var request = new RipeSearchRequest(queryString);

        // Act
        var restRequest = request.GetRequest();

        // Assert
        var param = restRequest.Parameters.FirstOrDefault(p => p.Name == "query-string");
        param.Should().NotBeNull();
        param!.Value.Should().Be(queryString);
    }

    [Fact]
    public void GetRequest_ShouldHaveCorrectResourceAndMethod()
    {
        // Arrange
        var request = new RipeSearchRequest("test");

        // Act
        var restRequest = request.GetRequest();

        // Assert
        restRequest.Resource.Should().Be("search");
        restRequest.Method.Should().Be(Method.Get);
    }

    [Fact]
    public void GetRequest_ShouldAddDefaultRipeSource()
    {
        // Arrange
        var request = new RipeSearchRequest("test");

        // Act
        var restRequest = request.GetRequest();

        // Assert
        var sourceParams = restRequest.Parameters.Where(p => p.Name == "source").ToList();
        sourceParams.Should().ContainSingle();
        sourceParams[0].Value.Should().Be("ripe");
    }

    [Fact]
    public void GetRequest_WithMultipleSources_ShouldAddAllSources()
    {
        // Arrange
        var request = new RipeSearchRequest("test")
        {
            Sources = new[] { "ripe", "apnic", "arin" }
        };

        // Act
        var restRequest = request.GetRequest();

        // Assert
        var sourceParams = restRequest.Parameters.Where(p => p.Name == "source").ToList();
        sourceParams.Should().HaveCount(3);
        sourceParams.Select(p => p.Value).Should().BeEquivalentTo(new[] { "ripe", "apnic", "arin" });
    }

    [Fact]
    public void GetRequest_WithRouteFilter_ShouldAddRouteTypeFilter()
    {
        // Arrange
        var request = new RipeSearchRequest("test");
        request.AddFilter(TypeFilter.Route);

        // Act
        var restRequest = request.GetRequest();

        // Assert
        var typeFilters = restRequest.Parameters.Where(p => p.Name == "type-filter").ToList();
        typeFilters.Should().ContainSingle();
        typeFilters[0].Value.Should().Be("route");
    }

    [Fact]
    public void GetRequest_WithRoute6Filter_ShouldAddRoute6TypeFilter()
    {
        // Arrange
        var request = new RipeSearchRequest("test", TypeFilter.Route6);

        // Act
        var restRequest = request.GetRequest();

        // Assert
        var typeFilters = restRequest.Parameters.Where(p => p.Name == "type-filter").ToList();
        typeFilters.Should().ContainSingle();
        typeFilters[0].Value.Should().Be("route6");
    }

    [Fact]
    public void GetRequest_WithInetnumFilter_ShouldAddInetnumTypeFilter()
    {
        // Arrange
        var request = new RipeSearchRequest("test", TypeFilter.Inetnum);

        // Act
        var restRequest = request.GetRequest();

        // Assert
        var typeFilters = restRequest.Parameters.Where(p => p.Name == "type-filter").ToList();
        typeFilters.Should().ContainSingle();
        typeFilters[0].Value.Should().Be("inetnum");
    }

    [Fact]
    public void GetRequest_WithInetnum6Filter_ShouldAddInet6numTypeFilter()
    {
        // Arrange
        var request = new RipeSearchRequest("test", TypeFilter.Inetnum6);

        // Act
        var restRequest = request.GetRequest();

        // Assert
        var typeFilters = restRequest.Parameters.Where(p => p.Name == "type-filter").ToList();
        typeFilters.Should().ContainSingle();
        typeFilters[0].Value.Should().Be("inetnum6");
    }

    [Fact]
    public void GetRequest_WithAutnumFilter_ShouldAddAutNumTypeFilter()
    {
        // Arrange
        var request = new RipeSearchRequest("test", TypeFilter.Autnum);

        // Act
        var restRequest = request.GetRequest();

        // Assert
        var typeFilters = restRequest.Parameters.Where(p => p.Name == "type-filter").ToList();
        typeFilters.Should().ContainSingle();
        typeFilters[0].Value.Should().Be("aut-num");
    }

    [Fact]
    public void GetRequest_WithMultipleTypeFilters_ShouldAddAllFilters()
    {
        // Arrange
        var request = new RipeSearchRequest("test");
        request.AddFilter(TypeFilter.Route);
        request.AddFilter(TypeFilter.Route6);
        request.AddFilter(TypeFilter.Inetnum);

        // Act
        var restRequest = request.GetRequest();

        // Assert
        var typeFilters = restRequest.Parameters.Where(p => p.Name == "type-filter").ToList();
        typeFilters.Should().HaveCount(3);
        typeFilters.Select(p => p.Value).Should().BeEquivalentTo(new[] { "route", "route6", "inetnum" });
    }

    [Fact]
    public void GetRequest_WithPersonFilter_ShouldNotAddTypeFilter()
    {
        // Arrange
        // Person filter is defined but not implemented in GetRequest
        var request = new RipeSearchRequest("test", TypeFilter.Person);

        // Act
        var restRequest = request.GetRequest();

        // Assert
        var typeFilters = restRequest.Parameters.Where(p => p.Name == "type-filter").ToList();
        typeFilters.Should().BeEmpty();
    }

    [Fact]
    public void GetRequest_WithAllMoreFlag_ShouldAddMFlag()
    {
        // Arrange
        var request = new RipeSearchRequest("192.0.2.0/24")
        {
            Flags = RipeSearchRequestFlags.AllMore
        };

        // Act
        var restRequest = request.GetRequest();

        // Assert
        var flagParams = restRequest.Parameters.Where(p => p.Name == "flags").ToList();
        flagParams.Should().ContainSingle();
        flagParams[0].Value.Should().Be("M");
    }

    [Fact]
    public void GetRequest_WithOneMoreFlag_ShouldAddSmallMFlag()
    {
        // Arrange
        var request = new RipeSearchRequest("192.0.2.0/24")
        {
            Flags = RipeSearchRequestFlags.OneMore
        };

        // Act
        var restRequest = request.GetRequest();

        // Assert
        var flagParams = restRequest.Parameters.Where(p => p.Name == "flags").ToList();
        flagParams.Should().ContainSingle();
        flagParams[0].Value.Should().Be("m");
    }

    [Fact]
    public void GetRequest_WithAllLessFlag_ShouldAddLFlag()
    {
        // Arrange
        var request = new RipeSearchRequest("192.0.2.0/24")
        {
            Flags = RipeSearchRequestFlags.AllLess
        };

        // Act
        var restRequest = request.GetRequest();

        // Assert
        var flagParams = restRequest.Parameters.Where(p => p.Name == "flags").ToList();
        flagParams.Should().ContainSingle();
        flagParams[0].Value.Should().Be("L");
    }

    [Fact]
    public void GetRequest_WithOneLessFlag_ShouldAddSmallLFlag()
    {
        // Arrange
        var request = new RipeSearchRequest("192.0.2.0/24")
        {
            Flags = RipeSearchRequestFlags.OneLess
        };

        // Act
        var restRequest = request.GetRequest();

        // Assert
        var flagParams = restRequest.Parameters.Where(p => p.Name == "flags").ToList();
        flagParams.Should().ContainSingle();
        flagParams[0].Value.Should().Be("l");
    }

    [Fact]
    public void GetRequest_WithNoFlags_ShouldNotAddFlagsParameter()
    {
        // Arrange
        var request = new RipeSearchRequest("192.0.2.0/24")
        {
            Flags = RipeSearchRequestFlags.None
        };

        // Act
        var restRequest = request.GetRequest();

        // Assert
        var flagParams = restRequest.Parameters.Where(p => p.Name == "flags").ToList();
        flagParams.Should().BeEmpty();
    }

    [Fact]
    public void GetRequest_ComplexScenario_ShouldIncludeAllParameters()
    {
        // Arrange
        var request = new RipeSearchRequest("192.0.2.0/24")
        {
            Sources = new[] { "ripe", "apnic" },
            Flags = RipeSearchRequestFlags.AllMore
        };
        request.AddFilter(TypeFilter.Route);
        request.AddFilter(TypeFilter.Route6);

        // Act
        var restRequest = request.GetRequest();

        // Assert
        restRequest.Parameters.Should().Contain(p => p.Name == "query-string" && (string)p.Value! == "192.0.2.0/24");

        var sources = restRequest.Parameters.Where(p => p.Name == "source").Select(p => p.Value).ToList();
        sources.Should().BeEquivalentTo(new[] { "ripe", "apnic" });

        var typeFilters = restRequest.Parameters.Where(p => p.Name == "type-filter").Select(p => p.Value).ToList();
        typeFilters.Should().BeEquivalentTo(new[] { "route", "route6" });

        var flags = restRequest.Parameters.Where(p => p.Name == "flags").Select(p => p.Value).ToList();
        flags.Should().ContainSingle().Which.Should().Be("M");
    }

    [Fact]
    public void TypeFilter_ShouldSupportBitwiseOperations()
    {
        // Arrange & Act
        var combined = TypeFilter.Route | TypeFilter.Route6 | TypeFilter.Inetnum;

        // Assert
        combined.Should().HaveFlag(TypeFilter.Route);
        combined.Should().HaveFlag(TypeFilter.Route6);
        combined.Should().HaveFlag(TypeFilter.Inetnum);
        combined.Should().NotHaveFlag(TypeFilter.Autnum);
    }

    [Fact]
    public void IRipeSearchRequest_ShouldBeImplemented()
    {
        // Arrange & Act
        var request = new RipeSearchRequest();

        // Assert
        request.Should().BeAssignableTo<IRipeSearchRequest>();
    }
}
