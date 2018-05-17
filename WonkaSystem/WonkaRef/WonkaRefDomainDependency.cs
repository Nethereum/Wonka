using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace WonkaRef
{
    /// <summary>
    /// 
    /// This class represents the abstract relationship between the domain of one attribute
    /// and the domain of another attribute.  For example, when a product record is submitted
    /// so that it may be saved, its PhysicalFormat value must have a certain corresponding 
    /// DisplayFormat value in the record.  Otherwise, the record will be rejected.
    ///     
    /// </summary>
    [DataContract(Namespace = "http://sample.wonkasystem.com")]
    [XmlType(TypeName = "DomainDependency")]
    public class WonkaRefDomainDependency
    {
        public WonkaRefDomainDependency()
        {
            CodeDependencyId = -1;

            ParentAttrName = ChildAttrName = null;

            Optional = false;
        }

        [DataMember]
        public int CodeDependencyId { get; set; }

        [DataMember]
        public string ParentAttrName { get; set; }

        [DataMember]
        public string ChildAttrName { get; set; }

        [DataMember]
        public bool Optional { get; set; }

    }
}
