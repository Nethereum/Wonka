using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using Wonka.MetaData;

namespace Wonka.Product
{
    /// <summary>
    /// 
    /// This class represents a single group of the product data, containing 1-to-many 
    /// multiple instances of WonkaPrdGroupDataRow.
    /// 
    /// For example, a product would have a Main group (record identifier, name, etc.) with one data row and could have 
    /// an Account group with two data rows (checking account, savings account, etc.).
    /// 
    /// </summary>
    public class WonkaPrdGroup : IDisposable, IEnumerable
    {
        public WonkaPrdGroup(WonkaRefGroup poRefGroup)
        {
            Modified = OldDataFound = ProperlySequenced = false;

            GroupId = poRefGroup.GroupId;

            DataRowVector = new List<WonkaPrdGroupDataRow>();
            MasterDataRow = new WonkaPrdGroupDataRow(poRefGroup);
            MasterGroup   = poRefGroup;
        }

        public WonkaPrdGroup(WonkaPrdGroup poOriginalGroup) : this(poOriginalGroup.MasterGroup)
        {
            this.DataRowVector.AddRange(poOriginalGroup.DataRowVector);

            this.Modified          = poOriginalGroup.Modified;
            this.OldDataFound      = poOriginalGroup.OldDataFound;
            this.ProperlySequenced = poOriginalGroup.ProperlySequenced;
        }

        #region IDisposable Methods
        public void Dispose()
        {
            DeleteRows();
        }
        #endregion

        #region IEnumerable Methods
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) GetEnumerator();
        }

        public WonkaPrdGroupEnumerator GetEnumerator()
        {
            return new WonkaPrdGroupEnumerator(this);
        }
        #endregion

        #region Operators

        public WonkaPrdGroupDataRow this[int nIndex]
        {
            get
            {
                return GetRow(nIndex);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// .NET demands that the Add() method be implemented for all classes that implement IEnumerable.
        /// However,  we are never going to use .NET serialization functionality to populate this class, so 
        /// this function will never actually be called.
        /// 
        /// <returns>None</returns>
        /// </summary>
        public void Add(object data)
        { }

        /// <summary>
        /// 
        /// This method will append an empty DataRow to the list and then return it for usage to the caller
        /// (such as filling it with values).
        /// 
        /// <returns>The new empty DataRow that has been inserted into the internal list</returns>
        /// </summary>
        public WonkaPrdGroupDataRow AppendRow()
        {
            WonkaPrdGroupDataRow NewDataRow = new WonkaPrdGroupDataRow(MasterGroup);
            DataRowVector.Add(NewDataRow);
            return NewDataRow;
        }

        /// <summary>
        /// 
        /// This method will append the provided DataRow to the internal list.
        /// 
        /// <param name="ProvidedDataRow">The DataRow that we will insert into our internal list (after it has been validated)</param>
        /// <returns>The provided DataRow that has been inserted into the internal list</returns>
        /// </summary>
        public WonkaPrdGroupDataRow AppendRow(WonkaPrdGroupDataRow ProvidedDataRow)
        {
            ValidateDataRow(ProvidedDataRow);
            DataRowVector.Add(ProvidedDataRow);
            return ProvidedDataRow;
        }

        /// <summary>
        /// 
        /// This method will iterate through the DataRows inside this Group in order to form
        /// the appropriate repeating composite bodies as its correct XML amalgamation.
        /// 
        /// For example, in the case of an Account group with two rows, it could then 
        /// generate XML like the following:
        /// 
        /// <Account>
        ///   <AccountType><![CDATA[Checking]]></AccountType>
        ///   <Currency><![CDATA[Ethereum]]></Currency>
        ///   <Amount><![CDATA[10.5]]></Amount>
        /// </Account>
        /// <Account>
        ///   <AccountType><![CDATA[Savings]]></AccountType>
        ///   <Currency><![CDATA[USD]]></Currency>
        ///   <Amount><![CDATA[20.5]]></Amount>
        /// </Account>
        /// 
        /// </summary>
        /// <returns>The serialized Wonka XML that represents this Product Group</returns>
        public string AssembleGroupXml()
        {
            WonkaRefEnvironment RefEnv = WonkaRefEnvironment.GetInstance();

            StringBuilder GroupBuilder = new StringBuilder();

            foreach (WonkaPrdGroupDataRow TempDataRow in this)
            {
                GroupBuilder.Append("<" + this.MasterGroup.GroupName + ">");

                foreach (int nTempAttrId in TempDataRow.Keys)
                {
                    WonkaRefAttr TempAttribute = RefEnv.GetAttributeByAttrId(nTempAttrId);

                    string sTempAttrValue = TempDataRow[nTempAttrId];

                    if (!String.IsNullOrEmpty(sTempAttrValue))
                        GroupBuilder.Append("\t<" + TempAttribute.AttrName + ">" +
                                            WrapWithCDATA(sTempAttrValue) +
                                            "</" + TempAttribute.AttrName + ">");
                }

                GroupBuilder.Append("</" + this.MasterGroup.GroupName + ">");
            }

            return GroupBuilder.ToString();
        }

        public void DeleteRows()
        {
            foreach (WonkaPrdGroupDataRow TempDataRow in DataRowVector)
                TempDataRow.ClearData();

            DataRowVector.Clear();
        }

        public void DeleteRow(int pnRowIndex)
        {
            ValidateRowIndex(pnRowIndex);
            DataRowVector.RemoveAt(pnRowIndex);
        }

        /// <summary>
        /// 
        /// This method will compare two values of the same Attribute type.  It will then use the
        /// Attribute information to help determine if the two values are the same.  For example,
        /// if the Attribute metadata specifies that they're decimal values, then they will be
        /// converted to Decimal types and then compared that way.
        /// 
        /// <param name="poAttr">The Attribute type of the two values</param>
        /// <param name="psThisValue">The left-hand value being compared</param>
        /// <param name="psThatValue">The right-hand value being compared</param>
        /// <returns>The bool indicating whether or not the two values are actually the same</returns>
        /// 
        /// </summary>
        public bool Compare(WonkaRefAttr poAttr, string psThisValue, string psThatValue)
        {
            bool bResult = true;

            if (!String.IsNullOrEmpty(psThisValue) && String.IsNullOrEmpty(psThatValue))
            {
                bResult = false;
            }
            else if (poAttr.IsNumeric)
            {
                if (poAttr.IsDecimal)
                {
                    try
                    {
                        // NOTE: Do we need to do any rounding here?
                        decimal fThisValue = Convert.ToDecimal(psThisValue);
                        decimal fThatValue = Convert.ToDecimal(psThatValue);

                        if (fThisValue != fThatValue)
                        {
                            bResult = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        bResult = false;
                    }
                }
                else
                {
                    try
                    {
                        // NOTE: Do we need to do any rounding here?
                        long nThisValue = Convert.ToInt64(psThisValue);
                        long nThatValue = Convert.ToInt64(psThatValue);

                        if (nThisValue != nThatValue)
                        {
                            bResult = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        bResult = false;
                    }
                }
            }
            else
            {
                bResult = false;
            }

            return bResult;
        }

        /// <summary>
        /// 
        /// This method will compare two Groups to see if they are equal. It will compare
        /// them by finding a corresponding DataRow within ThisGroup and ThatGroup, based
        /// on the key.
        /// 
        /// NOTE: This Group (i.e., the left-hand data row) usually represents incoming/new data
        /// 
        /// NOTE: Currently, it doesn't compare them exactly.  It just ensures that
        /// the contents of 'r1' are the same in 'r2', implying that 'r2' can be a
        /// superset of 'r1'.
        /// 
        /// <param name="ThatGroup">The right-hand data row (usually representing old data from persistence/storage)</param>
        /// <returns>Bool that indicates whether or not the two Groups are equal</returns>
        public bool Equals(WonkaPrdGroup poThatGroup)
        {
            foreach (WonkaPrdGroupDataRow ThisRow in this)
            {
                int nThatRowIndex = poThatGroup.GetRowIndex(ThisRow.GetKey());

                if (nThatRowIndex != -1)
                {
                    if (ThisRow != poThatGroup.GetRow(nThatRowIndex))
                        return false;
                }
                else
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// This method will compare two Groups to see if they are equal, but it will
        /// only compare those Attributes mentioned in the target Field.
        /// 
        /// <param name="poThatGroup">The group being compared against (usually representing old data from persistence/storage)</param>
        /// <param name="poTargetField">The Field that has the Attribute list of interest</param>
        /// <returns>Bool that indicates whether or not the two Groups are equal</returns>
        /// </summary>
        public bool Equals(WonkaPrdGroup poThatGroup, WonkaRefCadre poTargetField)
        {
            Dictionary<int, string> ThisGroupAttrValues = new Dictionary<int, string>();
            Dictionary<int, string> ThatGroupAttrValues = new Dictionary<int, string>();

            return Equals(poThatGroup, poTargetField, ThisGroupAttrValues, ThatGroupAttrValues);
        }

        /// <summary>
        /// 
        /// This method will compare two Groups to see if they are equal, but it will
        /// only compare those Attributes mentioned in the target Field.
        /// 
        /// NOTE: The auditing containers 'poThisAttrValues' and 'poThatAttrValues' will only 
        /// work correctly with a group that only has one row.
        /// 
        /// <param name="poThatGroup">The group being compared against (usually representing old data from the persistence/storage)</param>
        /// <param name="poTargetField">The Field that has the Attribute list of interest</param>
        /// <param name="poThisAttrValues">Storage for the values different from "this" group</param>
        /// <param name="poThatAttrValues">Storage for the values different from "that" group</param>
        /// <returns>Bool that indicates whether or not the two Groups are equal</returns>
        /// </summary>
        public bool Equals(WonkaPrdGroup poThatGroup, 
                      WonkaRefCadre      poTargetField,
                  Dictionary<int,string> poThisAttrValues,
                  Dictionary<int,string> poThatAttrValues)
        {
            HashSet<int> IgnoreAttrIds = new HashSet<int>();

            return Equals(poThatGroup, poTargetField, IgnoreAttrIds, poThisAttrValues, poThatAttrValues);
        }

        /// <summary>
        /// 
        /// This method will compare two Groups to see if they are equal, but it will
        /// only compare those Attributes mentioned in the target Field.
        /// 
        /// NOTE: The auditing containers 'poThisAttrValues' and 'poThatAttrValues' will only 
        /// work correctly with a group that only has one row.
        /// 
        /// <param name="poThatGroup">The group being compared against (usually representing old data from the DB)</param>
        /// <param name="poTargetField">The Field that has the Attribute list of interest</param>
        /// <param name="poIgnoreAttrIds">The list of Attributes that should be ignored when comparisons are done</param>
        /// <param name="poThisAttrValues">Storage for the values different from "this" group</param>
        /// <param name="poThatAttrValues">Storage for the values different from "that" group</param>
        /// <returns>Bool that indicates whether or not the two Groups are equal</returns>
        /// </summary>
        public bool Equals(WonkaPrdGroup poThatGroup,
                           WonkaRefCadre poTargetField,
                            HashSet<int> poIgnoreAttrIds,
                 Dictionary<int, string> poNewAttrValues,
                 Dictionary<int, string> poOldAttrValues)
        {
            bool bResult = true;

            foreach (WonkaPrdGroupDataRow ThisRow in this.DataRowVector)
            {
                int nThatRowIndex = poThatGroup.GetRowIndex( ThisRow.GetKey() );

                if (nThatRowIndex != -1)
                {
                    WonkaPrdGroupDataRow ThatRow = poThatGroup[nThatRowIndex];

                    HashSet<int> FieldAttrIds =
                        WonkaRefEnvironment.GetInstance().GetAttrIdsByFieldId(poTargetField.CadreId);

                    foreach (int nAttrId in FieldAttrIds)
                    {
                        WonkaRefAttr TempAttr =WonkaRefEnvironment.GetInstance().GetAttributeByAttrId(nAttrId);

                        if (poIgnoreAttrIds.Contains(nAttrId))
                            continue;

                        if (poTargetField.MergeNullAttrFlag || !String.IsNullOrEmpty(ThisRow[nAttrId]))
                        {
                            string sThisValue = ThisRow[nAttrId];
                            string sThatValue = ThatRow[nAttrId];

                            if (sThisValue != sThatValue)
                            {
                                // NOTE: Need to record these values, maybe for auditing
                                if (TempAttr.IsAudited)
                                {
                                    poNewAttrValues[TempAttr.AttrId] = sThisValue;
                                    poOldAttrValues[TempAttr.AttrId] = sThatValue;
                                }

                                bResult = Compare(TempAttr, sThisValue, sThatValue);
                            }
                        }
                    }
                }
            }

            return bResult;
        }

        public WonkaPrdGroupDataRow GetRow(int pnIndex)
        {
            return DataRowVector[pnIndex];
        }

        /// <summary>
        /// 
        /// This method will find the DataRow within this Group that matches the provided Key (if it exists).
        /// 
        /// <param name="poSoughtRowKey">The Key of the Row that we are interested in finding</param>
        /// <returns>The DataRow that matches the provided Key</returns>
        /// </summary>
        internal WonkaPrdGroupDataRow GetRow(WonkaPrdGroupDataRowKey poSoughtRowKey)
        {
            int nRowIndex = GetRowIndex(poSoughtRowKey);

            if (nRowIndex != -1)
                return GetRow(nRowIndex);
            else
                throw new Exception("ERROR!  WonkaPrdGroup::GetRow(WonkaPrdGroupDataRowKey) -> Key not found.");
        }

        public int GetRowCount()
        {
            return DataRowVector.Count;
        }

        /// <summary>
        /// 
        /// This method will find the index of the DataRow within this Group that matches the provided Key (if it exists).
        /// 
        /// <param name="poSoughtRowKey">The Key of the Row that we are interested in finding</param>
        /// <returns>Index of the DataRow that matches the provided Key</returns>
        /// </summary>
        internal int GetRowIndex(WonkaPrdGroupDataRowKey poSoughtRowKey)
        {
            int nTmpRowIndex = 0;
            int nRowIndex    = -1;

            for (nTmpRowIndex = 0; nTmpRowIndex < this.DataRowVector.Count; ++nTmpRowIndex)
            {
                if (this.DataRowVector[nTmpRowIndex].GetKey() == poSoughtRowKey)
                    nRowIndex = nTmpRowIndex;
            }

            return nRowIndex;
        }

        /// <summary>
        /// 
        /// This method will detect if all DataRows contained in this Group are considered 'null' (i.e., empty of values).
        /// 
        /// <returns>Bool that indicates if there are any rows present inside the Group that are not null</returns>
        /// </summary>
        public bool IsNull()
        {
            if (GetRowCount() == 0)
                return true;

            foreach (WonkaPrdGroupDataRow TempDataRow in DataRowVector)
            {
                if (!TempDataRow.IsNull())
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// This method will detect if all DataRows contained in this Group are considered 'null' (i.e., empty of values),
        /// but only for those Attributes mentioned within the target Field.
        /// 
        /// <param name="poField">The target Field with the list of Attributes on which we are focused</param>
        /// <param name="pbIgnoreDeletedRows">Indicates whether or not Marked-as-Delete rows should be ignored</param>
        /// <returns>Bool that indicates if there are any rows present inside the Group that are not null</returns>
        /// 
        /// </summary>
        public bool IsNull(WonkaRefCadre poField, bool pbIgnoreDeletedRows = true)
        {
            bool bResult = true;

            if (GetRowCount() == 0)
                return bResult;

            int          nGrpSeqAttrId = -1;
            HashSet<int> FieldAttrIds  = WonkaRefEnvironment.GetInstance().GetAttrIdsByFieldId(poField.CadreId);

            if (this.MasterGroup.IsSequenced)
                nGrpSeqAttrId = WonkaRefEnvironment.GetInstance().GetGroupSeqAttrId(MasterGroup.GroupId);

            foreach (WonkaPrdGroupDataRow TempDataRow in DataRowVector)
            {
                if (pbIgnoreDeletedRows)
                {
                    if ((nGrpSeqAttrId >= 0) && (TempDataRow[nGrpSeqAttrId] == "0"))
                        continue;
                }

                foreach (int nAttrId in FieldAttrIds)
                {
                    string sTempValue = TempDataRow[nAttrId];
                    if (!String.IsNullOrEmpty(sTempValue))
                        return false;
                }
            }

            return bResult;
        }

        /// <summary>
        /// 
        /// This method will merge the contents of this Group with the one provided.
        /// 
        /// NOTE: A true merge happens only between two instances of a group with one row.  The functionality 
        /// to merge other groups has not yet been truly implemented.
        /// 
        /// <param name="ThatProductGroup">The Group that should merge with this one</param>
        /// <returns>None</returns>
        /// </summary>
        public void Merge(WonkaPrdGroup ThatProductGroup)
        {
            if (this.MasterGroup.GroupId == ThatProductGroup.MasterGroup.GroupId)
            {
                if ((this.GetRowCount() == 1) && (ThatProductGroup.GetRowCount() == 1))
                {
                    if (ThatProductGroup.GetRowCount() > 0)
                    {
                        if (this.GetRowCount() <= 0)
                            AppendRow();

                        WonkaPrdGroupDataRow ThisRow = this.DataRowVector[0];
                        WonkaPrdGroupDataRow ThatRow = ThatProductGroup.DataRowVector[0];

                        ThisRow.Merge(ThatRow);
                    }
                }
                else if (ThatProductGroup.GetRowCount() > 0)
                    this.SetData(ThatProductGroup);
            }
            else
                throw new Exception("ERROR!  Attempting to merge two Groups of different sizes!");
        }

        public void PrintDisplay()
        {
            // NOTE: Will be implemented
        }

        /// <summary>
        /// 
        /// This method will replace the contents of its current DataRows with the contents of the provided Group.
        /// 
        /// <param name="poOriginalCopy">The Group from which we are going to copy the DataRows</param>
        /// <returns>None</returns>
        /// </summary>
        public void SetData(WonkaPrdGroup poOriginalCopy)
        {
            this.DeleteRows();

            foreach (WonkaPrdGroupDataRow ThatDataRow in poOriginalCopy.DataRowVector)
                AppendRow(new WonkaPrdGroupDataRow(ThatDataRow));
        }

        /// <summary>
        /// 
        /// This method will update the contents of a row (at index 'pnRowIndex') with the values from the supplied DataRow
        /// (via matching on the key), but only for the Attributes of a given Field.  In addition, if any Attribute inside 
        /// the updated Field has an associated AttrModDt, we will set that Attribute with the timestamp of CurrTimeStamp.
        /// 
        /// NOTE: This code assumes that only 1 AttrModDt will be updated per call of updateField(...) 
        /// 
        /// <param name="poThatGroup">The Group that we are using to update this one</param>
        /// <param name="poTargetField">The Field that possesses the Attribute list of interest</param>
        /// <param name="psCurrTimeStamp">The current Timestamp that we will use to set any associated AttrModDdt</param>
        /// <returns>The AttrID of the AttrModDt which has been updated with the CurrTimeStamp</returns>
        /// </summary>
        public int UpdateField(WonkaPrdGroup poThatGroup, WonkaRefCadre poTargetField, string psCurrTimeStamp = null)
        {
            int nUpdatedModDtAttrId = 0;

            HashSet<int> FieldAttrIds = WonkaRefEnvironment.GetInstance().GetAttrIdsByFieldId(poTargetField.CadreId);

            string sTimeStamp = (!String.IsNullOrEmpty(psCurrTimeStamp)) ? psCurrTimeStamp : DateTime.Now.ToString("yyyyMMddHHmmss");

            foreach (WonkaPrdGroupDataRow ThatRow in poThatGroup)
            {
                WonkaPrdGroupDataRow ThisRow = 
                    (this.GetRowIndex(ThatRow.GetKey()) >= 0) ? this.GetRow(ThatRow.GetKey()) : AppendRow();

                foreach (int nTempAttrId in FieldAttrIds)
                {
                    string sThatValue = ThatRow[nTempAttrId];

                    if (!String.IsNullOrEmpty(sThatValue))
                    {
                        WonkaRefAttr TempAttr = WonkaRefEnvironment.GetInstance().GetAttributeByAttrId(nTempAttrId);

                        ThisRow[nTempAttrId] = sThatValue;

                        if (TempAttr.AttrModDtFlag)
                        {
                            try
                            {
                                WonkaRefAttr TempAttrModDt =
                                    WonkaRefEnvironment.GetInstance().GetAttributeByAttrName(TempAttr.AttrModDt);

                                string sThatAttrModDtValue = ThatRow[TempAttrModDt.AttrId];

                                // NOTE: We will only use the CurrentTimestamp if there isn't already a timestamp value
                                //       in the provided DataRow of ThatGroup
                                if (String.IsNullOrEmpty(sThatAttrModDtValue))
                                {
                                    ThisRow[TempAttrModDt.AttrId] = sTimeStamp;

                                    if (nUpdatedModDtAttrId == 0)
                                        nUpdatedModDtAttrId = TempAttrModDt.AttrId;
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("ERROR!  Cannot set the ATTR_MOD_DT sibling (" + TempAttr.AttrModDt +
                                                    ") for ATTRIBUTE (" + TempAttr.AttrName + ").");
                            }
                        }
                    }
                }
            }

            return nUpdatedModDtAttrId;
        }

        /// <summary>
        /// 
        /// This method will update the contents of a row (at index 'pnRowIndex') with the values from the supplied DataRow.
        /// 
        /// <param name="poNewDataRow">The DataRow that we are using to set the DataRow found at index 'pnRowIndex'</param>
        /// <returns>None</returns>
        /// </summary>
        public void UpdateRow(WonkaPrdGroupDataRow poNewDataRow, int pnRowIndex)
        {
            ValidateRowIndex(pnRowIndex);
            ValidateDataRow(poNewDataRow);

            DataRowVector[pnRowIndex] = poNewDataRow;
        }

        /// <summary>
        /// 
        /// This method provides a quick validation of the DataRow and indicates whether or not it belongs within this Group.
        /// 
        /// <param name="poDataRow">The DataRow that we are validating for membership within this Group</param>
        /// <returns>Bool that indicates if the DataRow is acceptable</returns>
        /// </summary>
        public bool ValidateDataRow(WonkaPrdGroupDataRow poDataRow)
        {
            if ( (MasterDataRow.MasterGroup.GroupId != poDataRow.MasterGroup.GroupId) || (poDataRow.MasterGroup.GroupId <= 0) )
                throw new Exception("ERROR!  Invalid Group ID (" + poDataRow.MasterGroup.GroupId + ").");

            return true;
        }

        /// <summary>
        /// 
        /// This method provides a quick validation of the index value for a specific DataRow within this Group.
        /// 
        /// <param name="pnRowIndex">The index for a supposed DataRow within this Group</param>
        /// <returns>Bool that indicates if the suggested index correlates to an DataRow within this Group</returns>
        /// </summary>
        public bool ValidateRowIndex(int pnRowIndex)
        {
            if ((pnRowIndex < 0) || (pnRowIndex >= DataRowVector.Count))
                throw new Exception("ERROR! Invalid row index (" + pnRowIndex + ").");

            return true;
        }

        static public string WrapWithCDATA(string psValue)
        {
            return "<![CDATA[" + psValue + "]]>";
        }

        #endregion

        #region Properties

        public int GroupId { get; set; }

        public bool Modified { get; set; }

        public bool OldDataFound { get; set; }

        public bool ProperlySequenced { get; set; }

        public List<WonkaPrdGroupDataRow> DataRowVector { get; set; }

        public WonkaPrdGroupDataRow MasterDataRow { get; set; }

        public WonkaRefGroup MasterGroup { get; set; }

        #endregion
    }

    public class WonkaPrdGroupEnumerator : IEnumerator
    {
        private int                  mnIndex       = -1;
        private WonkaPrdGroup        moGroup       = null;
        private WonkaPrdGroupDataRow moCurrDataRow = null;

        public WonkaPrdGroupEnumerator(WonkaPrdGroup poGroup)
        {
            if (poGroup == null)
                throw new Exception("ERROR!  Group provided is null.");

            moGroup = poGroup;
        }

        public bool MoveNext()
        {
            bool bResult = true;

            ++mnIndex;

            if (mnIndex < moGroup.DataRowVector.Count)
                moCurrDataRow = moGroup.DataRowVector[mnIndex];
            else
            {
                moCurrDataRow = null;
                bResult = false;
            }

            return bResult;
        }

        public void Reset()
        {
            mnIndex = -1;
            return;
        }

        public object Current
        {
            get
            {
                return moCurrDataRow;
            }
        }
    }
}
