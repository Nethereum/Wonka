using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WonkaRef
{
    /// <summary>
    /// 
    /// This class represents a distinct data point within the Wonka System 
    /// (like UPC, Title, Price, etc.)
    ///     
    /// </summary>
    [DataContract(Namespace = "http://sample.wonkasystem.com")]
    [XmlType(TypeName = "Attr")]
    public class WonkaRefAttr
    {
        public WonkaRefAttr()
        {
            AttrId = -1;

            AttrName = TabCol  = DisplayName = null;
            TabName  = ColName = Description = null;
            
            IsKey = false;

            FieldId = GroupId = MaxLength = -1;

            Precision = Scale = -1;

            DefaultValue = MinValue = MaxValue = AttrModDt = null;

            MaxLengthTruncate = false;
            AttrModDtFlag     = false;

            Properties = new HashSet<string>();

            IsAudited      = IsDate    = IsDecimal  = false;
            IsMaintainable = IsNumeric = IsDomainCd = false;

            RefCdDomainAttrName = null;
        }

        [DataMember]
        public int AttrId { get; set; }

        [DataMember]
        public string AttrName { get; set; }
        
        [DataMember]
        public bool IsKey { get; set; }

        [DataMember]
        public string TabCol { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string TabName { get; set; }

        [DataMember]
        public string ColName { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int FieldId { get; set; }

        [DataMember]
        public int GroupId { get; set; }

        [DataMember]
        public int MaxLength { get; set; }

        [DataMember]
        public bool MaxLengthTruncate { get; set; }

        [DataMember]
        public int Precision { get; set; }

        [DataMember]
        public int Scale { get; set; }

        [DataMember]
        public string DefaultValue { get; set; }

        [DataMember]
        public string MinValue { get; set; }

        [DataMember]
        public string MaxValue { get; set; }

        [DataMember]
        public string AttrModDt { get; set; }

        [DataMember]
        public bool AttrModDtFlag { get; set; }

        [DataMember]
        public HashSet<string> Properties { get; set; }

        [DataMember]
        public bool IsDate { get; set; }

        [DataMember]
        public bool IsDecimal { get; set; }

        [DataMember]
        public bool IsNumeric { get; set; }

        [DataMember]
        public bool IsDomainCd { get; set; }

        [DataMember]
        public bool IsMaintainable { get; set; }

        [DataMember]
        public bool IsAudited { get; set; }

        [DataMember]
        public string RefCdDomainAttrName { get; set; }
    }
}
