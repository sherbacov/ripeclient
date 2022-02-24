using System;
using System.Xml.Serialization;

namespace RipeDatabaseObjects
{
    [Serializable]
    [XmlRoot(ElementName = "whois-resources")]
    public class WhoisResources
    {
        [XmlElement(ElementName = "objects")]
        public WhoisObjects Objects { get; set; }
    }
}