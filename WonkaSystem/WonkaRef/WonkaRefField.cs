using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Wonka.MetaData
{
    /// <summary>
    /// 
    /// Attributes are observed collectively in two different ways: Groups and Fields.  The Field 
    /// represents an abstract way of regarding Attributes from the perspective of security and auditing.
    /// For example, all Attributes related to price information belong to the Price field.  When we
    /// apply permissions or conduct auditing onto the Price field, we then perform actions on all
    /// of the attributes as a whole.
    /// 
    /// NOTE: In some cases, Groups and Fields are mirror copies of each other.
    /// 
    /// NOTE: Groups can be supersets of Fields, but not vice versa.
    ///     
    /// </summary>
    [DataContract(Namespace = "http://sample.wonkasystem.com")]
    [XmlType(TypeName = "Field")]
    public class WonkaRefField
    {
        public WonkaRefField()
        {
            FieldId   = GroupId = -1;
            FieldName = Description = DisplayName = null;

            MergeNullAttrFlag = AudTrxTotalsFlag = false;

            AttrIds          = new List<int>();
            TriggerOverrides = new List<int>();
        }

        [DataMember]
        public int FieldId { get; set; }

        [DataMember]
        public int GroupId { get; set; }

        [DataMember]
        public string FieldName { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public bool MergeNullAttrFlag { get; set; }

        [DataMember]
        public bool AudTrxTotalsFlag { get; set; }

        [DataMember]
        public List<int> AttrIds { get; set; }

        [DataMember]
        public List<int> TriggerOverrides { get; set; }

    }
}