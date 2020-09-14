using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

using Wonka.MetaData;

namespace Wonka.Product.Writers
{
    /// <summary>
    /// 
    /// This class will provide the functionality of writing a Wonka XML message,
    /// using an instance of the WonkaXmlMessage class for serialization.
    /// 
    /// </summary>
    [DataContract(Namespace = "http://sample.wonkasystem.com")]
    public class WonkaProductMsgWriter
    {
        #region Constants

        public static string CONST_PRODUCT_ID_TAG = "ProductId";

        #endregion

        #region Constructors

        public WonkaProductMsgWriter()
        {
            DefaultRootWriterSettings =
                new XmlWriterSettings()
                {
                    OmitXmlDeclaration = true,
                    Indent = true,
                    IndentChars = "  "
                };

            DefaultSubPrdWriterSettings =
                new XmlWriterSettings() { OmitXmlDeclaration = true,
                                          // ConformanceLevel = ConformanceLevel.Fragment,
                                          Indent = true,
                                          IndentChars = "        "
                                        };

            DefaultNamespaces = new XmlSerializerNamespaces();
            DefaultNamespaces.Add("", "");
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// This method will serialize a ProductCadre within a Product instance.
        /// 
        /// </summary>
        /// <param name="poTmpPrdCadre">The target ProductCadre being serialized</param>
        /// <param name="poProductListBuilder">The buffer that is holding the written WONKA-XML message</param>
        /// <returns>None</returns>
        private void AppendProductCadre(WonkaProductCadre poTmpPrdCadre, StringBuilder poProductListBuilder)
        {
            XmlSerializer Serializer     = new XmlSerializer(typeof(WonkaProductCadre));
            XmlWriter     WonkaXmlWriter = XmlWriter.Create(poProductListBuilder, DefaultSubPrdWriterSettings);

            poProductListBuilder.Append("\n      ");
            Serializer.Serialize(WonkaXmlWriter, poTmpPrdCadre, DefaultNamespaces);
        }

        /// <summary>
        /// 
        /// This method will serialize a ProductGroup within a Product instance.
        /// 
        /// </summary>
        /// <param name="poTmpPrdGroup">The target ProductGroup being serialized</param>
        /// <param name="poProductListBuilder">The buffer that is holding the written WONKA-XML message</param>
        /// <returns>None</returns>
        private void AppendProductGroup(WonkaPrdGroup poTmpPrdGroup, StringBuilder poProductListBuilder)
        {
            WonkaRefEnvironment WonkaRefEnv = WonkaRefEnvironment.GetInstance();

            if (poTmpPrdGroup.GetRowCount() > 0)
            {
                string sTmpAttrValue = null;

                foreach (WonkaPrdGroupDataRow TempDataRow in poTmpPrdGroup)
                {
                    poProductListBuilder.Append("\n      <" + poTmpPrdGroup.MasterGroup.GroupName + ">");

                    foreach (int nTmpAttrId in TempDataRow.Keys)
                    {
                        sTmpAttrValue = TempDataRow[nTmpAttrId];

                        if (!String.IsNullOrEmpty(sTmpAttrValue))
                        {
                            WonkaRefAttr TempAttribute = WonkaRefEnv.GetAttributeByAttrId(nTmpAttrId);

                            poProductListBuilder.Append("\n        <" + TempAttribute.AttrName + ">");
                            poProductListBuilder.Append(WrapWithCData(sTmpAttrValue));
                            poProductListBuilder.Append("</" + TempAttribute.AttrName + ">");
                        }
                    }

                    poProductListBuilder.Append("\n      </" + poTmpPrdGroup.MasterGroup.GroupName + ">");
                }
            }
        }

        /// <summary>
        /// 
        /// This method will serialize the WonkaXmlMessage into a string.  First, it will do automated 
        /// serialization of all hard-wired data.  Then, it will write the "<ProductList>" portion 
        /// of the WONKA-XML message.
        /// 
        /// </summary>
        /// <param name="poXmlMsg">The data structure representation of the WONKA-XML message</param>
        /// <returns>Contains the WONKA-XML message in string form</returns>
        public string WriteWonkaMsg(WonkaProductMessage poXmlMsg)
        {
            StringBuilder WonkaXmlMessageBuilder = new StringBuilder();
            StringBuilder ProductListBuilder     = new StringBuilder();
            XmlWriter     WonkaXmlWriter         = XmlWriter.Create(WonkaXmlMessageBuilder, DefaultRootWriterSettings);
            XmlSerializer WonkaXmlSerializer     = new XmlSerializer(typeof(WonkaProductMessage));

            WonkaXmlSerializer.Serialize(WonkaXmlWriter, poXmlMsg, DefaultNamespaces);

            if (poXmlMsg.ProdList.Count > 0)
            {
                ProductListBuilder.Append("  <ProductList>");

                foreach (WonkaProduct TempProduct in poXmlMsg.ProdList)
                {
                    WriteWonkaProduct(TempProduct, ProductListBuilder);
                }

                // NOTE: A hack for formatting purposes
                ProductListBuilder.Replace("</Error>",        "      </Error>");
                ProductListBuilder.Replace("</ProductCadre>", "      </ProductCadre>");

                ProductListBuilder.Append("\n  </ProductList>");
                ProductListBuilder.Append("\n</WonkaMessage>");
            }

            if (ProductListBuilder.Length > 0)
                WonkaXmlMessageBuilder.Replace("</WonkaMessage>", ProductListBuilder.ToString());

            return WonkaXmlMessageBuilder.ToString();
        }

        /// <summary>
        /// 
        /// This method will serialize a WonkaProduct into a string buffer.
        /// 
        /// </summary>
        /// <param name="poProduct">The data structure representation of a WONKA product</param>
        /// <param name="poMessageBuffer">The buffer to which the serialized version of the product will be written</param>
        /// <returns>N/A</returns>
        public void WriteWonkaProduct(WonkaProduct poProduct, StringBuilder poMessageBuffer)
        {
            poMessageBuffer.Append("\n    <Product>");

            foreach (int nFieldId in poProduct.ProductCadreIndex.Keys)
            {
                WonkaProductCadre TempProductCadre = poProduct.ProductCadreIndex[nFieldId];

                AppendProductCadre(TempProductCadre, poMessageBuffer);
            }

            foreach (WonkaPrdGroup TempProductGroup in poProduct)
                AppendProductGroup(TempProductGroup, poMessageBuffer);

            poMessageBuffer.Append("\n    </Product>");
        }

        public static string WrapWithCData(string psTarget)
        {
            if (!String.IsNullOrEmpty(psTarget))
                return "<![CDATA[" + psTarget + "]]>";
            else
                return "<![CDATA[]]>";
        } // method

        #endregion

        #region Properties

        private XmlWriterSettings       DefaultRootWriterSettings;
        private XmlWriterSettings       DefaultSubPrdWriterSettings;
        private XmlSerializerNamespaces DefaultNamespaces;

        #endregion
    }
}
