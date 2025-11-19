using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using HttpTracer;
using HttpTracer.Logger;
using RestSharp;
using RipeDatabaseObjects;
using Attribute = RipeDatabaseObjects.Attribute;

namespace ClientsRipe
{
    public class RipeClientException : Exception
    {
        public RipeClientException(string message) : base(message)
        {
        }
    }
    
    public class RipeClientBadRequestException : Exception {
    
        public RipeClientBadRequestException(string message) : base(message)
        {
        }

        public ErrorMessages ErrorMessages { get; set; }
        
        public RipeClientBadRequestException(ErrorMessages errorMessages)
        {
            ErrorMessages = errorMessages;
        }
    }
    
    public class RipeClientNotFoundException : Exception
    {
        public RipeClientNotFoundException(string message) : base(message)
        {
        }
    }
    
    public class RipeClientConflictException: Exception
    {
        public RipeClientConflictException(string content)
        {
            Content = content;
        }
        
        public string Content { get; }
    }

    public class RipeClientAuthPasswordException : Exception
    {
        public RipeClientAuthPasswordException(string message) : base(message)
        {
            
        }
    }
    
    public class RipeSearchNotFoundException : Exception
    {
        public RipeSearchNotFoundException() : base() { }
        public RipeSearchNotFoundException(string message) : base(message)  { }
    }
    

    public interface IRipeClientAuth
    {
        Task<string> GetSecret();
    }

    /// <summary>
    /// Enhanced authentication interface that allows different authentication methods
    /// to apply their credentials directly to the HTTP request
    /// </summary>
    public interface IRipeClientAuthEnhanced : IRipeClientAuth
    {
        /// <summary>
        /// Applies authentication to the RestSharp request
        /// </summary>
        /// <param name="request">The RestRequest to apply authentication to</param>
        Task ApplyAuthentication(RestRequest request);
    }

    /// <summary>
    /// Anonymous authentication (no credentials)
    /// Used for public read-only operations
    /// </summary>
    public class RipeClientAuthAnonymous : IRipeClientAuthEnhanced
    {
        public Task<string> GetSecret()
        {
            return Task.FromResult("");
        }

        public Task ApplyAuthentication(RestRequest request)
        {
            // No authentication needed for anonymous access
            return Task.CompletedTask;
        }
    } 
    
    /// <summary>
    /// Authentication using password query parameter
    /// Password is sent as a query string parameter (?password=xxx)
    /// </summary>
    public class RipeClientAuthPassword : IRipeClientAuthEnhanced
    {
        private readonly string _password;

        public RipeClientAuthPassword(string password)
        {
            _password = password;
        }

        public Task<string> GetSecret()
        {
            return Task.FromResult(_password);
        }

        public Task ApplyAuthentication(RestRequest request)
        {
            request.AddParameter("password", _password, ParameterType.QueryString);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Authentication using RIPE Database API Key
    /// API keys are sent via the X-API-Key HTTP header
    /// </summary>
    public class RipeClientAuthApiKey : IRipeClientAuthEnhanced
    {
        private readonly string _apiKey;

        public RipeClientAuthApiKey(string apiKey)
        {
            _apiKey = apiKey;
        }

        public Task<string> GetSecret()
        {
            return Task.FromResult(_apiKey);
        }

        public Task ApplyAuthentication(RestRequest request)
        {
            request.AddHeader("X-API-Key", _apiKey);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Authentication using HTTP Basic Authentication
    /// Credentials are sent as Base64-encoded username:password in the Authorization header
    /// </summary>
    public class RipeClientAuthBasic : IRipeClientAuthEnhanced
    {
        private readonly string _username;
        private readonly string _password;

        public RipeClientAuthBasic(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public Task<string> GetSecret()
        {
            var credentials = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_username}:{_password}")
            );
            return Task.FromResult(credentials);
        }

        public Task ApplyAuthentication(RestRequest request)
        {
            var credentials = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_username}:{_password}")
            );
            request.AddHeader("Authorization", $"Basic {credentials}");
            return Task.CompletedTask;
        }
    }
    
    public interface IRipeClient
    {
        public bool Debug { get; set; } 
            
        public IEnumerable<DatabaseObject> SearchSync(IRipeSearchRequest query);
        
        /// <summary>
        /// Searching object at RIPE database
        /// </summary>
        /// <param name="query">Searching object. Like "91.194.10.0/24"</param>
        /// <returns>Founded objects from RIPE Database</returns>
        public Task<IEnumerable<DatabaseObject>> Search(IRipeSearchRequest query);


        public Task<DatabaseObject> GetObjectByKey(string key, string objectType, string source);
        
        /// <summary>
        /// Add object to RIPE Database
        /// </summary>
        /// <param name="obj">Object to add</param>
        /// <returns>Raw reply from RIPE</returns>
        public Task<string> AddObject(RipeObject obj);
        public Task<RipeObjects> UpdateObject(RipeObject obj);
        public Task RemoveObject(RipeObject obj);
    }
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

        public async Task<IEnumerable<DatabaseObject>> Search(IRipeSearchRequest query)
        {
            var client = new RestClient(_baseUrl);
            var restRequest = query.GetRequest();
            
            var queryResult = await client.ExecuteAsync<RipeObjects>(restRequest);

            if (queryResult.StatusCode == HttpStatusCode.NotFound)
                return new List<DatabaseObject>();

            if (!queryResult.IsSuccessful)
                throw new Exception($"{queryResult.StatusDescription}");

            return queryResult?.Data?.Objects?.Object;
        }

        public async Task<DatabaseObject> GetObjectByKey(string key, string objectType, string source)
        {
            var client = new RestClient(_baseUrl);
            var request = new RestRequest($"/{source}/{objectType}/{key}");
            
            var queryResult = await client.ExecuteAsync<RipeObjects>(request);

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
            var searchTask = Search(query);
            searchTask.Wait();

            return searchTask.Result;
        }

        public async Task<string> AddObject(RipeObject obj)
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
                throw new ArgumentOutOfRangeException(nameof(source), "Source not provided.");

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
            if (_auth is IRipeClientAuthEnhanced enhancedAuth)
            {
                await enhancedAuth.ApplyAuthentication(restRequest);
            }
            else
            {
                // Backward compatibility - assume password-based auth
                restRequest.AddParameter("password", await _auth.GetSecret(), ParameterType.QueryString);
            }
            
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
            
            var reply = await client.ExecuteAsync<RipeObjects>(restRequest);            

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

        private string SerializerContent(WhoisResources whoisResource)
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            var memoryStream = new MemoryStream();

            var formatter = new XmlSerializer(typeof(WhoisResources));
            using var textWriter = new StreamWriter(memoryStream, Encoding.UTF8);
            
            formatter.Serialize(textWriter, whoisResource, ns);
            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }

        public async Task<RipeObjects> UpdateObject(RipeObject obj)
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
                throw new ArgumentOutOfRangeException(nameof(source), "Source not provided.");

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
            if (_auth is IRipeClientAuthEnhanced enhancedAuth)
            {
                await enhancedAuth.ApplyAuthentication(restRequest);
            }
            else
            {
                // Backward compatibility - assume password-based auth
                restRequest.AddParameter("password", await _auth.GetSecret(), ParameterType.QueryString);
            }

            var client = new RestClient(_baseUrl);
            restRequest.AddBody(requestContent, "application/xml");
            
            var reply = await client.ExecuteAsync<RipeObjects>(restRequest);

            return reply.Data;
        }

        public async Task RemoveObject(RipeObject obj)
        {
            //curl -X DELETE 'https://rest.db.ripe.net/ripe/person/pp1-ripe?password=123'
            //get object type 
            var databaseObjType = obj.GetRipeObjectType();

            //location source 
            var source = obj["source"];

            if (string.IsNullOrEmpty(source))
                throw new ArgumentOutOfRangeException(nameof(source), "Source not provided.");

            var key = obj.GetKey(false);

            var restRequest = new RestRequest($"/{source}/{databaseObjType}/{key}");
            restRequest.Method = Method.Delete;
            
            //AUTH method
            if (_auth is IRipeClientAuthEnhanced enhancedAuth)
            {
                await enhancedAuth.ApplyAuthentication(restRequest);
            }
            else
            {
                // Backward compatibility - assume password-based auth
                restRequest.AddParameter("password", await _auth.GetSecret(), ParameterType.QueryString);
            }

            var client = new RestClient(_baseUrl);
            
            var reply = await client.ExecuteAsync(restRequest);

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