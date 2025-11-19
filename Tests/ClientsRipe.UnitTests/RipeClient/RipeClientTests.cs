using ClientsRipe.UnitTests.Helpers;
using System.Net;
using RipeDatabaseObjects;

namespace ClientsRipe.UnitTests.RipeClient;

public class RipeClientTests
{
    private Mock<IRipeLocation> CreateMockLocation()
    {
        var mockLocation = new Mock<IRipeLocation>();
        mockLocation.Setup(l => l.Url).Returns(TestConstants.RipeTestUrl);
        return mockLocation;
    }

    #region Search Tests

    [Fact]
    public void Search_Method_ShouldExistAndReturnCorrectType()
    {
        // Arrange & Act
        var searchMethod = typeof(ClientsRipe.RipeClient).GetMethod("Search");

        // Assert
        searchMethod.Should().NotBeNull();
        searchMethod!.ReturnType.Should().Be(typeof(Task<IEnumerable<DatabaseObject>>));

        var parameters = searchMethod.GetParameters();
        parameters.Should().HaveCount(2);
        parameters[0].ParameterType.Should().Be(typeof(IRipeSearchRequest));
        parameters[1].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void Search_ShouldImplementCorrectInterface()
    {
        // Arrange
        var mockLocation = CreateMockLocation();
        var auth = new RipeClientAuthAnonymous();

        // Act
        var client = new ClientsRipe.RipeClient(mockLocation.Object, auth);

        // Assert
        client.Should().BeAssignableTo<IRipeClient>();
    }

    [Fact]
    public void Constructor_WithLocationAndAuth_ShouldCreateInstance()
    {
        // Arrange
        var mockLocation = CreateMockLocation();
        var auth = new RipeClientAuthPassword(TestConstants.TestPassword);

        // Act
        var client = new ClientsRipe.RipeClient(mockLocation.Object, auth);

        // Assert
        client.Should().NotBeNull();
        client.Debug.Should().BeFalse();
    }

    [Fact]
    public void Debug_Property_ShouldBeSettable()
    {
        // Arrange
        var mockLocation = CreateMockLocation();
        var auth = new RipeClientAuthAnonymous();
        var client = new ClientsRipe.RipeClient(mockLocation.Object, auth);

        // Act
        client.Debug = true;

        // Assert
        client.Debug.Should().BeTrue();
    }

    #endregion

    #region Exception Types Tests

    [Fact]
    public void RipeClientException_ShouldBeInstantiable()
    {
        // Arrange & Act
        var exception = new RipeClientException("Test error");

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be("Test error");
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void RipeClientBadRequestException_WithMessage_ShouldStoreMessage()
    {
        // Arrange & Act
        var exception = new RipeClientBadRequestException("Bad request");

        // Assert
        exception.Message.Should().Be("Bad request");
        exception.ErrorMessages.Should().BeNull();
    }

    [Fact]
    public void RipeClientBadRequestException_WithErrorMessages_ShouldStoreErrorMessages()
    {
        // Arrange
        var errorMessages = TestDataFactory.CreateErrorResponse("Validation failed");

        // Act
        var exception = new RipeClientBadRequestException(errorMessages.ErrorMessages);

        // Assert
        exception.ErrorMessages.Should().NotBeNull();
        exception.ErrorMessages.Should().Be(errorMessages.ErrorMessages);
    }

    [Fact]
    public void RipeClientNotFoundException_ShouldBeInstantiable()
    {
        // Arrange & Act
        var exception = new RipeClientNotFoundException("Not found");

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be("Not found");
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void RipeClientConflictException_ShouldStoreContent()
    {
        // Arrange
        var content = "Conflict content";

        // Act
        var exception = new RipeClientConflictException(content);

        // Assert
        exception.Content.Should().Be(content);
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void RipeClientAuthPasswordException_ShouldBeInstantiable()
    {
        // Arrange & Act
        var exception = new RipeClientAuthPasswordException("Auth failed");

        // Assert
        exception.Message.Should().Be("Auth failed");
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void RipeSearchNotFoundException_DefaultConstructor_ShouldWork()
    {
        // Arrange & Act
        var exception = new RipeSearchNotFoundException();

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void RipeSearchNotFoundException_WithMessage_ShouldStoreMessage()
    {
        // Arrange & Act
        var exception = new RipeSearchNotFoundException("Search not found");

        // Assert
        exception.Message.Should().Be("Search not found");
    }

    #endregion

    #region IRipeLocation Tests

    [Fact]
    public void RipeSecureLocation_ShouldProvideHttpsUrl()
    {
        // Arrange & Act
        var location = new RipeSecureLocation();

        // Assert
        location.Url.Should().Be("https://rest.db.ripe.net/");
    }

    [Fact]
    public void RipeNonSecureLocation_ShouldProvideHttpUrl()
    {
        // Arrange & Act
        var location = new RipeNonSecureLocation();

        // Assert
        location.Url.Should().Be("http://rest.db.ripe.net/");
    }

    #endregion

    #region Authentication Interface Tests

    [Fact]
    public void IRipeClientAuth_ShouldHaveGetSecretMethod()
    {
        // Arrange
        var auth = new RipeClientAuthPassword("test");

        // Act & Assert
        auth.Should().BeAssignableTo<IRipeClientAuth>();
    }

    [Fact]
    public void IRipeClientAuthEnhanced_ShouldExtendIRipeClientAuth()
    {
        // Arrange
        var auth = new RipeClientAuthApiKey("test-key");

        // Act & Assert
        auth.Should().BeAssignableTo<IRipeClientAuthEnhanced>();
        auth.Should().BeAssignableTo<IRipeClientAuth>();
    }

    [Fact]
    public void AllAuthClasses_ShouldImplementAppropriateInterfaces()
    {
        // Assert
        new RipeClientAuthAnonymous().Should().BeAssignableTo<IRipeClientAuthEnhanced>();
        new RipeClientAuthPassword("test").Should().BeAssignableTo<IRipeClientAuthEnhanced>();
        new RipeClientAuthApiKey("test").Should().BeAssignableTo<IRipeClientAuthEnhanced>();
        new RipeClientAuthBasic("user", "pass").Should().BeAssignableTo<IRipeClientAuthEnhanced>();
    }

    #endregion

    #region RipeSearchRequest Integration Tests

    [Fact]
    public void RipeSearchRequest_GeneratesCorrectRestRequest()
    {
        // Arrange
        var searchRequest = new RipeSearchRequest(TestConstants.TestIpV4Range);
        searchRequest.AddFilter(TypeFilter.Route);
        searchRequest.Flags = RipeSearchRequestFlags.AllMore;

        // Act
        var restRequest = searchRequest.GetRequest();

        // Assert
        restRequest.Should().NotBeNull();
        restRequest.Resource.Should().Be("search");
        restRequest.Method.Should().Be(Method.Get);

        var queryParam = restRequest.Parameters.FirstOrDefault(p => p.Name == "query-string");
        queryParam.Should().NotBeNull();
        queryParam!.Value.Should().Be(TestConstants.TestIpV4Range);
    }

    [Fact]
    public void RipeSearchRequest_WithMultipleFilters_GeneratesCorrectParameters()
    {
        // Arrange
        var searchRequest = new RipeSearchRequest(TestConstants.TestIpV4Range);
        searchRequest.AddFilter(TypeFilter.Route);
        searchRequest.AddFilter(TypeFilter.Route6);
        searchRequest.AddFilter(TypeFilter.Inetnum);

        // Act
        var restRequest = searchRequest.GetRequest();

        // Assert
        var typeFilters = restRequest.Parameters.Where(p => p.Name == "type-filter").ToList();
        typeFilters.Should().HaveCount(3);
        typeFilters.Select(p => p.Value).Should().Contain(new[] { "route", "route6", "inetnum" });
    }

    #endregion

    #region TypeFilter Enum Tests

    [Fact]
    public void TypeFilter_ShouldHaveFlagsAttribute()
    {
        // Arrange & Act
        var type = typeof(TypeFilter);

        // Assert
        type.Should().BeDecoratedWith<FlagsAttribute>();
    }

    [Fact]
    public void TypeFilter_Values_ShouldBePowersOfTwo()
    {
        // Assert - for proper flag combinations
        ((int)TypeFilter.None).Should().Be(0);
        ((int)TypeFilter.Autnum).Should().Be(1);
        ((int)TypeFilter.Inetnum).Should().Be(2);
        ((int)TypeFilter.Inetnum6).Should().Be(4);
        ((int)TypeFilter.Route).Should().Be(8);
        ((int)TypeFilter.Route6).Should().Be(16);
        ((int)TypeFilter.Person).Should().Be(32);
    }

    #endregion

    #region RipeSearchRequestFlags Enum Tests

    [Fact]
    public void RipeSearchRequestFlags_ShouldHaveExpectedValues()
    {
        // Assert
        Enum.IsDefined(typeof(RipeSearchRequestFlags), RipeSearchRequestFlags.None).Should().BeTrue();
        Enum.IsDefined(typeof(RipeSearchRequestFlags), RipeSearchRequestFlags.AllMore).Should().BeTrue();
        Enum.IsDefined(typeof(RipeSearchRequestFlags), RipeSearchRequestFlags.OneMore).Should().BeTrue();
        Enum.IsDefined(typeof(RipeSearchRequestFlags), RipeSearchRequestFlags.AllLess).Should().BeTrue();
        Enum.IsDefined(typeof(RipeSearchRequestFlags), RipeSearchRequestFlags.OneLess).Should().BeTrue();
    }

    #endregion

    #region CancellationToken Support Tests

    [Fact]
    public async Task Search_ShouldAcceptCancellationToken()
    {
        // Arrange
        var mockLocation = CreateMockLocation();
        var auth = new RipeClientAuthAnonymous();
        var client = new ClientsRipe.RipeClient(mockLocation.Object, auth);
        var searchRequest = new RipeSearchRequest(TestConstants.TestIpV4Range);
        var cts = new CancellationTokenSource();

        // Act & Assert - method signature should accept CancellationToken
        // We're testing the API surface, actual cancellation requires integration test
        var searchMethod = typeof(ClientsRipe.RipeClient).GetMethod("Search");
        var parameters = searchMethod!.GetParameters();

        parameters.Should().HaveCount(2);
        parameters[1].Name.Should().Be("cancellationToken");
        parameters[1].ParameterType.Should().Be(typeof(CancellationToken));
        parameters[1].HasDefaultValue.Should().BeTrue();
    }

    [Fact]
    public void GetObjectByKey_ShouldAcceptCancellationToken()
    {
        // Arrange & Act
        var method = typeof(ClientsRipe.RipeClient).GetMethod("GetObjectByKey");
        var parameters = method!.GetParameters();

        // Assert
        parameters.Should().HaveCount(4);
        parameters[3].Name.Should().Be("cancellationToken");
        parameters[3].ParameterType.Should().Be(typeof(CancellationToken));
        parameters[3].HasDefaultValue.Should().BeTrue();
    }

    [Fact]
    public void AddObject_ShouldAcceptCancellationToken()
    {
        // Arrange & Act
        var method = typeof(ClientsRipe.RipeClient).GetMethod("AddObject");
        var parameters = method!.GetParameters();

        // Assert
        parameters.Should().HaveCount(2);
        parameters[1].Name.Should().Be("cancellationToken");
        parameters[1].ParameterType.Should().Be(typeof(CancellationToken));
        parameters[1].HasDefaultValue.Should().BeTrue();
    }

    [Fact]
    public void UpdateObject_ShouldAcceptCancellationToken()
    {
        // Arrange & Act
        var method = typeof(ClientsRipe.RipeClient).GetMethod("UpdateObject");
        var parameters = method!.GetParameters();

        // Assert
        parameters.Should().HaveCount(2);
        parameters[1].Name.Should().Be("cancellationToken");
        parameters[1].ParameterType.Should().Be(typeof(CancellationToken));
        parameters[1].HasDefaultValue.Should().BeTrue();
    }

    [Fact]
    public void RemoveObject_ShouldAcceptCancellationToken()
    {
        // Arrange & Act
        var method = typeof(ClientsRipe.RipeClient).GetMethod("RemoveObject");
        var parameters = method!.GetParameters();

        // Assert
        parameters.Should().HaveCount(2);
        parameters[1].Name.Should().Be("cancellationToken");
        parameters[1].ParameterType.Should().Be(typeof(CancellationToken));
        parameters[1].HasDefaultValue.Should().BeTrue();
    }

    #endregion

    #region SearchSync Obsolete Test

    [Fact]
    public void SearchSync_ShouldBeMarkedAsObsolete()
    {
        // Arrange & Act
        var method = typeof(ClientsRipe.RipeClient).GetMethod("SearchSync");

        // Assert
        method.Should().NotBeNull();
        method!.Should().BeDecoratedWith<ObsoleteAttribute>();

        var obsoleteAttr = method.GetCustomAttributes(typeof(ObsoleteAttribute), false)
            .Cast<ObsoleteAttribute>()
            .FirstOrDefault();

        obsoleteAttr.Should().NotBeNull();
        obsoleteAttr!.Message.Should().Contain("sync-over-async");
        obsoleteAttr.Message.Should().Contain("deadlock");
    }

    [Fact]
    public void IRipeClient_SearchSyncMethod_ShouldBeMarkedAsObsolete()
    {
        // Arrange & Act
        var method = typeof(IRipeClient).GetMethod("SearchSync");

        // Assert
        method.Should().NotBeNull();
        method!.Should().BeDecoratedWith<ObsoleteAttribute>();
    }

    #endregion
}
