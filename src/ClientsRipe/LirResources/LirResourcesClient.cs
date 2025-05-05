using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClientsRipe.LirResources.Models;
using ClientsRipe.RpkiClient.Models;
using HttpTracer;
using HttpTracer.Logger;
using RestSharp;

namespace ClientsRipe.LirResources;

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

    public async Task<LirResourcesReply> GetAll(string apiKey, CancellationToken cancellationToken)
    {
        return await RequestResources("", apiKey, cancellationToken);
    }

    public async Task<LirResourcesReply> Get(string apiKey, CancellationToken cancellationToken)
    {
        return await RequestResources("", apiKey, cancellationToken);
    }

    public async Task<LirResourcesReply> GetAsn(string apiKey, CancellationToken cancellationToken)
    {
        return await RequestResources("asn", apiKey, cancellationToken);
    }

    public async Task<LirResourcesReply> GetIpv4(string apiKey, CancellationToken cancellationToken)
    {
        return await RequestResources("ipv4", apiKey, cancellationToken);
    }

    public async Task<LirResourcesReply> GetIpv6(string apiKey, CancellationToken cancellationToken)
    {
        return await RequestResources("ipv6", apiKey, cancellationToken);
    }

    public async Task<LirResourcesReply> GetResources(string apiKey, CancellationToken cancellationToken)
    {
        var resources = await GetAll(apiKey, cancellationToken);
        //TODO: Not implemented
        resources.Ipv6Assignments = null;

        return resources;
    }


    protected virtual async Task<LirResourcesReply> RequestResources(string resource, string apiKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(apiKey))
            throw new ArgumentException("API key not provided.", nameof(apiKey));

        var request = new RestRequest(resource, Method.Get);

        request.AddParameter("format", "json");
        request.AddParameter("jsonCallback", "?");

        var client = GetClient(apiKey);

        try
        {
            var reply = await client.ExecuteAsync<LirResourcesReply>(request, cancellationToken);
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
}