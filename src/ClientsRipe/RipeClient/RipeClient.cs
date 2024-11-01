﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using HttpTracer;
using HttpTracer.Logger;
using RestSharp;
using RipeDatabaseObjects;
using Attribute = RipeDatabaseObjects.Attribute;

namespace ClientsRipe
{
    public class RipeClient : IRipeClient
    {
        private readonly string _baseUrl;
        private readonly IRipeClientAuth _auth;
        
        public RipeClient(IRipeLocation location, IRipeClientAuth auth)
        {
            _baseUrl = location.Url;
            _auth = auth;
        }

        public bool Debug { get; set; }

        public async Task<IEnumerable<DatabaseObject>> Search(IRipeSearchRequest query, CancellationToken cancellationToken)
        {
            var client = new RestClient(_baseUrl);
            var restRequest = query.GetRequest();
            
            var queryResult = await client.ExecuteAsync<RipeObjects>(restRequest, cancellationToken);

            if (queryResult.StatusCode == HttpStatusCode.NotFound)
                return new List<DatabaseObject>();

            if (!queryResult.IsSuccessful)
                throw new Exception($"{queryResult.StatusDescription}");

            return queryResult?.Data?.Objects?.Object;
        }

        public async Task<DatabaseObject> GetObjectByKey(string key, string objectType, string source, CancellationToken cancellationToken)
        {
            var client = new RestClient(_baseUrl);
            var request = new RestRequest($"/{source}/{objectType}/{key}");
            
            var queryResult = await client.ExecuteAsync<RipeObjects>(request, cancellationToken);

            if (queryResult.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (queryResult.Data == null)
                throw new RipeClientNotFoundException(queryResult.StatusDescription);

            return queryResult.Data.Objects.Object.FirstOrDefault();
        }

        public IEnumerable<DatabaseObject> SearchSync(IRipeSearchRequest query)
        {
            var searchTask = Search(query, CancellationToken.None);
            searchTask.Wait();

            return searchTask.Result;
        }

        public async Task<string> AddObject(RipeObject obj, CancellationToken cancellationToken)
        {
            var whoisResource = new WhoisResources
            {
                Objects = new WhoisObjects
                {
                    Object = new List<DatabaseWhoisObject>()
                }
            };

            string requestContent = "";

            var databaseObj = new DatabaseWhoisObject();
            
            //get object type 
            var databaseObjType = obj.GetRipeObjectType();

            //location source 
            var source = obj["source"];

            if (string.IsNullOrEmpty(source))
                throw new Exception("Source not provided.");

            databaseObj.Type = databaseObjType;
            databaseObj.Source = new Source() {Id = source};

            databaseObj.Attributes = new WhoisAttributes
            {
                Attribute = new List<Attribute>()
            };

            foreach (var (key, value) in obj)
            {
                if (key.ToLowerInvariant() == "source") continue;
                
                databaseObj.Attributes.Attribute.Add(new Attribute{Name = key, Value = value});    
            }

            // just for beautiful object
            databaseObj.Attributes.Attribute.Add(new Attribute{Name = "source", Value = obj["source"]});
            whoisResource.Objects.Object.Add(databaseObj);

            requestContent = SerializerContent(whoisResource);

            var restRequest = new RestRequest($"/{source}/{databaseObjType}", Method.Post);
            restRequest.AddBody(requestContent, "application/xml");

            //AUTH method
            restRequest.AddParameter("password",   await _auth.GetSecret(), ParameterType.QueryString);
            
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
            
            var reply = await client.ExecuteAsync<RipeObjects>(restRequest, cancellationToken);            

            if (reply.StatusCode == HttpStatusCode.Conflict)
            {
                throw new RipeClientConflictException(reply.Content);
            }

            if (reply.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new RipeClientAuthPasswordException($"Unauthorized for RIPE Object {restRequest.Resource}");
            }

            if (reply.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new RipeClientBadRequestException(reply?.Data?.ErrorMessages);
            }

            if (!reply.IsSuccessful)
            {
                throw new RipeClientException(reply.ErrorMessage);
            }
            
            return reply.Content;
        }

        private static string SerializerContent(WhoisResources whoisResource)
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            var memoryStream = new MemoryStream();

            var formatter = new XmlSerializer(typeof(WhoisResources));
            using var textWriter = new StreamWriter(memoryStream, Encoding.UTF8);
            
            formatter.Serialize(textWriter, whoisResource, ns);
            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }

        public async Task<RipeObjects> UpdateObject(RipeObject obj, CancellationToken cancellationToken)
        {
            var whoisResource = new WhoisResources
            {
                Objects = new WhoisObjects
                {
                    Object = new List<DatabaseWhoisObject>()
                }
            };

            //curl -X PUT 'https://rest.db.ripe.net/ripe/person/pp1-ripe?password=123'
            //get object type 
            var databaseObjType = obj.GetRipeObjectType();

            //location source 
            var source = obj["source"];
            var key = obj.GetKey(false);
            
            var databaseObj = new DatabaseWhoisObject();

            if (string.IsNullOrEmpty(source))
                throw new Exception("Source not provided.");

            databaseObj.Type = databaseObjType;
            databaseObj.Source = new Source() {Id = source};

            databaseObj.Attributes = new WhoisAttributes
            {
                Attribute = new List<Attribute>()
            };

            foreach (var (objKey, value) in obj)
            {
                if (objKey.ToLowerInvariant() == "source") continue;
                
                databaseObj.Attributes.Attribute.Add(new Attribute{Name = objKey, Value = value});    
            }

            // just for beautiful object
            databaseObj.Attributes.Attribute.Add(new Attribute{Name = "source", Value = obj["source"]});
            whoisResource.Objects.Object.Add(databaseObj);

            var requestContent = SerializerContent(whoisResource);
            
            
            var restRequest = new RestRequest($"/{source}/{databaseObjType}/{key}")
            {
                Method = Method.Put
            };

            //AUTH method
            restRequest.AddParameter("password",  await _auth.GetSecret(), ParameterType.QueryString);
            
            var client = new RestClient(_baseUrl);
            restRequest.AddBody(requestContent, "application/xml");
            
            var reply = await client.ExecuteAsync<RipeObjects>(restRequest, cancellationToken);

            return reply.Data;
        }

        public async Task RemoveObject(RipeObject obj, CancellationToken cancellationToken)
        {
            //curl -X DELETE 'https://rest.db.ripe.net/ripe/person/pp1-ripe?password=123'
            //get object type 
            var databaseObjType = obj.GetRipeObjectType();

            //location source 
            var source = obj["source"];

            if (string.IsNullOrEmpty(source))
                throw new Exception("Source not provided.");

            var key = obj.GetKey(false);

            var restRequest = new RestRequest($"/{source}/{databaseObjType}/{key}")
            {
                Method = Method.Delete
            };

            //AUTH method
            restRequest.AddParameter("password",  await _auth.GetSecret(), ParameterType.QueryString);

            var client = new RestClient(_baseUrl);
            
            var reply = await client.ExecuteAsync(restRequest, cancellationToken);

            if (reply.StatusCode == HttpStatusCode.NotFound)
            {
                throw new RipeClientNotFoundException(reply.StatusDescription);
            }
            
            if (!reply.IsSuccessful)
            {
                throw new RipeClientException(reply.StatusDescription);
            }
        }
    }
}