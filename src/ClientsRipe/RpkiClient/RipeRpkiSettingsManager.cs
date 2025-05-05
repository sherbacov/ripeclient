using Microsoft.Extensions.Configuration;
using NodaTime.Text;


namespace ClientsRpki
{
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
}