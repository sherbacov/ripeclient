using System.Collections.Generic;
using Newtonsoft.Json;

namespace RipeDatabaseObjects
{
    public class QueryStrings
    {
        [JsonProperty("query-string")]
        public List<QueryString> QueryString { get; set; }
    }
}