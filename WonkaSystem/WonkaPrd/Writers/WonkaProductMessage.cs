using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

using Wonka.MetaData;

namespace Wonka.Product.Writers
{
    /// <summary>
    /// 
    /// This class will represent an instance of a Wonka Product message.
    /// 
    /// </summary>
    [DataContract(Namespace = "http://sample.wonkasystem.com")]
    [XmlRoot(ElementName = "WonkaMessage")]
    public class WonkaProductMessage
    {
        public WonkaProductMessage(bool pbPopulateMetadata = true)
        {
            ProductCount    = 0;
            CommitThreshold = 500;

            AltXmlMessage = ExtractSQL = "";

            AttrCollectionList     = new List<WonkaRefAttrCollection>();
            CopyAttrCollectionList = new List<WonkaRefAttrCollection>();

            AttrList    = new List<WonkaRefAttr>();

            DomainValueList         = new List<WonkaRefDomainValue>();
            DomainDependencyList    = new List<WonkaRefDomainDependency>();
            DomainDependencyDefList = new List<WonkaRefDomainDependencyDef>();

            CurrencyList   = new List<WonkaRefCurrency>();
            CadreList      = new List<WonkaRefCadre>();
            GroupList      = new List<WonkaRefGroup>();

            QueryCriteriaList     = new List<WonkaRefQueryCriteria>();
            QueryProductCadreList = new List<WonkaRefQueryProductCadre>();
            QueryErrorList        = new List<WonkaRefQueryError>();
            SourceList            = new List<WonkaRefSource>();
            SourceCadreList       = new List<WonkaRefSourceCadre>();
            StandardList          = new List<WonkaRefStandard>();

            ProdListXml = "";
            ProdList    = new List<WonkaProduct>();

            ErrorMessage = null;

            if (pbPopulateMetadata)
			{
                WonkaRefEnvironment WonkaRefEnv = WonkaRefEnvironment.GetInstance();

                AttrList.AddRange(WonkaRefEnv.AttrCache);
                GroupList.AddRange(WonkaRefEnv.GroupCache);
                CadreList.AddRange(WonkaRefEnv.CadreCache);

                SourceList.AddRange(WonkaRefEnv.SourceCache);
                SourceCadreList.AddRange(WonkaRefEnv.SourceCadreCache);
            }
        }

        #region Methods

        #endregion

        #region Properties

        [XmlIgnoreAttribute]
        public int ProductCount { get; set; }

        [XmlIgnoreAttribute]
        public int CommitThreshold { get; set; }

        [XmlIgnoreAttribute]
        public string AltXmlMessage { get; set; }

        [XmlIgnoreAttribute]
        public string ExtractSQL { get; set; }

        /*
         * NOTE: Should this option be used?
         * 
        [DataMember(Order = 1)]
        public WonkaAction Action { get; set; }
         */

        [DataMember(Order = 2)]
        public List<WonkaRefAttrCollection> AttrCollectionList { get; set; }

        public bool ShouldSerializeAttrCollectionList()
        {
            return (AttrCollectionList != null) && (AttrCollectionList.Any());
        }

        [DataMember(Order = 3)]
        public List<WonkaRefAttrCollection> CopyAttrCollectionList { get; set; }

        public bool ShouldSerializeCopyAttrCollectionList()
        {
            return (CopyAttrCollectionList != null) && (CopyAttrCollectionList.Any());
        }

        [DataMember(Order = 4)]
        public List<WonkaRefAttr> AttrList { get; set; }

        public bool ShouldSerializeAttrList()
        {
            return (AttrList != null) && (AttrList.Any());
        }

        [DataMember(Order = 5)]
        public List<WonkaRefDomainValue> DomainValueList { get; set; }

        public bool ShouldSerializeDomainValueList()
        {
            return (DomainValueList != null) && (DomainValueList.Any());
        }

        [DataMember(Order = 6)]
        public List<WonkaRefDomainDependency> DomainDependencyList { get; set; }

        public bool ShouldSerializeDomainDependencyList()
        {
            return (DomainDependencyList != null) && (DomainDependencyList.Any());
        }

        [DataMember(Order = 7)]
        public List<WonkaRefDomainDependencyDef> DomainDependencyDefList { get; set; }

        public bool ShouldSerializeDomainDependencyDefList()
        {
            return (DomainDependencyDefList != null) && (DomainDependencyDefList.Any());
        }

        [DataMember(Order = 8)]
        public List<WonkaRefCurrency> CurrencyList { get; set; }

        public bool ShouldSerializeCurrencyList()
        {
            return (CurrencyList != null) && (CurrencyList.Any());
        }

        [DataMember(Order = 9)]
        public List<WonkaRefCadre> CadreList { get; set; }

        public bool ShouldSerializeCadreList()
        {
            return (CadreList != null) && (CadreList.Any());
        }

        [DataMember(Order = 10)]
        public List<WonkaRefGroup> GroupList { get; set; }

        public bool ShouldSerializeGroupList()
        {
            return (GroupList != null) && (GroupList.Any());
        }

        [DataMember(Order = 11)]
        public List<WonkaRefQueryCriteria> QueryCriteriaList { get; set; }

        public bool ShouldSerializeQueryCriteriaList()
        {
            return (QueryCriteriaList != null) && (QueryCriteriaList.Any());
        }

        [DataMember(Order = 12)]
        public List<WonkaRefQueryProductCadre> QueryProductCadreList { get; set; }

        public bool ShouldSerializeQueryProductCadreList()
        {
            return (QueryProductCadreList != null) && (QueryProductCadreList.Any());
        }

        [DataMember(Order = 13)]
        public List<WonkaRefQueryError> QueryErrorList { get; set; }

        public bool ShouldSerializeQueryErrorList()
        {
            return (QueryErrorList != null) && (QueryErrorList.Any());
        }

        [DataMember(Order = 14)]
        public List<WonkaRefSource> SourceList { get; set; }

        public bool ShouldSerializeSourceList()
        {
            return (SourceList != null) && (SourceList.Any());
        }

        [DataMember(Order = 15)]
        public List<WonkaRefSourceCadre> SourceCadreList { get; set; }

        public bool ShouldSerializeSourceCadreList()
        {
            return (SourceCadreList != null) && (SourceCadreList.Any());
        }

        [DataMember(Order = 16)]
        public List<WonkaRefStandard> StandardList { get; set; }

        public bool ShouldSerializeStandardList()
        {
            return (StandardList != null) && (StandardList.Any());
        }

        [XmlIgnore]
        public string ProdListXml { get; set; }

        [XmlIgnore]
        public List<WonkaProduct> ProdList { get; set; }

        [DataMember(Name = "SystemError", Order = 17)]
        public string ErrorMessage { get; set; }

        #endregion
    }
}
