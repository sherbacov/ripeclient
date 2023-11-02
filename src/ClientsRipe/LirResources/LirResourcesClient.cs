using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using ClientsRipe.LirResources.Models;
using ClientsRipe.RpkiClient.Models;
using HttpTracer;
using HttpTracer.Logger;
using RestSharp;

namespace ClientsRipe.LirResources;

public interface ILirResourcesClient
{
    public bool Debug { get; set; } 
       
    public Task<LirResourcesReply> GetResources(string apiKey);
    public IEnumerable<RpkiRoaPlain> GetRoas(string apiKey);

    public Task RpkiOperation(string apiKey, RpkiOperations operations);
    public Task RpkiOperationAdd(string apiKey, PublishRpkiRoaPlain operation);
    public Task RpkiOperationDelete(string apiKey, PublishRpkiRoaPlain operation);
}

public class LirResourcesClient : ILirResourcesClient
{
    private readonly string _baseUrl;
        
    public LirResourcesClient(ILirResourcesLocation ripeRpkiLocation)
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
    
    public IEnumerable<RpkiRoaPlain> GetRoas(string apiKey)
    {
        var request = new RestRequest("roas");

        var client = GetClient(apiKey);

        var replyTask = client.GetAsync<List<RpkiRoaPlain>>(request);
        replyTask.Wait();

        return replyTask.Result;
    }

    public async Task<LirResourcesReply> GetAsn(string apiKey)
    {
        return await RequestResources("asn", apiKey);
    }
    
    public async Task<LirResourcesReply> GetIpv4(string apiKey)
    {
        return await RequestResources("ipv4", apiKey);
    }

    public async Task<LirResourcesReply> GetIpv6(string apiKey)
    {
        return await RequestResources("ipv6", apiKey);
    }

    public async Task<LirResourcesReply> GetResources(string apiKey)
    {
        var asn = await GetAsn(apiKey);

        var ipv4 = await GetIpv4(apiKey);
        asn.Ipv4Allocations = ipv4.Ipv4Allocations;
        asn.Ipv4Assignments = ipv4.Ipv4Assignments;

        var ipv6 = await GetIpv6(apiKey);
        asn.Ipv6Allocations = ipv6.Ipv6Allocations;
        asn.Ipv6Assignments = ipv6.Ipv6Assignments;

        return asn;
    }
    
    
    protected virtual async Task<LirResourcesReply> RequestResources(string resource, string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
            throw new ArgumentException("API key not provided.", nameof(apiKey));
            
        var request = new RestRequest(resource, Method.Get);
        
        request.AddParameter("format", "json");
        request.AddParameter("jsonCallback", "?");

        var client = GetClient(apiKey);

        try
        {
            var reply = await client.ExecuteAsync<LirResourcesReply>(request);
            if (!reply.IsSuccessful)
                throw new Exception(reply.Content);

            return reply.Data;

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }        
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
            var reply = await  client.PostAsync(request);
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
        await RpkiOperation(apiKey, new RpkiOperations().Add(operation));
    }

    public async Task RpkiOperationDelete(string apiKey, PublishRpkiRoaPlain operation)
    {
        await RpkiOperation(apiKey, new RpkiOperations().Delete(operation));
    }
}