using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using HttpTracer;
using HttpTracer.Logger;
using RestSharp;
using RipeRpkiObjects;

namespace ClientsRpki
{
    public interface IRipeRpkiClient
    {
        public bool Debug { get; set; } 
       
        public RpkiResourcesPlain GetResources(string apiKey);
        public IEnumerable<RpkiRoaPlain> GetRoas(string apiKey);

        public Task RpkiOperation(string apiKey, RpkiOperations operations);
        public Task RpkiOperationAdd(string apiKey, PublishRpkiRoaPlain operation);
        public Task RpkiOperationDelete(string apiKey, PublishRpkiRoaPlain operation);
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

        public RpkiResourcesPlain GetResources(string apiKey)
        {
            var request = new RestRequest("resources");

            var client = GetClient(apiKey);

            var replyTask = client.GetAsync<RpkiResourcesPlain>(request);
            replyTask.Wait();
            var reply = replyTask.Result;

            //return reply.Data;
            return reply;
        }

        public IEnumerable<RpkiRoaPlain> GetRoas(string apiKey)
        {
            var request = new RestRequest("roas");

            var client = GetClient(apiKey);

            var replyTask = client.GetAsync<List<RpkiRoaPlain>>(request);
            replyTask.Wait();

            return replyTask.Result;
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
}