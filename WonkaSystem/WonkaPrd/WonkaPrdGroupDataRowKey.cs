using WonkaRef;

namespace WonkaPrd
{
    /// <summary>
    /// 
    /// This class represents a wrapper around the key Attributes that *should* uniquely define
    /// a DataRow within a Group.
    /// 
    /// For example, a product could have a Contributor group, where the metadata defines 
    /// its two key Attributes as Contributor ID and Contributor Role ID.
    /// 
    /// </summary>
    internal class WonkaPrdGroupDataRowKey
    {
        public WonkaPrdGroupDataRowKey(WonkaPrdGroupDataRow poParentDataRow)
        {
            this.ParentDataRow = poParentDataRow;
        }

        /// <summary>
        /// 
        /// This method will compare two DataRowKeys to see if they are equal.
        /// 
        /// NOTE: Currently, it doesn't compare them exactly.  It just ensures that
        /// the contents of 'k1' are the same in 'k2', implying that 'k2' can be a
        /// superset of 'k1'.
        /// 
        /// </summary>
        /// <param name="k1">The left-hand data row key</param>
        /// <param name="k2">The right-hand data row key</param>
        /// <returns>Bool that indicates whether or not the two DataRowKeys are equal</returns>
        public static bool operator ==(WonkaPrdGroupDataRowKey k1, WonkaPrdGroupDataRowKey k2)
        {
            foreach (int nAttrId in k1.ParentDataRow.KeyAttrIds)
            {
                if (k1.ParentDataRow[nAttrId] != k2.ParentDataRow[nAttrId])
                    return false;
            }

            return true;
        }

        public static bool operator !=(WonkaPrdGroupDataRowKey k1, WonkaPrdGroupDataRowKey k2)
        {
            return !(k1 == k2);
        }

        public WonkaPrdGroupDataRow ParentDataRow { get; set; }
    }
}

