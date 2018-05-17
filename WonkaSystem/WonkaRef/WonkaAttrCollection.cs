using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace WonkaRef
{
    /// <summary>
    /// 
    /// This class represents an item within an Attribute Collection.
    ///     
    /// </summary>
    [DataContract(Namespace = "http://sample.wonkasystem.com")]
    [XmlType(TypeName = "AttrCollectionItem")]
    public class WonkaRefAttrCollectionItem
    {
        public WonkaRefAttrCollectionItem()
        {
            AttrName = null;
            Position = -1;
        }

        [DataMember]
        public string AttrName { get; set; }

        [DataMember]
        public int Position { get; set; }
    }

    /// <summary>
    /// 
    /// This class represents an Attribute Collection, which are collections of attributes
    /// that are used by more complex abstract entities.
    ///     
    /// </summary>
    [DataContract(Namespace = "http://sample.wonkasystem.com")]
    [XmlType(TypeName = "AttrCollection")]
    public class WonkaRefAttrCollection
    {
        public WonkaRefAttrCollection()
        {
            AttrCollectionId = -1;
            Items            = new List<WonkaRefAttrCollectionItem>();
        }

        [XmlIgnore]
        public int AttrCollectionId { get; set; }

        [DataMember]
        [XmlElement("AttrCollectionItem")]
        public List<WonkaRefAttrCollectionItem> Items { get; set; }
    }
}

