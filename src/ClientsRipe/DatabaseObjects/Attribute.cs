using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace RipeDatabaseObjects
{
    [XmlRoot(ElementName = "attribute")]
    public class Attribute
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }

        public Link Link { get; set; }

        [JsonProperty("referenced-type")]
        public string ReferencedType { get; set; }

        public override string ToString()
        {
            return $"{Name}={Value}";
        }
    }

    public enum RipeRouteRPKI
    {
        Unknown,
        Valid,
        Invalid
    }

    public class RipeInetnum : RipeObject
    {
        public RipeInetnum()
        {
            ObjectKeys = new[] { "inetnum" };
        }
        
        public RipeInetnum(string inetnum)
        {
            ObjectKeys = new[] { "inetnum" };

            Add(new KeyValuePair<string, string>("inetnum", inetnum ));
            Add(new KeyValuePair<string, string>("source", "RIPE" ));
        }
        
        public override string GetKey(bool allowSpaces)
        {
            if (!ObjectKeys.All(ContainsKey))
                throw new ArgumentOutOfRangeException();

            return this["inetnum"];
        }
        
        public override string GetRipeObjectType()
        {
            return "inetnum";
        }

        public void ClearMnts(IEnumerable<string> exceptMnts = null)
        {
            var exceptList = exceptMnts?.ToList();
            
            foreach (var mntPair in this.Where(k => k.Key == "mnt-by"))
            {
                if (exceptList == null)
                    Remove(mntPair);
                else if (!exceptList.Contains(mntPair.Value))
                {
                    Remove(mntPair);
                } 
            }
        }
    }

    public class RipeDomain : RipeObject
    {
        public RipeDomain()
        {
            ObjectKeys = new[] { "domain" };
        }
        
        public RipeDomain(string domain)
        {
            ObjectKeys = new[] { "domain" };

            Add(new KeyValuePair<string, string>("domain", domain ));
            Add(new KeyValuePair<string, string>("source", "RIPE" ));
        }        
        
        public string GetDomain()
        {
            return this["domain"];
        }

        public override string GetKey(bool allowSpaces)
        {
            return GetDomain();
        }

        public override string GetRipeObjectType()
        {
            return "domain";
        }

        public void AddNServer(string server)
        {
            Add(new KeyValuePair<string, string>("nserver", server ));
        }
    }
    
    public class RipeRoute : RipeObject
    {
        public RipeRoute()
        {
            ObjectKeys = new[] { "route", "origin" };
            RouteRPKI = RipeRouteRPKI.Unknown;
        }
        public RipeRoute(string route, string origin)
        {
            ObjectKeys = new[] { "route", "origin" };
            RouteRPKI = RipeRouteRPKI.Unknown;

            Add(new KeyValuePair<string, string>("route", route ));
            Add(new KeyValuePair<string, string>("origin", origin ));
            Add(new KeyValuePair<string, string>("source", "RIPE" ));
        }

        public string GetOrigin()
        {
            return this["origin"];
        }

        public RipeRouteRPKI RouteRPKI { get; set; }

        public override string GetRipeObjectType()
        {
            return "route";
        }

        public override string GetKey(bool allowSpaces)
        {
            if (!ObjectKeys.All(ContainsKey))
                throw new ArgumentOutOfRangeException();

            var route = this["route"];
            var origin = this["origin"];

            if (allowSpaces)
                return $"{route} {origin}";
            else
                return $"{route}{origin}";
        }
    }
}