using System;
using System.Collections.Generic;
using System.IO;
using ClientsRipe.RpkiClient.Models;
using Newtonsoft.Json;


namespace ClientsRpki
{
    public class CacheManager : ICacheManager
    {
        private readonly RpkiSettings _settings;

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
}