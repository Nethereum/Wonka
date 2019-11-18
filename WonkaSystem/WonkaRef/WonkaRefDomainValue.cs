using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Wonka.MetaData
{
    /// <summary>
    /// 
    /// This class represents one possible value for an attribute that has
    /// an assigned domain.
    ///     
    /// </summary>
    [DataContract(Namespace = "http://sample.wonkasystem.com")]
    [XmlType(TypeName = "DomainValue")]
    public class WonkaRefDomainValue
    {
        public WonkaRefDomainValue()
        {
            CodeDefId = -1;

            AttrName = AttrValue = StatusCd = Description = null;
        }

        [DataMember]
        public int CodeDefId { get; set; }

        [DataMember]
        public string AttrName { get; set; }

        [DataMember]
        public string AttrValue { get; set; }

        [DataMember]
        public string StatusCd { get; set; }

        [DataMember]
        public string Description { get; set; }
    }
}
