using System.Net;
using RipeDatabaseObjects;

namespace ClientsRipe.UnitTests.Helpers;

/// <summary>
/// Helper for creating mocked RestSharp responses using Moq
/// Since RestSharp v111+ doesn't allow direct instantiation, we use mocks
/// </summary>
public static class MockResponseHelper
{
    /// <summary>
    /// Creates a mocked successful RestResponse with data
    /// </summary>
    public static Mock<RestResponse<T>> CreateMockedSuccessResponse<T>(T data, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var mockResponse = new Mock<RestResponse<T>>(MockBehavior.Loose, null as RestRequest);
        mockResponse.Setup(r => r.Data).Returns(data);
        mockResponse.Setup(r => r.StatusCode).Returns(statusCode);
        mockResponse.Setup(r => r.IsSuccessful).Returns(true);
        mockResponse.Setup(r => r.ResponseStatus).Returns(ResponseStatus.Completed);
        return mockResponse;
    }

    /// <summary>
    /// Creates a mocked failed RestResponse
    /// </summary>
    public static Mock<RestResponse<T>> CreateMockedFailedResponse<T>(HttpStatusCode statusCode, string content = "")
    {
        var mockResponse = new Mock<RestResponse<T>>(MockBehavior.Loose, null as RestRequest);
        mockResponse.Setup(r => r.StatusCode).Returns(statusCode);
        mockResponse.Setup(r => r.IsSuccessful).Returns(false);
        mockResponse.Setup(r => r.Content).Returns(content);
        mockResponse.Setup(r => r.ResponseStatus).Returns(ResponseStatus.Completed);
        return mockResponse;
    }

    /// <summary>
    /// Creates a 404 Not Found response
    /// </summary>
    public static Mock<RestResponse<T>> CreateMockedNotFoundResponse<T>()
    {
        return CreateMockedFailedResponse<T>(HttpStatusCode.NotFound, "Not Found");
    }

    /// <summary>
    /// Creates a 400 Bad Request response with error messages
    /// </summary>
    public static Mock<RestResponse<RipeObjects>> CreateMockedBadRequestResponse(string errorMessage = "Bad Request")
    {
        var errorData = TestDataFactory.CreateErrorResponse(errorMessage);
        var mockResponse = new Mock<RestResponse<RipeObjects>>(MockBehavior.Loose, null as RestRequest);
        mockResponse.Setup(r => r.Data).Returns(errorData);
        mockResponse.Setup(r => r.StatusCode).Returns(HttpStatusCode.BadRequest);
        mockResponse.Setup(r => r.IsSuccessful).Returns(false);
        mockResponse.Setup(r => r.Content).Returns(errorMessage);
        mockResponse.Setup(r => r.ResponseStatus).Returns(ResponseStatus.Completed);
        return mockResponse;
    }

    /// <summary>
    /// Creates a 401 Unauthorized response
    /// </summary>
    public static Mock<RestResponse<RipeObjects>> CreateMockedUnauthorizedResponse()
    {
        return CreateMockedFailedResponse<RipeObjects>(HttpStatusCode.Unauthorized, "Unauthorized");
    }

    /// <summary>
    /// Creates a 409 Conflict response
    /// </summary>
    public static Mock<RestResponse<RipeObjects>> CreateMockedConflictResponse(string content = "Conflict")
    {
        return CreateMockedFailedResponse<RipeObjects>(HttpStatusCode.Conflict, content);
    }

    /// <summary>
    /// Creates a mocked RestClient that returns the specified response
    /// </summary>
    public static Mock<IRestClient> CreateMockedRestClient<T>(RestResponse<T> response)
    {
        var mockClient = new Mock<IRestClient>();

        mockClient
            .Setup(x => x.ExecuteAsync<T>(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        return mockClient;
    }

    /// <summary>
    /// Creates a mocked RestClient that returns the specified mocked response
    /// </summary>
    public static Mock<IRestClient> CreateMockedRestClientFromMock<T>(Mock<RestResponse<T>> mockedResponse)
    {
        return CreateMockedRestClient(mockedResponse.Object);
    }

    /// <summary>
    /// Creates a mocked RestClient that returns the specified response for generic Execute
    /// </summary>
    public static Mock<IRestClient> CreateMockedRestClientForExecute(RestResponse response)
    {
        var mockClient = new Mock<IRestClient>();

        mockClient
            .Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        return mockClient;
    }

    /// <summary>
    /// Verifies that ExecuteAsync was called with the expected request
    /// </summary>
    public static void VerifyExecuteAsync<T>(Mock<IRestClient> mockClient, Times times)
    {
        mockClient.Verify(
            x => x.ExecuteAsync<T>(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()),
            times
        );
    }

    /// <summary>
    /// Verifies that ExecuteAsync was called with a specific resource path
    /// </summary>
    public static void VerifyExecuteAsyncWithPath<T>(Mock<IRestClient> mockClient, string expectedPath)
    {
        mockClient.Verify(
            x => x.ExecuteAsync<T>(
                It.Is<RestRequest>(r => r.Resource == expectedPath),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }
}
