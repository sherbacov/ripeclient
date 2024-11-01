using System.Collections.Generic;
using ClientsRipe.RpkiClient.Models;
using RipeDatabaseObjects;


namespace ClientsRpki
{
    public interface IRipeRouteManager
    {
        public RpkiResources GetRpkiResources(bool allowCache = false);
        public IEnumerable<RpkiRoa> GetRpkiRoas(bool allowCache = false);
        public RipeRouteRPKI GetRpkiState(RipeRoute resource);
    }
}