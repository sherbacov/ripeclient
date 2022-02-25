using System.Collections.Generic;

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