using System.Collections.Generic;
using ClientsRipe.RpkiClient.Models;


namespace ClientsRpki
{
    public interface ICacheManager
    {
        public void Save(List<RpkiRoa> rpkiRoa);
        public void Save(Dictionary<RpkiResource, string> resources);

        public List<RpkiRoa> LoadRpkiRoas();
        public Dictionary<RpkiResource, string> LoadRpkiResources();

        public void DropRoasCache();

    }
}