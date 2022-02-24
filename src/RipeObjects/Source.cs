using System.Xml.Serialization;

namespace RipeDatabaseObjects
{
    public class Source
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
    }
}