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
    /// Attributes are observed collectively in two different ways: Groups and Fields.  The Group 
    /// represents an abstract way of regarding Attributes from the perspective of logical entities and persistence.
    /// For example, all Attributes related to contributor information belong to the Contributor group.  When we
    /// standardize or persist Contributor data, we then perform actions on all of the attributes as a whole.
    /// 
    /// NOTE: In some cases, Groups and Fields are mirror copies of each other.  For example, the Contributor group
    /// and the Contributor field are the same collection of attributes.
    /// 
    /// NOTE: Groups can be supersets of Fields, but not vice versa.
    /// 
    /// NOTE: Aside from a dedicated Main group (product identifier, name, etc.), all groups should have the potential of being multiple instances. 
    ///     
    /// </summary>
    [DataContract(Namespace = "http://sample.wonkasystem.com")]
    [XmlType(TypeName = "Group")]
    public class WonkaRefGroup
    {
        public WonkaRefGroup()
        {
            GroupId = -1;

            GroupName         = Description      = ProductTabName   = OuterTabName = null;
            InternalOuterJoin = ProductInnerJoin = ProductOuterJoin = null;

            IsStandardized  = IsSequenced  = IsMultiFieldSecured = false;
            IsStdBeautified = IsAppendable = CanFasttrackAppend  = false;

            KeyTabCols = new HashSet<string>();
            FieldIds   = new HashSet<int>();

            MaxRows = -1;
        }

        [DataMember]
        public int GroupId { get; set; }

        [DataMember]
        public string GroupName { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string ProductTabName { get; set; }

        [DataMember]
        public string OuterTabName { get; set; }

        [DataMember]
        public string InternalOuterJoin { get; set; }

        [DataMember]
        public string ProductInnerJoin { get; set; }

        [DataMember]
        public string ProductOuterJoin { get; set; }

        [DataMember]
        public bool IsStandardized { get; set; }

        [DataMember]
        public bool IsSequenced { get; set; }

        [DataMember]
        public bool IsMultiFieldSecured { get; set; }

        [DataMember]
        public bool IsStdBeautified { get; set; }

        [DataMember]
        public bool IsAppendable { get; set; }

        [DataMember]
        public bool CanFasttrackAppend { get; set; }

        [DataMember]
        public HashSet<string> KeyTabCols { get; set; }

        public int MaxRows { get; set; }

        HashSet<int> FieldIds { get; set; }

    }
}
