using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

using WonkaRef;

namespace WonkaRef.Extensions
{
    /// <summary>
    /// 
    /// This class will help instantiate the Ref environment (singleton) by parsing the serialized instance
    /// from a previous session.
    /// 
    /// </summary>
    public class WonkaRefDeserializeLocalSource : IMetadataRetrievable
    {
        private string            msMetadataFileContents = null;
        private XElement          moMetadataXmlElement   = null;

        private XmlDocument       moXmlDoc = null;

        public WonkaRefDeserializeLocalSource(FileInfo poMetadataConfigFile) : this(File.ReadAllText(poMetadataConfigFile.FullName))
        { }

        public WonkaRefDeserializeLocalSource(string psMetadataConfigFileText)
        {
            msMetadataFileContents = psMetadataConfigFileText;
            moMetadataXmlElement   = XElement.Parse(psMetadataConfigFileText);

            using (StringReader MetadataReader = new StringReader(psMetadataConfigFileText))
            {
                moXmlDoc = new XmlDocument();
                moXmlDoc.Load(MetadataReader);
            }
        }

        #region Required Interface Methods

        #region Standard Metadata Cache (Minimum Set)

        public List<WonkaRefAttr> GetAttrCache()
        {
            List<WonkaRefAttr> AttrCache      = new List<WonkaRefAttr>();
            XmlSerializer      AttrSerializer = new XmlSerializer(typeof(WonkaRefAttr));

            XmlNodeList AttrNodeList = moXmlDoc.GetElementsByTagName("Attr");
            foreach (XmlNode AttrNode in AttrNodeList)
            {
                WonkaRefAttr TempAttr = (WonkaRefAttr) AttrSerializer.Deserialize(new StringReader(AttrNode.OuterXml));

                AttrCache.Add(TempAttr);
            }

            return AttrCache;
        }

        public List<WonkaRefCurrency> GetCurrencyCache()
        {
            List<WonkaRefCurrency> CurrencyCache      = new List<WonkaRefCurrency>();
            XmlSerializer          CurrencySerializer = new XmlSerializer(typeof(WonkaRefCurrency));

            XmlNodeList CurrencyNodeList = moXmlDoc.GetElementsByTagName("WonkaRefCurrency");
            foreach (XmlNode CurrencyNode in CurrencyNodeList)
            {
                WonkaRefCurrency TempCurrency = 
                    (WonkaRefCurrency )CurrencySerializer.Deserialize(new StringReader(CurrencyNode.OuterXml));

                CurrencyCache.Add(TempCurrency);
            }

            return CurrencyCache;
        }

        public List<WonkaRefField> GetFieldCache()
        {
            List<WonkaRefField> FieldCache      = new List<WonkaRefField>();
            XmlSerializer       FieldSerializer = new XmlSerializer(typeof(WonkaRefField));

            XmlNodeList FieldNodeList = moXmlDoc.GetElementsByTagName("Field");
            foreach (XmlNode FieldNode in FieldNodeList)
            {
                WonkaRefField TempField = (WonkaRefField) FieldSerializer.Deserialize(new StringReader(FieldNode.OuterXml));

                FieldCache.Add(TempField);
            }

            return FieldCache;
        }

        public List<WonkaRefGroup> GetGroupCache()
        {
            List<WonkaRefGroup> GroupCache      = new List<WonkaRefGroup>();
            XmlSerializer       GroupSerializer = new XmlSerializer(typeof(WonkaRefGroup));

            XmlNodeList GroupNodeList = moXmlDoc.GetElementsByTagName("Group");
            foreach (XmlNode GroupNode in GroupNodeList)
            {
                WonkaRefGroup TempGroup = (WonkaRefGroup) GroupSerializer.Deserialize(new StringReader(GroupNode.OuterXml));

                GroupCache.Add(TempGroup);
            }

            return GroupCache;
        }

        public List<WonkaRefSource> GetSourceCache()
        {
            List<WonkaRefSource> SourceCache      = new List<WonkaRefSource>();
            XmlSerializer        SourceSerializer = new XmlSerializer(typeof(WonkaRefSource));

            XmlNodeList SourceNodeList = moXmlDoc.GetElementsByTagName("WonkaRefSource");
            foreach (XmlNode SourceNode in SourceNodeList)
            {
                WonkaRefSource TempSource = (WonkaRefSource) SourceSerializer.Deserialize(new StringReader(SourceNode.OuterXml));

                SourceCache.Add(TempSource);
            }

            return SourceCache;
        }

        public List<WonkaRefSourceField> GetSourceFieldCache()
        {
            List<WonkaRefSourceField> SrcFldCache      = new List<WonkaRefSourceField>();
            XmlSerializer             SrcFldSerializer = new XmlSerializer(typeof(WonkaRefSourceField));

            XmlNodeList SrcFldNodeList = moXmlDoc.GetElementsByTagName("WonkaRefSourceField");
            foreach (XmlNode SourceFieldNode in SrcFldNodeList)
            {
                WonkaRefSourceField TempSourceField = 
                    (WonkaRefSourceField) SrcFldSerializer.Deserialize(new StringReader(SourceFieldNode.OuterXml));

                SrcFldCache.Add(TempSourceField);
            }

            return SrcFldCache;
        }

        public List<WonkaRefStandard> GetStandardCache()
        {
            List<WonkaRefStandard> StandardCache      = new List<WonkaRefStandard>();
            XmlSerializer          StandardSerializer = new XmlSerializer(typeof(WonkaRefStandard));

            XmlNodeList StdNodeList = moXmlDoc.GetElementsByTagName("WonkaRefStandard");
            foreach (XmlNode StandardNode in StdNodeList)
            {
                WonkaRefStandard TempStandard = 
                    (WonkaRefStandard) StandardSerializer.Deserialize(new StringReader(StandardNode.OuterXml));

                StandardCache.Add(TempStandard);
            }

            return StandardCache;
        }

        #endregion

        #region Extended Metadata Cache

        public List<WonkaRefAttrCollection> GetAttrCollectionCache()
        {
            List<WonkaRefAttrCollection> AttrCollCache      = new List<WonkaRefAttrCollection>();
            XmlSerializer                AttrCollSerializer = new XmlSerializer(typeof(WonkaRefAttrCollection));

            XmlNodeList AttrCollNodeList = moXmlDoc.GetElementsByTagName("AttrCollection");
            foreach (XmlNode AttrCollNode in AttrCollNodeList)
            {
                WonkaRefAttrCollection TempAttrColl = 
                    (WonkaRefAttrCollection) AttrCollSerializer.Deserialize(new StringReader(AttrCollNode.OuterXml));

                AttrCollCache.Add(TempAttrColl);
            }

            return AttrCollCache;
        }

        #endregion

        #endregion
    }
}