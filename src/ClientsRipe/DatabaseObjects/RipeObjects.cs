using System.Collections.Generic;
using Newtonsoft.Json;

namespace RipeDatabaseObjects
{
    public class ErrorMessageArg
    {
        public string Value { get; set; }
    }

    public class ErrorMessage
    {
        public string Severity { get; set; }
        public string Text { get; set; }
        public Attribute Attribute { get; set; }
        public List<ErrorMessageArg> Args { get; set; }
    }

    public class ErrorMessages
    {
        public List<ErrorMessage> ErrorMessage { get; set; }
    }
    
    
    public class RipeObjects
    {
        public Objects Objects { get; set; }
        
        public ErrorMessages ErrorMessages { get; set; }

        [JsonProperty("terms-and-conditions")]
        public TermsAndConditions TermsAndConditions { get; set; }
        public VersionType Version { get; set; }
    }
}