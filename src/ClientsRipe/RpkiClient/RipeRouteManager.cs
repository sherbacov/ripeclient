using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ClientsRipe;
using ClientsRipe.RpkiClient.Models;
using RipeDatabaseObjects;


namespace ClientsRpki
{
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

                        if (rpkiResource.Contains('-'))
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

        public async Task Add(string route, string origin, CancellationToken cancellationToken)
        {
            var routeObject = new RipeRoute(route, origin);
            await Add(routeObject, cancellationToken);
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

        public async Task Add(RipeRoute route, CancellationToken cancellationToken)
        {
            var raw = await _clientRipe.AddObject(route, cancellationToken);
        }

        public async Task Remove(RipeRoute route, CancellationToken cancellationToken)
        {
            await _clientRipe.RemoveObject(route, cancellationToken);
        }

        public async Task Remove(string route, string origin, CancellationToken cancellationToken)
        {
            var routeObject = new RipeRoute(route, origin);
            await Remove(routeObject, cancellationToken);
        }
    }
}