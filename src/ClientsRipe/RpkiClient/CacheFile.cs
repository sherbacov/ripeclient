using System.Collections.Generic;
using ClientsRipe.RpkiClient.Models;


namespace ClientsRpki
{
    class CacheFile
    {
        public List<RpkiRoa> RpkiRoa { get; set; }
        public string Resources { get; set; }
    }
}