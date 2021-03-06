﻿using System.Collections.Generic;

using Wonka.MetaData;

namespace Wonka.Import.Metadata
{
    public class WonkaImportSource : IMetadataRetrievable
    {
        public WonkaImportSource()
        {
            AttrCollection   = new List<WonkaRefAttr>();
            CadreCollection  = new List<WonkaRefCadre>();
            GroupCollection  = new List<WonkaRefGroup>();
            SourceCollection = new List<WonkaRefSource>();
            SrcFldCollection = new List<WonkaRefSourceCadre>();
        }

        public void AddAttribute(WonkaRefAttr poNewAttribute)
        {
            AttrCollection.Add(poNewAttribute);
        }

        public void AddGroup(WonkaRefGroup poNewGroup)
        {
            GroupCollection.Add(poNewGroup);
        }

        public void AddField(WonkaRefCadre poNewField)
        {
            CadreCollection.Add(poNewField);
        }

        public void AddSource(WonkaRefSource poNewSource)
        {
            SourceCollection.Add(poNewSource);
        }

        public void AddSourceField(WonkaRefSourceCadre poNewSrcField)
        {
            SrcFldCollection.Add(poNewSrcField);
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

        public List<WonkaRefCadre> GetCadreCache()
        {
            return CadreCollection;
        }

        public List<WonkaRefGroup> GetGroupCache()
        {
            return GroupCollection;
        }

        public List<WonkaRefSource> GetSourceCache()
        {
            return SourceCollection;
        }

        public List<WonkaRefSourceCadre> GetSourceCadreCache()
        {
            return SrcFldCollection;
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

        private List<WonkaRefGroup> GroupCollection;

        private List<WonkaRefCadre> CadreCollection;

        private List<WonkaRefSource> SourceCollection;

        private List<WonkaRefSourceCadre> SrcFldCollection;

        #endregion
    }
}
