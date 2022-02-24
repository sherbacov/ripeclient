using System.Collections.Generic;
using Newtonsoft.Json;

namespace RipeDatabaseObjects
{
    public class TypeFilters
    {
        [JsonProperty("type-filter")]
        public List<TypeFilter> TypeFilter { get; set; }
    }
}