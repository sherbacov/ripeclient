using System.Collections.Generic;
using System.Xml.Serialization;

namespace RipeDatabaseObjects
{
    public class Objects
    {
        [XmlElement(ElementName = "object")]
        public List<DatabaseObject> Object { get; set; }
    }

    public class WhoisObjects
    {
        [XmlElement(ElementName = "object")]
        public List<DatabaseWhoisObject> Object { get; set; }
    }
}