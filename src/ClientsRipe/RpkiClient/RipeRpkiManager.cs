using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using ClientsRipe;
using ClientsRipe.RpkiClient.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NodaTime.Text;
using RipeDatabaseObjects;


namespace ClientsRpki
{
    public interface IRipeRpkiSettingsManager
    {
        public RpkiSettings LoadSettings();
    }

    public class RipeRpkiSettingsManager : IRipeRpkiSettingsManager
    {
        private readonly IConfiguration _cfg;

        public RipeRpkiSettingsManager(IConfiguration cfg)
        {
            _cfg = cfg;
        }
        
        public RpkiSettings LoadSettings()
        {
            var settings = new RpkiSettings();


            for (var i = 0; i < 30; i++)
            {
                var key = _cfg[$"rpki:keys:{i}"];

                if (string.IsNullOrEmpty(key))
                    break;
                
                
                settings.Keys.Add(key);
            }

            //
            var timeout = _cfg["rpki:cache_timeout"];
            if (string.IsNullOrEmpty(timeout))
                timeout = "1:00:00";

            var pattern = DurationPattern.CreateWithInvariantCulture("D:hh:mm");
            settings.CacheTimeout = (int)pattern.Parse(timeout).Value.TotalSeconds;

            return settings;
        }
    }

    public interface ICacheManager
    {
        public void Save(List<RpkiRoa> rpkiRoa);
        public void Save(Dictionary<RpkiResource, string> resources);

        public List<RpkiRoa> LoadRpkiRoas();
        public Dictionary<RpkiResource, string> LoadRpkiResources();

        public void DropRoasCache();

    }

    class CacheFile
    {
        public List<RpkiRoa> RpkiRoa { get; set; }
        public string Resources { get; set; }
    }

    public class CacheManager : ICacheManager
    {
        private RpkiSettings _settings;

        public CacheManager(IRipeRpkiSettingsManager settings)
        {
            _settings = settings.LoadSettings();
            

            var folder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            _cacheFullName = Path.Combine(folder, CacheFileName);
        }
        
        private readonly string _cacheFullName;
        
        private const string CacheFileName = "ripe.cache.json";
        
        public void Save(List<RpkiRoa> rpkiRoa)
        {
            var cacheFile = new CacheFile {RpkiRoa = rpkiRoa};
            var json = JsonConvert.SerializeObject(cacheFile, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

            File.WriteAllText(_cacheFullName, json);
        }

        public void Save(Dictionary<RpkiResource, string> resources)
        {
            throw new NotImplementedException();
        }

        public List<RpkiRoa> LoadRpkiRoas()
        {
            if (!File.Exists(_cacheFullName))
                return null;
            
            try
            {
                var content = File.ReadAllText(_cacheFullName);

                var file = JsonConvert.DeserializeObject<CacheFile>(content, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

                return file.RpkiRoa;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public Dictionary<RpkiResource, string> LoadRpkiResources()
        {
            throw new NotImplementedException();
        }

        public void DropRoasCache()
        {
            if (File.Exists(_cacheFullName))
                File.Delete(_cacheFullName);
        }
    }
    
    public interface IRipeRouteManager
    {
        public RpkiResources GetRpkiResources(bool allowCache = false);
        public IEnumerable<RpkiRoa> GetRpkiRoas(bool allowCache = false);
        public RipeRouteRPKI GetRpkiState(RipeRoute resource);
    }

    public interface IRipeRPKIRoute
    {
        
    }
    

    public interface IRipeDatabaseRoute
    {
        public Task Add(string route, string origin);
        public Task Remove(string route, string origin);
        
        public Task Add(RipeRoute route);
        public Task Remove(RipeRoute route);
    }

    public class RipeRouteManager : IRipeRouteManager, IRipeDatabaseRoute
    {
        private readonly RpkiSettings _settings;
        private readonly IRipeRpkiClient _clientRpki;
        private readonly IRipeClient _clientRipe;
        private readonly ICacheManager _cacheManager;


        private Dictionary<RpkiResource, string> _cache;
        
        public RipeRouteManager(IRipeRpkiSettingsManager settings,
            IRipeRpkiClient clientRpki,
            IRipeClient clientRipe,
                ICacheManager cache)
        {
            _settings = settings.LoadSettings();
            _clientRpki = clientRpki;
            _clientRipe = clientRipe;
            _cacheManager = cache;
        }
        
        public RpkiResources GetRpkiResources(bool allowCache = false)
        {
            var rpkiResources = new RpkiResources { Resources = new List<RpkiResource>() };

            if (_cache != null)
            {
                rpkiResources.Resources.AddRange(
                    _cache.Select(c => c.Key)
                );
            }
            else
            {
                _cache = new Dictionary<RpkiResource, string>();

                foreach (var apiKey in _settings.Keys)
                {
                    var resourcesPlain = _clientRpki.GetResources(apiKey);

                    foreach (var rpkiResource in resourcesPlain.Resources)
                    {
                        //its as number
                        if (rpkiResource.StartsWith("AS"))
                        {
                            rpkiResources.Resources.Add(new RpkiResourceAsn{Asn = rpkiResource});
                            continue;
                        }

                        if (rpkiResource.Contains("-"))
                        {
                            Console.WriteLine($"We got network - {rpkiResource}, cannot parce now. :(");
                            continue;
                        }

                        var network = IPNetwork2.Parse(rpkiResource);

                        if (network.AddressFamily == AddressFamily.InterNetwork)
                        {
                            rpkiResources.Resources.Add(new RpkiResourceIPv4 {Inetnum = rpkiResource});
                            continue;
                        }

                        if (network.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            rpkiResources.Resources.Add(new RpkiResourceIPv6 {Inetnum6 = rpkiResource });
                            continue;
                        }

                        throw new ArgumentException($"Object not supported - {rpkiResource}");
                    }

                    //Saving to cache
                    foreach (var rpkiResource in rpkiResources.Resources)
                    {
                        if (!_cache.ContainsKey(rpkiResource))
                            _cache.Add(rpkiResource, apiKey);
                    }
                    //end - Saving to cache
                }
            }

            return rpkiResources;
        }
       
        public IEnumerable<RpkiRoa> GetRpkiRoas(bool allowCache = false)
        {
            List<RpkiRoa> result;
            
            if (allowCache)
            {
                 result = _cacheManager.LoadRpkiRoas();

                 if (result != null)
                     return result;
            }
            
            result = new List<RpkiRoa>();
            
            foreach (var apiKey in _settings.Keys)
            {
                var roas = _clientRpki.GetRoas(apiKey);

                foreach (var rpkiRoaPlain in roas.ToList())
                {
                    var network = IPNetwork2.Parse(rpkiRoaPlain.prefix);

                    if (network.AddressFamily == AddressFamily.InterNetwork)
                    {
                        result.Add(new RpkiRoaIpv4(
                            rpkiRoaPlain._numberOfValidsCaused,
                            rpkiRoaPlain._numberOfInvalidsCaused,
                            network.Cidr)
                        {
                            Asn = rpkiRoaPlain.asn,
                            Prefix = rpkiRoaPlain.prefix,
                            MaximalLength = rpkiRoaPlain.maximalLength
                        });
                        continue;
                    } 
                    if (network.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        result.Add(new RpkiRoaIpv6(
                            rpkiRoaPlain._numberOfValidsCaused,
                            rpkiRoaPlain._numberOfInvalidsCaused,
                            network.Cidr)
                        {
                            Asn = rpkiRoaPlain.asn,
                            Prefix = rpkiRoaPlain.prefix,
                            MaximalLength = rpkiRoaPlain.maximalLength
                        });
                        continue;
                    }

                    throw new ArgumentOutOfRangeException("prefix", $"Resource type {network.AddressFamily}");
                }
            }

            _cacheManager.Save(result);

            return result;
        }

        public RipeRouteRPKI GetRpkiState(RipeRoute route)
        {
            var roas = GetRpkiRoas(true);

            var prefixRoute = roas.Where(r => r.Prefix == route["route"]).ToList();

            // we have rpki records
            if (prefixRoute.Any())
            {
                var result = RipeRouteRPKI.Invalid;

                foreach (var rpkiRoa in prefixRoute)
                {
                    if (route["origin"] == rpkiRoa.Asn)
                    {
                        result = RipeRouteRPKI.Valid;
                        break;
                    }
                }

                return result;
            }
            
            return RipeRouteRPKI.Unknown;
        }

        protected string GetRpkiKey(string network)
        {
            foreach (var keyValuePair in _cache)
            {
                if (keyValuePair.Key.ToString() == network)
                    return keyValuePair.Value;
            }

            return null;
        }

        private string FindKey(string route)
        {
            var net = IPNetwork2.Parse(route);

            string key = "";

            //getting api key
            var rpkiResources = GetRpkiResources(true);

            // version 4
            if (net.AddressFamily == AddressFamily.InterNetwork)
            {
                var ipv4 = rpkiResources.Resources.OfType<RpkiResourceIPv4>();

                foreach (var rpkiResourceIPv4 in ipv4)
                {
                    var netRpki = IPNetwork2.Parse(rpkiResourceIPv4.Inetnum);

                    if (netRpki.Contains(net))
                    {
                        key = GetRpkiKey(netRpki.ToString());
                        break;
                    }
                }
            }

            return key;
        }

        public async Task Add(string route, string origin)
        {
            var routeObject = new RipeRoute(route, origin);
            await Add(routeObject);
            // ----- END DATABASE
            
            var key = FindKey(route);

            // We don't have RPKI key for this network
            if (string.IsNullOrEmpty(key)) return;
            
            var rpkiOperations = new RpkiOperations();
            rpkiOperations.Added.Add(new PublishRpkiRoaPlain
            {
                Prefix = route, 
                Asn = origin, 
                
                // Load from config
                MaximalLength = "32"
            });

            _cacheManager.DropRoasCache();
            await _clientRpki.RpkiOperation(key, rpkiOperations);
        }

        public async Task Add(RipeRoute route)
        {
            var raw = await _clientRipe.AddObject(route);
        }

        public async Task Remove(RipeRoute route)
        {
            await _clientRipe.RemoveObject(route);
        }

        public async Task Remove(string route, string origin)
        {
            var routeObject = new RipeRoute(route, origin);
            await Remove(routeObject);
        }
    }
}