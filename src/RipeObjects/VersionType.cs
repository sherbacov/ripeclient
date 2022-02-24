using System;
using Newtonsoft.Json;

namespace RipeDatabaseObjects
{
    public class VersionType
    {
        public string Version { get; set; }
        public DateTime Timestamp { get; set; }

        [JsonProperty("commit-id")]
        public string CommitId { get; set; }
    }
}