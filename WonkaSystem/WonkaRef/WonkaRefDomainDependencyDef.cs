using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WonkaRef
{
    /// <summary>
    /// 
    /// This class represents an instance of the relationship defined by a 
    /// WonkaRefDomainDependency class.
    ///     
    /// </summary>
    [DataContract(Namespace = "http://sample.wonkasystem.com")]
    [XmlType(TypeName = "DomainDependencyDef")]
    public class WonkaRefDomainDependencyDef
    {
        public WonkaRefDomainDependencyDef()
        {
            CodeDependencyDefId = -1;

            ParentAttrName = ParentAttrValue = null;
            ChildAttrName  = ChildAttrValue  = null;
        }

        [DataMember]
        public int CodeDependencyDefId { get; set; }

        [DataMember]
        public string ParentAttrName { get; set; }

        [DataMember]
        public string ParentAttrValue { get; set; }

        [DataMember]
        public string ChildAttrName { get; set; }

        [DataMember]
        public string ChildAttrValue { get; set; }

    }
}
