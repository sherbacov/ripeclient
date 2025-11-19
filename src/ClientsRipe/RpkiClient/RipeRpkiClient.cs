using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using ClientsRipe.RpkiClient.Models;
using HttpTracer;
using HttpTracer.Logger;
using RestSharp;

namespace ClientsRpki;

public interface IRipeRpkiClient
{
    public bool Debug { get; set; }

    // Async methods (recommended)
    Task<RpkiResourcesPlain> GetResourcesAsync(string apiKey);
    Task<IEnumerable<RpkiRoaPlain>> GetRoasAsync(string apiKey);
    Task RpkiOperation(string apiKey, RpkiOperations operations);
    Task RpkiOperationAdd(string apiKey, PublishRpkiRoaPlain operation);
    Task RpkiOperationDelete(string apiKey, PublishRpkiRoaPlain operation);

    // Sync methods (deprecated - can cause deadlocks)
    [Obsolete("Use GetResourcesAsync instead. This method uses sync-over-async anti-pattern and can cause deadlocks.")]
    RpkiResourcesPlain GetResources(string apiKey);
    [Obsolete("Use GetRoasAsync instead. This method uses sync-over-async anti-pattern and can cause deadlocks.")]
    IEnumerable<RpkiRoaPlain> GetRoas(string apiKey);
}

public class RipeRpkiClient : IRipeRpkiClient
{
    private readonly string _baseUrl;
        
    public RipeRpkiClient(IRipeRpkiLocation ripeRpkiLocation)
    {
        _baseUrl = ripeRpkiLocation.Url;
    }

    private RestClient GetClient(string apiKey)
    {
        RestClient client;
            
        if (Debug)
        {
            var options = new RestClientOptions(_baseUrl)
            {
                ConfigureMessageHandler = handler =>
                    new HttpTracerHandler(handler, new ConsoleLogger(), HttpMessageParts.All)
            };

            client = new RestClient(options);
        }
        else
        {
            client = new RestClient(_baseUrl);
        }

        client.AddDefaultHeader("ncc-api-authorization", apiKey);
            
        return client;
    }

    public bool Debug { get; set; }

    // Async methods (recommended)
    public async Task<RpkiResourcesPlain> GetResourcesAsync(string apiKey)
    {
        var request = new RestRequest("resources");
        var client = GetClient(apiKey);

        var reply = await client.GetAsync<RpkiResourcesPlain>(request).ConfigureAwait(false);
        return reply;
    }

    public async Task<IEnumerable<RpkiRoaPlain>> GetRoasAsync(string apiKey)
    {
        var request = new RestRequest("roas");
        var client = GetClient(apiKey);

        var reply = await client.GetAsync<List<RpkiRoaPlain>>(request).ConfigureAwait(false);
        return reply;
    }

    // Sync methods (deprecated - kept for backward compatibility)
    public RpkiResourcesPlain GetResources(string apiKey)
    {
        // Use GetAwaiter().GetResult() instead of Wait/Result for slightly better behavior
        return GetResourcesAsync(apiKey).GetAwaiter().GetResult();
    }

    public IEnumerable<RpkiRoaPlain> GetRoas(string apiKey)
    {
        // Use GetAwaiter().GetResult() instead of Wait/Result for slightly better behavior
        return GetRoasAsync(apiKey).GetAwaiter().GetResult();
    }

    public async Task RpkiOperation(string apiKey, RpkiOperations operations)
    {
        if (string.IsNullOrEmpty(apiKey))
            throw new ArgumentException("API key not provided.", nameof(apiKey));
            
        var request = new RestRequest("roas/publish", Method.Post);
            
        var jsonString = JsonSerializer.Serialize(operations, options: new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
            
        request.AddBody(jsonString, "application/json");

        var client = GetClient(apiKey);

        try
        {
            var reply = await client.PostAsync(request).ConfigureAwait(false);
            if (!reply.IsSuccessful)
                throw new Exception(reply.Content);

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task RpkiOperationAdd(string apiKey, PublishRpkiRoaPlain operation)
    {
        await RpkiOperation(apiKey, new RpkiOperations().Add(operation)).ConfigureAwait(false);
    }

    public async Task RpkiOperationDelete(string apiKey, PublishRpkiRoaPlain operation)
    {
        await RpkiOperation(apiKey, new RpkiOperations().Delete(operation)).ConfigureAwait(false);
    }
}