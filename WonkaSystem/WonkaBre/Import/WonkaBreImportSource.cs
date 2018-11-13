using System.Collections.Generic;

using WonkaRef;

namespace WonkaBre.Import
{
    public class WonkaBreImportSource : IMetadataRetrievable
    {
        public WonkaBreImportSource()
        {
            AttrCollection = new List<WonkaRefAttr>();
        }

        public void AddAttribute(WonkaRefAttr poNewAttribute)
        {
            AttrCollection.Add(poNewAttribute);
        }

        #region Required Interface Methods

        #region Standard Metadata Cache (Minimum Set)

        public List<WonkaRefAttr> GetAttrCache()
        {
            return AttrCollection;
        }

        public List<WonkaRefCurrency> GetCurrencyCache()
        {
            List<WonkaRefCurrency> CurrencyCache = new List<WonkaRefCurrency>();

            // NOTE: Not necessary

            return CurrencyCache;
        }

        public List<WonkaRefField> GetFieldCache()
        {
            List<WonkaRefField> FieldCache = new List<WonkaRefField>();

            // NOTE: To be implemented

            return FieldCache;
        }

        public List<WonkaRefGroup> GetGroupCache()
        {
            List<WonkaRefGroup> GroupCache = new List<WonkaRefGroup>();

            // NOTE: To be implemented

            return GroupCache;
        }

        public List<WonkaRefSource> GetSourceCache()
        {
            List<WonkaRefSource> SourceCache = new List<WonkaRefSource>();

            // NOTE: To be implemented

            return SourceCache;
        }

        public List<WonkaRefSourceField> GetSourceFieldCache()
        {
            List<WonkaRefSourceField> SourceFieldCache = new List<WonkaRefSourceField>();

            // NOTE: To be implemented

            return SourceFieldCache;
        }

        public List<WonkaRefStandard> GetStandardCache()
        {
            List<WonkaRefStandard> StandardCache = new List<WonkaRefStandard>();

            // NOTE: To be implemented

            return StandardCache;
        }

        #endregion

        #region Extended Metadata Cache

        public List<WonkaRefAttrCollection> GetAttrCollectionCache()
        {
            List<WonkaRefAttrCollection> AttrCollCache = new List<WonkaRefAttrCollection>();

            // NOTE: To be implemented

            return AttrCollCache;
        }

        #endregion

        #endregion

        #region Properties

        private List<WonkaRefAttr> AttrCollection;

        #endregion
    }
}
