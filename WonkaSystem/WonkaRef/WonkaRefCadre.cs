using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Wonka.MetaData
{
    /// <summary>
    /// 
    /// Attributes are observed collectively in two different ways: Groups and Cadres.  The Cadre 
    /// represents an abstract way of regarding Attributes from the perspective of security and auditing.
    /// For example, all Attributes related to price information belong to the Price cadre.  When we
    /// apply permissions or conduct auditing onto the Price cadre, we then perform actions on all
    /// of the attributes as a whole.
    /// 
    /// NOTE: In some cases, Groups and Cadres are mirror copies of each other.
    /// 
    /// NOTE: Groups can be supersets of Cadres, but not vice versa.
    ///     
    /// </summary>
    [DataContract(Namespace = "http://sample.wonkasystem.com")]
    [XmlType(TypeName = "Cadre")]
    public class WonkaRefCadre
    {
        public WonkaRefCadre()
        {
            CadreId   = GroupId = -1;
            CadreName = Description = DisplayName = null;

            MergeNullAttrFlag = AudTrxTotalsFlag = false;

            AttrIds          = new List<int>();
            TriggerOverrides = new List<int>();
        }

        [DataMember]
        public int CadreId { get; set; }

        [DataMember]
        public int GroupId { get; set; }

        [DataMember]
        public string CadreName { get; set; }

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