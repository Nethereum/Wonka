using System;
using System.Collections.Generic;

using WonkaRef;

namespace WonkaPrd
{
    /// <summary>
    /// 
    /// This class represents a single instance of the product data for a specific group.
    /// 
    /// For example, a product would have a Main group with one data row (with name, price, etc.) and could have 
    /// a Contributor group with two data rows (i.e., one row being the inventor and one row being the manufacturer).
    /// 
    /// </summary>
    public class WonkaPrdGroupDataRow : Dictionary<int, string>, IDisposable
    {
        public WonkaPrdGroupDataRow(WonkaRefGroup poGroup)
        {
            GroupId     = poGroup.GroupId;
            GroupName   = poGroup.GroupName;
            MasterGroup = poGroup;

            AttrIds = WonkaRefEnvironment.GetInstance().IdXref.GroupIdToAttrIds[poGroup.GroupId];

            if (this.MasterGroup.IsSequenced)
            {
                GroupSeqAttrId = WonkaRefEnvironment.GetInstance().GetGroupSeqAttrId(MasterGroup.GroupId);

				// NOTE: Should KeyAttrIds be required for a group?
				//if (!WonkaRefEnvironment.GetInstance().IdXref.GroupIdToKeyAttrIds.ContainsKey(poGroup.GroupId))
				//	throw new Exception("ERROR!  No keys found for the group with ID(" + poGroup.GroupId + ")");

				if (WonkaRefEnvironment.GetInstance().IdXref.GroupIdToKeyAttrIds.ContainsKey(poGroup.GroupId))
					KeyAttrIds = WonkaRefEnvironment.GetInstance().IdXref.GroupIdToKeyAttrIds[poGroup.GroupId];
            }
            else
                GroupSeqAttrId = 0;

            Key = new WonkaPrdGroupDataRowKey(this);
        }

        public WonkaPrdGroupDataRow(WonkaRefGroup poGroup, Dictionary<int, string> poDataRow) : this(poGroup)
        {
            if ((poDataRow != null) && (poDataRow.Count > 0))
            {
                foreach (int nTmpAttrId in poDataRow.Keys)
                {
                    string sTmpValue = poDataRow[nTmpAttrId];

                    WonkaRefAttr TmpAttribute =
                        WonkaRefEnvironment.GetInstance().GetAttributeByAttrId(nTmpAttrId);

                    this[TmpAttribute.AttrId] = sTmpValue;
                }
            }
        }

        public WonkaPrdGroupDataRow(WonkaRefGroup poGroup, Dictionary<string, string> poDataRow) : this(poGroup)
        {
            if ((poDataRow != null) && (poDataRow.Count > 0))
            {
                foreach (string sTmpAttrName in poDataRow.Keys)
                {
                    string sTmpValue = poDataRow[sTmpAttrName];

                    WonkaRefAttr TmpAttribute =
                        WonkaRefEnvironment.GetInstance().GetAttributeByAttrName(sTmpAttrName);

                    this[TmpAttribute.AttrId] = sTmpValue;
                }
            }
        }

        public WonkaPrdGroupDataRow(WonkaPrdGroupDataRow poOriginalRow) : base(poOriginalRow)
        {
            this.GroupId     = poOriginalRow.GroupId;
            this.GroupName   = poOriginalRow.GroupName;
            this.AttrIds     = poOriginalRow.AttrIds;
            this.KeyAttrIds  = poOriginalRow.KeyAttrIds;
            this.MasterGroup = poOriginalRow.MasterGroup;
        }

        #region IDisposable Methods
        public void Dispose()
        {
            ClearData();
        }
        #endregion

        #region Operators

        /// <summary>
        /// 
        /// This method will compare two DataRows to see if they are equal.
        /// 
        /// NOTE: Currently, it doesn't compare them exactly.  It just ensures that
        /// the contents of 'r1' are the same in 'r2', implying that 'r2' can be a
        /// superset of 'r1'.
        /// 
        /// </summary>
        /// <param name="r1">The left-hand data row</param>
        /// <param name="r2">The right-hand data row</param>
        /// <returns>Bool that indicates whether or not the two DataRows are equal</returns>
        public static bool operator ==(WonkaPrdGroupDataRow r1, WonkaPrdGroupDataRow r2)
        {
            foreach (int nAttrId in r1.Keys)
            {
                if (r1[nAttrId] != r2[nAttrId])
                    return false;
            }

            return true;
        }

        public static bool operator !=(WonkaPrdGroupDataRow r1, WonkaPrdGroupDataRow r2)
        {
            return !(r1 == r2);
        }

        #endregion

        #region Methods

        public void ClearData()
        {
            this.Clear();
        }

        /// <summary>
        /// 
        /// This method will compare two DataRows to see if their Fields are equal.
        /// 
        /// NOTE: Currently, it doesn't compare them exactly.  It just ensures that
        /// the contents of the Field in 'r1' are the same in 'r2', implying that 
        /// the'r2' Field can be a superset of 'r1'.
        /// 
        /// </summary>
        /// <param name="poThatDataRow">The data row that this one is being compared to</param>
        /// <returns>Bool that indicates whether or not the two DataRows are equal</returns>
        public bool Equals(WonkaPrdGroupDataRow poThatDataRow, WonkaRefField poField)
        {
            foreach (int nAttrId in poField.AttrIds)
            {
                if (this[nAttrId] != poThatDataRow[nAttrId])
                    return false;
            }

            return true;
        }

        public string GetData(int pnAttrId)
        {
            return this[pnAttrId];
        }

        internal WonkaPrdGroupDataRowKey GetKey()
        {
            return Key;
        }

        /// <summary>
        /// 
        /// This method will detect if this Data Row is null (i.e., empty with no values present).
        /// 
        /// NOTE: If 'bIgnoreDeletedRows' is set to 'true', then it will ignore empty rows that 
        /// are marked with their group_seq being '0' (which is considered a logical Delete from
        /// the product).
        /// 
        /// </summary>
        /// <param name="bIgnoreDeletedRows">The indicator for whether we should evaluate rows marked for Deletion</param>
        /// <returns>Bool whether the row is empty (i.e., null)</returns>
        public bool IsNull(bool bIgnoreDeletedRows = true)
        {
            if (bIgnoreDeletedRows && MasterGroup.IsSequenced)
            {
                if (this[GroupSeqAttrId] == "0")
                    return true;
            }

            foreach (int nAttrId in this.Keys)
            {
                string sValue = this[nAttrId];

                if (sValue.Length > 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// This method will detect if this Data Row is null (i.e., empty with no values present)
        /// with respect to those Attributes in the indicated Field.
        /// 
        /// </summary>
        /// <param name="poField">The Field whose Attributes are being inspected</param>
        /// <returns>Bool whether the row is empty (i.e., null) in terms of the Attributes for the Field</returns>
        public bool IsNull(WonkaRefField poField)
        {
            foreach (int nAttrId in poField.AttrIds)
            {
                string sValue = this[nAttrId];

                if (sValue.Length > 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// This method will merge the contents of ThatDataRow with the contents of this one, storing
        /// the results in here.
        /// 
        /// NOTE: It will only accept actual values (i.e., not null and not empty) from ThatDataRow
        /// when it does the merge.
        /// 
        /// </summary>
        /// <param name="ThatDataRow">The DataRow whose values are going to be merged into this DataRow</param>
        /// <returns>None</returns>
        public void Merge(WonkaPrdGroupDataRow ThatDataRow)
        {
            foreach (int TempAttrId in AttrIds)
            {
                string sThatValue = ThatDataRow[TempAttrId];

                if (!string.IsNullOrEmpty(sThatValue))
                    this[TempAttrId] = ThatDataRow[TempAttrId];
            }
        }

        /// <summary>
        /// This method will set an Attribute within the Row.
        /// </summary>
        /// <param name="pnAttrId">The ID of the Attribute that we are setting with a value</param>
        /// <param name="psNewValue">The value to set the Attribute</param>
        /// <returns>None</returns>
        public void SetData(int pnAttrId, string psNewValue)
        {
            if (WonkaRefEnvironment.GetInstance().DoesAttributeExist(pnAttrId))
                this[pnAttrId] = psNewValue;
            else
                throw new Exception("ERROR!  Attribute ID(" + pnAttrId + ") does not exist.");
        }

        /// <summary>
        /// This method will set all of the Attributes within this Row to those in the provided one.
        /// </summary>
        /// <param name="poOriginal">The DataRow that we are copying all of the Attribute values from</param>
        /// <returns>None</returns>
        public void SetData(WonkaPrdGroupDataRow poOriginal)
        {
            foreach (int TmpAttrId in poOriginal.Keys)
                this[TmpAttrId] = poOriginal[TmpAttrId];
        }

        /// <summary>
        /// This method will set all of the Attributes (for the target Field) within this Row to 
        /// those in the provided one.
        /// </summary>
        /// <param name="poOriginal">The DataRow that we are copying all of the Attribute values from</param>
        /// <param name="poField">The subset of Attributes that we are targeting for the copy</param>
        /// <returns>None</returns>
        public void SetData(WonkaPrdGroupDataRow poOriginal, WonkaRefField poField)
        {
            foreach (int TmpAttrId in poField.AttrIds)
                this[TmpAttrId] = poOriginal[TmpAttrId];
        }

        #endregion

        #region Properties

        public int GroupId { get; }

        public int GroupSeqAttrId { get; }

        public string GroupName { get; }

        public HashSet<int> AttrIds { get; }

        public HashSet<int> KeyAttrIds { get; }

        public WonkaRefGroup MasterGroup { get; }

        private WonkaPrdGroupDataRowKey Key { get; set; }

        #endregion
    }
}
