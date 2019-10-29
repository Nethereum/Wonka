using System;
using System.Collections.Generic;

using Wonka.MetaData;

namespace Wonka.MetaData.Extensions
{
    public class WonkaRefDummySource : IMetadataRetrievable
    {
        public WonkaRefDummySource()
        { }

        #region Required Interface Methods

        #region Standard Metadata Cache (Minimum Set)

        public List<WonkaRefAttr> GetAttrCache()
        {
            return new List<WonkaRefAttr>();
        }

        public List<WonkaRefCurrency> GetCurrencyCache()
        {
            return new List<WonkaRefCurrency>();
        }

        public List<WonkaRefField> GetFieldCache()
        {
            return new List<WonkaRefField>();
        }

        public List<WonkaRefGroup> GetGroupCache()
        {
            return new List<WonkaRefGroup>();
        }

        public List<WonkaRefSource> GetSourceCache()
        {
            return new List<WonkaRefSource>();
        }

        public List<WonkaRefSourceField> GetSourceFieldCache()
        {
            return new List<WonkaRefSourceField>();
        }

        public List<WonkaRefStandard> GetStandardCache()
        {
            return new List<WonkaRefStandard>();
        }

        #endregion

        #region Extended Metadata Cache

        public List<WonkaRefAttrCollection> GetAttrCollectionCache()
        {
            return new List<WonkaRefAttrCollection>();
        }

        #endregion

        #endregion
    }
}
