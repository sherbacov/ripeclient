using Newtonsoft.Json;

namespace RipeDatabaseObjects
{
    public class RipeDatabaseSearchObjects
    {
        public RipeService Service { get; set; }
        public Parameters Parameters { get; set; }
        public Objects Objects { get; set; }

        [JsonProperty("terms-and-conditions")]
        public TermsAndConditions TermsAndConditions { get; set; }
        public VersionType Version { get; set; }
    }
}