using Newtonsoft.Json;

namespace RipeDatabaseObjects
{
    public class Parameters
    {
        [JsonProperty("inverse-lookup")]
        public InverseLookup InverseLookup { get; set; }

        [JsonProperty("type-filters")]
        public TypeFilters TypeFilters { get; set; }
        public Flags Flags { get; set; }

        [JsonProperty("query-strings")]
        public QueryStrings QueryStrings { get; set; }
        public Sources Sources { get; set; }
    }
}