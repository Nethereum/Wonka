using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

using Wonka.MetaData;

namespace Wonka.Product
{
    /// <summary>
    /// 
    /// This class represents a single product, serving as the wrapper for the various Groups 
    /// that are the logical collections of its data.
    /// 
    /// For example, a product would have a Main group with one data row and could have 
    /// an Account group with two data rows.
    /// 
    /// </summary>
    [DataContract(Namespace = "http://sample.wonkasystem.com")]
    [XmlType(TypeName = "Product")]
    public class WonkaProduct : IDisposable, IEnumerable
    {
        public WonkaProduct()
        {
            IsBatch    = IsAppendFasttrackEnabled = IsGroupAppendEnabled = false;
            IsModified = IsGroupReplaceEnabled    = false;

            ProductId      = null;
            OwnerSourceIds = new HashSet<int>();

            TransactionId = 0;

            ProductGroups     = new Dictionary<int, WonkaPrdGroup>();
            ProductCadreIndex = new Dictionary<int, WonkaProductCadre>();
            ResequenceGroups  = new HashSet<int>();
            ProductErrors     = new List<WonkaProductError>();
            MiscData          = new Dictionary<string, string>();
        }

        public WonkaProduct(string pnProductId) : this()
        {
            this.ProductId = pnProductId;
        }

        public WonkaProduct(WonkaProduct poThatProduct) : this()
        {
            Update(poThatProduct);
        }

        #region IDisposable Methods
        public void Dispose()
        {
            ClearData();

            ClearFields();

            this.ProductGroups.Clear();
        }
        #endregion

        #region IEnumerable Methods
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) GetEnumerator();
        }

        public WonkaProductEnumerator GetEnumerator()
        {
            return new WonkaProductEnumerator(this);
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// 
        /// This method will iterate through a list of Products and apply the default value to every instance
        /// of an empty Attribute (i.e., in every DataRow of its Group).
        /// 
        /// <param name="poProducts">The product list that we are iterating through</param>
        /// <param name="poDefaultValues">The default values for Attributes</param>
        /// <returns>None</returns>
        /// </summary>
        public static void ApplyDefaults(List<WonkaProduct> poProducts, Dictionary<int, string> poDefaultValues)
        {
            foreach (WonkaProduct TempProduct in poProducts)
            {
                var iDefaultValEnumerator = poDefaultValues.GetEnumerator();
                while (iDefaultValEnumerator.MoveNext())
                {
                    int    nDefaultAttrId    = iDefaultValEnumerator.Current.Key;
                    string sDefaultAttrValue = iDefaultValEnumerator.Current.Value;

                    WonkaRefAttr TempAttr = WonkaRefEnvironment.GetInstance().GetAttributeByAttrId(nDefaultAttrId);

                    WonkaPrdGroup TargetGroup = TempProduct.GetProductGroup(TempAttr.GroupId);

                    foreach (WonkaPrdGroupDataRow TempDataRow in TargetGroup)
                    {
                        string sTempValue = TempDataRow[TempAttr.AttrId];

                        if (String.IsNullOrEmpty(sTempValue))
                            TempDataRow.SetData(TempAttr.AttrId, sDefaultAttrValue);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// This method will iterate through a list of Products and nullify (i.e., assign an empty value)
        /// every Attribute indicated in the TargetAttributes list.
        /// 
        /// NOTE: This method will nullify Attributes only for the first DataRow of the Group.
        /// 
        /// <param name="poProducts">The product list that we are iterating through</param>
        /// <param name="poTargetAttributes">The Attributes that we intend to nullify</param>
        /// <returns>None</returns>
        /// </summary>
        public static void Nullify(List<WonkaProduct> poProducts, HashSet<int> poTargetAttributes)
        {
            string sNullifyValue = "";

            Dictionary<int, string> ApplyValues = new Dictionary<int, string>();
            foreach (int nTempAttrId in poTargetAttributes)
                ApplyValues[nTempAttrId] = sNullifyValue;

            foreach (WonkaProduct TempProduct in poProducts)
                WonkaProduct.PopulateProduct(TempProduct, ApplyValues);
        }

        /// <summary>
        /// 
        /// This method will assign the Attribute values specifically to the first DataRow
        /// of their respective Group.
        /// 
        /// NOTE: This method will nullify Attributes only for the first DataRow of the Group.
        /// 
        /// <param name="poProduct">The Product that we are assigning these Attribute values</param>
        /// <param name="poAttrValues">The Attribute values that we are going to set inside the provided Product</param>
        /// <returns>None</returns>
        /// </summary>
        public static void PopulateProduct(WonkaProduct poProduct, Dictionary<int, string> poAttrValues)
        {
            var iAttrValueEnum = poAttrValues.GetEnumerator();

            while (iAttrValueEnum.MoveNext())
            {
                int    nAttrId    = iAttrValueEnum.Current.Key;
                string sAttrValue = iAttrValueEnum.Current.Value;

                WonkaRefAttr  TempAttribute = WonkaRefEnvironment.GetInstance().GetAttributeByAttrId(nAttrId);
                WonkaPrdGroup TempPrdGroup  = poProduct.ProductGroups[TempAttribute.GroupId];

                if (TempPrdGroup.GetRowCount() <= 0)
                    TempPrdGroup.AppendRow();

                WonkaPrdGroupDataRow GrpDataRow = TempPrdGroup.GetRow(0);

                GrpDataRow.SetData(nAttrId, sAttrValue);
            }
        }

        /// <summary>
        /// 
        /// This method will assign the Attribute values for a new DataRow
        /// created for their respective Group.
        /// 
        /// <param name="poProduct">The Product that we are assigning these Attribute values</param>
        /// <param name="poAttrValues">The Attribute values that we are going to set inside the provided Product</param>
        /// <returns>None</returns>
        /// </summary>
        public static void PopulateProductGroup(WonkaProduct poProduct, int pnGroupId, Dictionary<int, string> poAttrValues)
        {
            WonkaPrdGroup        TempPrdGroup = poProduct.ProductGroups[pnGroupId];
            WonkaPrdGroupDataRow TempDataRow  = TempPrdGroup.AppendRow();

            var iAttrValueEnum = poAttrValues.GetEnumerator();
            while (iAttrValueEnum.MoveNext())
            {
                int    nAttrId    = iAttrValueEnum.Current.Key;
                string sAttrValue = iAttrValueEnum.Current.Value;

                WonkaRefAttr TempAttribute = WonkaRefEnvironment.GetInstance().GetAttributeByAttrId(nAttrId);

                if (TempAttribute.GroupId != pnGroupId)
                    continue;

                TempDataRow.SetData(nAttrId, sAttrValue);
            }
        }

        #endregion

        #region Member Methods

        /// <summary>
        /// 
        /// .NET demands that the Add() method be implemented for all classes that implement IEnumerable.
        /// However, we are never going to use .NET serialization functionality to populate this class, so 
        /// this function will never actually be called.
        /// 
        /// <returns>None</returns>
        /// </summary>
        public void Add(object data)
        { }

        /// <summary>
        /// 
        /// This method will reset the various properties inside this Product instance, especially by emptying
        /// each Group of its current data.
        /// 
        /// <returns>None</returns>
        /// </summary>
        public void ClearData()
        {
            var enumGroups = this.ProductGroups.GetEnumerator();
            while (enumGroups.MoveNext())
            {
                var TempProductGroup = enumGroups.Current.Value;

                TempProductGroup.DeleteRows();

                TempProductGroup.Modified     = false;
                TempProductGroup.OldDataFound = false;
            }
        }

        public void ClearErrors()
        {
            this.ProductErrors.Clear();
        }


        public void ClearFields()
        {
            this.ProductCadreIndex.Clear();
        }

        /// <summary>
        /// 
        /// This method will seek the primary row (i.e., usually the first row) of a ProductGroup and then 
        /// return the value for the Attribute in that row.  If the ProductGroup is sequenced, we will 
        /// look for the row with the 'group_seq' equal to 1; if it's not, we will just take the first row.
        /// 
        /// <param name="pnGroupId">The ID of the Group that we are interested in</param>
        /// <param name="pnAttrId">The ID of the Attribute that we are interested in</param>
        /// <returns>The value of the Attribute for the Group's primary row</returns>
        /// </summary>
        public string GetPrimaryAttributeData(int pnGroupId, int pnAttrId)
        {
            string sAttrValue = null;

            WonkaPrdGroup TargetGroup = this.GetProductGroup(pnGroupId);
            if (TargetGroup.DataRowVector.Count > 0)
            {
                if (TargetGroup.MasterGroup.IsSequenced)
                {
                    int nSeqAttrId =
                        WonkaRefEnvironment.GetInstance().GetGroupSeqAttrId(pnGroupId);

                    WonkaPrdGroupDataRow TargetDataRow = null;

                    if (TargetGroup.DataRowVector.Any(x => x.ContainsKey(nSeqAttrId) && x[nSeqAttrId] == "1"))
                        TargetDataRow = TargetGroup.DataRowVector.Where(x => x.ContainsKey(nSeqAttrId) && x[nSeqAttrId] == "1").FirstOrDefault();
                    else
                        TargetDataRow = TargetGroup[0];

                    if (TargetDataRow.ContainsKey(pnAttrId))
                        sAttrValue = TargetDataRow[pnAttrId];
                }
                else
                {
                    WonkaPrdGroupDataRow TargetDataRow = TargetGroup[0];

                    if (TargetDataRow.ContainsKey(pnAttrId))
                        sAttrValue = TargetDataRow[pnAttrId];
                }
            }

            return sAttrValue;
        }

        /// <summary>
        /// 
        /// This method will return the sought ProductField of this Product, if one is already present.
        /// If it is not present, it will create one by default and return it after inserting into 
        /// our collection.
        /// 
        /// <param name="poField">The Field representing the ProductField that we want to retrieve</param>
        /// <returns>The ProductField that we want to retrieve from this Product</returns>
        /// </summary>
        public WonkaProductCadre GetProductField(WonkaRefCadre poField)
        {
            WonkaProductCadre SoughtField = null;

            if (ProductCadreIndex.Keys.Contains(poField.CadreId))
                SoughtField = ProductCadreIndex[poField.CadreId];
            else if (WonkaRefEnvironment.GetInstance().DoesFieldExist(poField.CadreId))
            {
                ProductCadreIndex[poField.CadreId] = new WonkaProductCadre();

                SoughtField = ProductCadreIndex[poField.CadreId];

                SoughtField.ProductId = this.ProductId;
                SoughtField.CadreId   = poField.CadreId;
                SoughtField.LockCd    = "N";

                SoughtField.LastTouchedSourceId = 
                    (this.OwnerSourceIds != null) && (this.OwnerSourceIds.Count > 0) ? this.OwnerSourceIds.ElementAt(0) : 0;
            }
            else
                throw new Exception("ERROR!  WonkaProduct::getProductField(const WonkaRefField&) : " + 
                                    "Requested field does not exist: (" + poField.CadreName + ").");

            return SoughtField;
        }

        /// <summary>
        /// 
        /// This method will provide the ProductGroup inside this Product, if one has already 
        /// been instantiated with values.  If not, it will create a new empty one and place it
        /// inside the collection.
        /// 
        /// <param name="pnGroupId">The ID of the Group that we are interested in retrieving</param>
        /// <returns>The ProductGroup that we want to retrieve from this Product</returns>
        /// </summary>
        public WonkaPrdGroup GetProductGroup(int pnGroupId)
        {
            WonkaPrdGroup SoughtGroup = null;

            if (HasProductGroup(pnGroupId))
                SoughtGroup = this.ProductGroups[pnGroupId];
            else
            {
                if (WonkaRefEnvironment.GetInstance().DoesGroupExist(pnGroupId))
                {
                    this.ProductGroups[pnGroupId] = 
                        new WonkaPrdGroup( WonkaRefEnvironment.GetInstance().GetGroupByGroupId(pnGroupId) );

                    return this.ProductGroups[pnGroupId];
                }
                else
                    throw new Exception("ERROR!  Requested Group ID (" + pnGroupId + ") does not exist.");
            }

            return SoughtGroup;
        }

        /// <summary>
        /// 
        /// This method will detect whether or not this Product already has an instance of the identified
        /// ProductGroup.
        /// 
        /// <param name="pnGroupId">The ID of the Group that we are interested in detecting</param>
        /// <returns>Indicator of whether or not an instance of that Group has been created within this Product</returns>
        /// </summary>
        public bool HasProductGroup(int pnGroupId)
        {
            return this.ProductGroups.Keys.Contains(pnGroupId);
        }

        /// <summary>
        /// 
        /// This method will detect whether or not this Product already has an instance of the identified
        /// ProductField
        /// 
        /// <param name="poField">The Field indicating the ProductField that we are interested in</param>
        /// <returns>Indicator of whether or not an instance of that ProductField has been created within this Product</returns>
        /// </summary>
        public bool HasProductField(WonkaRefCadre poField)
        {
            return this.ProductCadreIndex.Keys.Contains(poField.CadreId);
        }

        public void PrintDisplay()
        {
            // NOTE: Might be implemented at some point, but as we know with these kinds of comments --- not likely
        }

        /// <summary>
        /// 
        /// This method will update all contents of this Product with those of the provided one.
        /// 
        /// <param name="poThatProduct">The Product from which we will copy our sought data</param>
        /// <param name="pbMergeFlag">Indicator of whether the provided data should completely overlay or merge with the contents of this Product</param>
        /// <returns>None</returns>
        /// </summary>
        public void Update(WonkaProduct ThatProduct, bool pbMergeFlag = true)
        {
            if (pbMergeFlag)
            {
                foreach (WonkaPrdGroup ThatPrdGroup in ThatProduct)
                    this.Update(ThatPrdGroup, pbMergeFlag);

                var iThatProductField = ThatProduct.ProductCadreIndex.GetEnumerator();
                while (iThatProductField.MoveNext())
                {
                    WonkaProductCadre ThatProductField = iThatProductField.Current.Value;

                    if (this.ProductCadreIndex.Keys.Contains(ThatProductField.CadreId))
                        this.ProductCadreIndex[ThatProductField.CadreId] = ThatProductField;
                }
            }
            else
            {
                ClearData();

                foreach (WonkaPrdGroup ThatPrdGroup in ThatProduct)
                    this.Update(ThatPrdGroup, pbMergeFlag);

                ClearFields();

                this.ProductCadreIndex = ThatProduct.ProductCadreIndex;
            }

            ClearErrors();
            this.ProductErrors = ThatProduct.ProductErrors;

            this.ResequenceGroups = ThatProduct.ResequenceGroups;

            this.IsAppendFasttrackEnabled = ThatProduct.IsAppendFasttrackEnabled;
            this.IsBatch                  = ThatProduct.IsBatch;
            this.IsGroupAppendEnabled     = ThatProduct.IsGroupAppendEnabled;
            this.IsGroupReplaceEnabled    = ThatProduct.IsGroupReplaceEnabled;
            this.ProductId                = ThatProduct.ProductId;

            this.OwnerSourceIds = ThatProduct.OwnerSourceIds;
            this.TransactionId  = ThatProduct.TransactionId;
        }

        /// <summary>
        /// 
        /// This method will update a particular ProductGroup of this Product with the one provided.
        /// 
        /// <param name="poThatProductGroup">The ProductGroup from which we will copy our sought data</param>
        /// <param name="pbMergeFlag">Indicator of whether the provided data should completely overlay or merge with the contents of this ProductGroup</param>
        /// <returns>None</returns>
        /// </summary>
        public void Update(WonkaPrdGroup poThatProductGroup, bool pbMergeFlag)
        {
            WonkaPrdGroup ThisPrdGroup = this.GetProductGroup(poThatProductGroup.GroupId);

            if (pbMergeFlag)
                ThisPrdGroup.Merge(poThatProductGroup);
            else
                ThisPrdGroup = poThatProductGroup;
        }

        /// <summary>
        /// 
        /// This method will iterate through all of the data in the contained groups and detect whether
        /// any of them are not valid according to the designated type for that Attribute.
        /// 
        /// <param name="poErrors">The list to which we will add any errors concerning the validation of types</param>
        /// <returns>Indicates whether or not there any errors with validating types</returns>
        /// </summary>
        public bool ValidateTypes(List<WonkaProductError> poErrors)
        {
            WonkaRefEnvironment RefEnv = WonkaRefEnvironment.GetInstance();

            bool bResult = true;

            foreach (int nGrpId in this.ProductGroups.Keys)
            {
                WonkaPrdGroup TempGroup = ProductGroups[nGrpId];

                foreach (WonkaPrdGroupDataRow TempDataRow in TempGroup)
                {
                    foreach (int nAttrId in TempDataRow.Keys)
                    {
                        string sAttrValue = TempDataRow[nAttrId];

                        if (!String.IsNullOrEmpty(sAttrValue))
                        {
                            WonkaRefAttr TempAttr = RefEnv.GetAttributeByAttrId(nAttrId);

                            if (TempAttr.IsDecimal)
                            {
                                try
                                {
                                    Decimal dValue = Convert.ToDecimal(sAttrValue);
                                }
                                catch (Exception ex)
                                {
                                    poErrors.Add(new WonkaProductError()
                                                     { AttrName = TempAttr.AttrName,
                                                       ErrorMessage = "ERROR!  Value(" + sAttrValue + ") is not a valid decimal!"
                                                     });
                                }
                            }
                            else if (TempAttr.IsNumeric)
                            {
                                try
                                {
                                    long nValue = Convert.ToInt64(sAttrValue);
                                }
                                catch (Exception ex)
                                {
                                    poErrors.Add(new WonkaProductError()
                                                    {
                                                        AttrName = TempAttr.AttrName,
                                                        ErrorMessage = "ERROR!  Value(" + sAttrValue + ") is not a valid number!"
                                                    });
                                }
                            }
                            else if (TempAttr.IsDate)
                            {
                                try
                                {
                                    DateTime dtValue = DateTime.Parse(sAttrValue);
                                }
                                catch (Exception ex)
                                {
                                    poErrors.Add(new WonkaProductError()
                                    {
                                        AttrName = TempAttr.AttrName,
                                        ErrorMessage = "ERROR!  Value(" + sAttrValue + ") is not a valid date!"
                                    });
                                }
                            }
                        }
                    }
                }
            }

            return bResult;
        }

        #endregion

        #region Properties

        public bool IsBatch { get; set; }

        public bool IsAppendFasttrackEnabled { get; set; }

        public bool IsGroupAppendEnabled { get; set; }

        public bool IsGroupReplaceEnabled { get; set; }

        public bool IsModified { get; set; }

        public string ProductId { get; set; }

        public HashSet<int> OwnerSourceIds { get; set; }

        public int TransactionId { get; set; }

        public Dictionary<int, WonkaPrdGroup> ProductGroups { get; set; }

        public Dictionary<int, WonkaProductCadre> ProductCadreIndex { get; set; }

        public HashSet<int> ResequenceGroups { get; set; }

        public List<WonkaProductError> ProductErrors { get; set; }

        public Dictionary<string, string> MiscData { get; set; }

        #endregion
    }

    public class WonkaProductEnumerator : IEnumerator
    {
        private WonkaProduct moProduct = null;

        private Dictionary<int, WonkaPrdGroup>.Enumerator moGrpEnum;

        public WonkaProductEnumerator(WonkaProduct poProduct)
        {
            if (poProduct == null)
                throw new Exception("ERROR!  Product provided is null.");

            moProduct = poProduct;
            moGrpEnum = moProduct.ProductGroups.GetEnumerator();            
        }

        public bool MoveNext()
        {
            return moGrpEnum.MoveNext();
        }

        public void Reset()
        {
            moGrpEnum = moProduct.ProductGroups.GetEnumerator();
        }

        public object Current
        {
            get
            {
                return moGrpEnum.Current.Value;
            }
        }
    }

}

