using System.Collections.Generic;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace RipeDatabaseObjects
{
    public class DatabaseObject 
    {
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
        public Link Link { get; set; }

        [XmlElement(ElementName = "source")]
        public Source Source { get; set; }

        [JsonProperty("primary-key")]
        public PrimaryKey PrimaryKey { get; set; }
        
        [XmlElement(ElementName = "attributes")]
        public Attributes Attributes { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Type))
            {
                return $"[{Type}] {PrimaryKey}";
            }
            
            return base.ToString();
        }
    }


    public class DatabaseWhoisObject
    {
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
        public Link Link { get; set; }

        [XmlElement(ElementName = "source")]
        public Source Source { get; set; }

        [XmlElement(ElementName = "attributes")]
        public WhoisAttributes Attributes { get; set; }
    }


    [XmlRoot(ElementName = "attributes")]
    public class WhoisAttributes
    {
        [XmlElement(ElementName = "attribute")]
        public List<Attribute> Attribute { get; set; }
    }

}