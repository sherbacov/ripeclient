using System.Collections.Generic;
using RipeRpkiObjects;

namespace ClientsRpki
{
    public class RpkiSettings
    {
        public RpkiSettings()
        {
            Keys = new List<string>();
        }
        
        public List<string> Keys { get; set; }

        public string MaxLength { get; set; }

        public int CacheTimeout { get; set; }
    }
}