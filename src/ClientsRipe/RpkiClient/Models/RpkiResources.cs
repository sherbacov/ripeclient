using System.Collections.Generic;
using Newtonsoft.Json;
using RipeDatabaseObjects;

namespace RipeRpkiObjects
{
    public class RpkiResource
    {
    }
    
    public class RpkiResourceAsn : RpkiResource {
        public string Asn { get; set; }

        public override string ToString()
        {
            return Asn;
        }
    }

    public class RpkiResourceIPv4 : RpkiResource {

        public string Inetnum { get; set; }

        public override string ToString()
        {
            return Inetnum;
        }
    }

    public class RpkiResourceIPv6 : RpkiResource {

        public string Inetnum6 { get; set; }
        
        public override string ToString()
        {
            return Inetnum6;
        }
    }

    public class RpkiResources
    {
        public List<RpkiResource> Resources { get; set; }
    }

    public enum RpkiResourceState
    {
        Unknown,
        Valid,
        Invalid
    }

    public class PublishRpkiRoaPlain
    {
        public PublishRpkiRoaPlain() {}

        public PublishRpkiRoaPlain(string asn, string prefix, int maximalLength)
        {
            Asn = asn;
            Prefix = prefix;
            MaximalLength = maximalLength.ToString();
        }
        
        public string Asn { get; set; }
        public string Prefix { get; set; }
        public string MaximalLength { get; set; }
    }

    public class RpkiOperations
    {
        public RpkiOperations()
        {
            Added = new List<PublishRpkiRoaPlain>();
            Deleted = new List<PublishRpkiRoaPlain>();
        }

        public RpkiOperations Add(PublishRpkiRoaPlain roa)
        {
            Added.Add(roa);
            
            return this;
        }

        public RpkiOperations Delete(PublishRpkiRoaPlain roa)
        {
            Deleted.Add(roa);
            
            return this;
        }
        
        public List<PublishRpkiRoaPlain> Added { get; set; }
        public List<PublishRpkiRoaPlain> Deleted { get; set; }
    }

    public class RpkiRoaPlain
    {
        public string asn { get; set; }
        public string prefix { get; set; }
        public int maximalLength { get; set; }
        public int _numberOfValidsCaused { get; set; }
        public int _numberOfInvalidsCaused { get; set; }
    }

    public class  RpkiRoa : IRipeObject
    {
        public RpkiRoa() { }
        
        public RpkiRoa(int valid, int invalid, byte netmask)
        {
            _valid = valid;
            _invalid = invalid;
            Netmask = netmask;
        }

        private int _valid;
        private int _invalid;

        public byte Netmask { get; set; }

        public string Prefix { get; set; }
        public string Asn { get; set; }
        public int MaximalLength { get; set; }

        public string GetRipeObjectType()
        {
            //now using only at RIPE Database
            throw new System.NotImplementedException();
        }

        [JsonIgnore]
        public string[] ObjectKeys => new string[] {"Prefix", "Asn", "MaximalLength"};

        public int NetworkNetmask()
        {
            return Netmask;
        }
        
        
        public int ValidsCaused()
        {
            return _valid;
        }

        public int InvalidsCaused()
        {
            return _invalid;
        }

        public string GetKey(bool allowSpaces)
        {
            if (Netmask != MaximalLength) 
                return $"{Prefix} {Asn} [{MaximalLength}]";


            return $"{Prefix} {Asn}";
        }
    }

    public class RpkiRoaIpv4 : RpkiRoa
    {
        public RpkiRoaIpv4() { }
        public RpkiRoaIpv4(int valid, int invalid, byte netmask) : base(valid, invalid,netmask) { }
    }

    public class RpkiRoaIpv6 : RpkiRoa
    {
        public RpkiRoaIpv6() { }
        public RpkiRoaIpv6(int valid, int invalid, byte netmask) : base(valid, invalid, netmask) { }
    }

    public class RpkiResourcesPlain
    {
        public List<string> Resources { get; set; }
    }

}
