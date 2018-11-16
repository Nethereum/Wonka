using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace WonkaRef
{
    /// <summary>
    /// 
    /// This singleton, when initialized, will contain all of the cached metadata
    /// that drives the Wonka System.
    /// 
    /// </summary>
    [DataContract(Namespace = "http://sample.wonkasystem.com")]
    public class WonkaRefEnvironment
    {
        private static object              mLock     = new object();
        private static WonkaRefEnvironment mInstance = null;

        // NOTE: This constructor is necessary for serialization/deserialization purposes
        private WonkaRefEnvironment()
        { }

        private WonkaRefEnvironment(bool bAllMetadata, IMetadataRetrievable pMetadataRetrievable)
        {
            DebugLevel = 1;

            DefaultCommitThreshold = 500;

            IdXref = new WonkaRefIdXref();

            AttrCache              = pMetadataRetrievable.GetAttrCache();
            AttrCollectionCache    = pMetadataRetrievable.GetAttrCollectionCache();
            CurrencyCache          = pMetadataRetrievable.GetCurrencyCache();
            FieldCache             = pMetadataRetrievable.GetFieldCache();
            GroupCache             = pMetadataRetrievable.GetGroupCache();
            SourceCache            = pMetadataRetrievable.GetSourceCache();
            SourceFieldCache       = pMetadataRetrievable.GetSourceFieldCache();
            StandardCache          = pMetadataRetrievable.GetStandardCache();

            // NOTE: To be implemented later
            // CodeDefCache           = pMetadataRetrievable.GetCodeDefCache();

            AttrKeys       = new List<WonkaRefAttr>();
            AttrMap        = new Dictionary<int, WonkaRefAttr>();
            FieldMap       = new Dictionary<int, WonkaRefField>();
            GroupMap       = new Dictionary<int, WonkaRefGroup>();
            SourceMap      = new Dictionary<int, WonkaRefSource>();
            SourceFieldMap = new Dictionary<int, WonkaRefSourceField>();

            foreach (WonkaRefAttr TmpAttribute in AttrCache)
            {
                if (TmpAttribute.IsKey)
                    AttrKeys.Add(TmpAttribute);
                
                if (!IdXref.FieldIdToAttrIds.Keys.Contains(TmpAttribute.FieldId))
                    IdXref.FieldIdToAttrIds[TmpAttribute.FieldId] = new HashSet<int>();

                IdXref.FieldIdToAttrIds[TmpAttribute.FieldId].Add(TmpAttribute.AttrId);

                if (!IdXref.GroupIdToAttrIds.Keys.Contains(TmpAttribute.GroupId))
                    IdXref.GroupIdToAttrIds[TmpAttribute.GroupId] = new HashSet<int>();

                IdXref.GroupIdToAttrIds[TmpAttribute.GroupId].Add(TmpAttribute.AttrId);

                if (!IdXref.GroupIdToFieldIds.Keys.Contains(TmpAttribute.GroupId))
                    IdXref.GroupIdToFieldIds[TmpAttribute.GroupId] = new HashSet<int>();

                IdXref.GroupIdToFieldIds[TmpAttribute.GroupId].Add(TmpAttribute.FieldId);

                if (!String.IsNullOrEmpty(TmpAttribute.ColName) && (TmpAttribute.ColName == GetStandardByStdName("GSCName").StandardValue))
                    IdXref.GroupIdToGroupSeqAttrId[TmpAttribute.GroupId] = TmpAttribute.AttrId;

                AttrMap[TmpAttribute.AttrId] = TmpAttribute;
            }

            foreach (WonkaRefGroup TmpGroup in GroupCache)
            {
                if (TmpGroup.KeyTabCols.Count > 0)
                {
                    IdXref.GroupIdToKeyAttrIds[TmpGroup.GroupId] = new HashSet<int>();

                    foreach (string sTmpKeyTabCol in TmpGroup.KeyTabCols)
                    {
                        int nTargetAttrId = GetAttributeByTabColName(sTmpKeyTabCol).AttrId;
                        IdXref.GroupIdToKeyAttrIds[TmpGroup.GroupId].Add(nTargetAttrId);
                    }
                }

                GroupMap[TmpGroup.GroupId] = TmpGroup;
            }

            foreach (WonkaRefField TmpField in FieldCache)
                FieldMap[TmpField.FieldId] = TmpField;

            foreach (WonkaRefSource TmpSource in SourceCache)
                SourceMap[TmpSource.SourceId] = TmpSource;

            foreach (WonkaRefSourceField TmpSrcField in SourceFieldCache)
                SourceFieldMap[TmpSrcField.SourceFieldId] = TmpSrcField;            

            if (bAllMetadata)
            {
                AttrCollectionCache    = pMetadataRetrievable.GetAttrCollectionCache();

                // NOTE: To be implemented later
                // CodeDependencyCache    = pMetadataRetrievable.GetCodeDependencyCache();
                // CodeDependencyDefCache = pMetadataRetrievable.GetCodeDependencyDefCache();
            }
        }

        static public WonkaRefEnvironment CreateInstance(bool bAllMetadataData, IMetadataRetrievable pMetadataRetrievable)
        {
            lock (mLock)
            {
                if (mInstance == null)
                    mInstance = new WonkaRefEnvironment(bAllMetadataData, pMetadataRetrievable);

                return mInstance;
            }
        }

        static public WonkaRefEnvironment GetInstance()
        {
            lock (mLock)
            {
                if (mInstance == null)
                    throw new Exception("ERROR!  WonkaRefEnvironment has not yet been initialized!");

                return mInstance;
            }
        }

        #region Methods

        public void Debug(string psDebugMessage, int pnLevel)
        {
            // NOTE: To be ported later
        }

        public bool DoesAttributeExist(int pnAttrId)
        {
            return AttrMap.Keys.Contains(pnAttrId);
        }

        public bool DoesFieldExist(int pnFieldId)
        {
            return FieldMap.Keys.Contains(pnFieldId);
        }

        public bool DoesGroupExist(int pnGroupId)
        {
            return GroupMap.Keys.Contains(pnGroupId);
        }

        public bool DoesGroupExist(string psGroupName)
        {
            WonkaRefGroup TestGroup = GetGroupByGroupName(psGroupName);
            return (TestGroup.GroupId > 0);
        }

        public WonkaRefAttr GetAttributeByAttrId(int pnAttrId)
        {
            if (!AttrMap.Keys.Contains(pnAttrId))
                throw new Exception("ERROR!  Attr ID (" + pnAttrId + ") does not exist.");

            return AttrMap[pnAttrId];
        }

        public WonkaRefAttr GetAttributeByAttrName(string psAttrName)
        {
            return AttrCache.Where(x => x.AttrName == psAttrName).FirstOrDefault();
        }

        public WonkaRefAttr GetAttributeByColName(string psColName)
        {
            return AttrCache.Where(x => x.ColName == psColName).FirstOrDefault();
        }

        public WonkaRefAttrCollection GetAttrCollectionById(int nAttrCollId)
        {
            return AttrCollectionCache.Where(x => x.AttrCollectionId == nAttrCollId).FirstOrDefault();
        }

        public HashSet<int> GetAttrIdsByFieldId(int pnFieldId)
        {
            return new HashSet<int>(IdXref.FieldIdToAttrIds[pnFieldId]);
        }

        public WonkaRefAttr GetAttributeByTabColName(string psTabColName)
        {
            return AttrCache.Where(x => x.TabCol == psTabColName).FirstOrDefault();
        }

        public WonkaRefField GetFieldByFieldId(int pnFieldId)
        {
            if (!FieldMap.Keys.Contains(pnFieldId))
                throw new Exception("ERROR!  Field ID (" + pnFieldId + ") does not exist.");

            return FieldMap[pnFieldId];
        }

        public WonkaRefField GetFieldByFieldName(string psFieldName)
        {
            return FieldCache.Where(x => x.FieldName == psFieldName).FirstOrDefault();
        }

        public List<WonkaRefField> GetFieldsByGroupId(int pnGroupId)
        {
            HashSet<int>      oFieldIds = IdXref.GroupIdToFieldIds[pnGroupId];
            List<WonkaRefField> oFields   = new List<WonkaRefField>();

            // var oFields = DownloadedItems.Where(x => !CurrentCollection.Any(y => x.bar == y.bar));

            foreach (int nFieldId in oFieldIds)
            {
                WonkaRefField oTmpField = FieldMap[nFieldId];
                oFields.Add(oTmpField);
            }

            return oFields;
        }

        public HashSet<int> GetFieldIdsByGroupId(int pnGroupId)
        {
            return new HashSet<int>(IdXref.GroupIdToFieldIds[pnGroupId]);
        }

        public WonkaRefGroup GetGroupByGroupId(int pnGroupId)
        {
            if (!GroupMap.Keys.Contains(pnGroupId))
                throw new Exception("ERROR!  Group ID (" + pnGroupId + ") does not exist.");

            return GroupMap[pnGroupId];
        }

        public WonkaRefGroup GetGroupByGroupName(string psGroupName)
        {
            return GroupCache.Where(x => x.GroupName == psGroupName).FirstOrDefault();
        }

        public int GetGroupSeqAttrId(int pnGroupId)
        {
            return IdXref.GroupIdToGroupSeqAttrId[pnGroupId];
        }

        public WonkaRefSourceField GetSourceField(WonkaRefSource poSource, WonkaRefField poField)
        {
            if ( (poSource != null) && (poField != null))
            {
                Dictionary<int, int> FieldsToSourceFields = IdXref.SourceFields[poSource.SourceId];
                return SourceFieldCache[FieldsToSourceFields[poField.FieldId]];
            }
            else
                return null;
        }

        public WonkaRefStandard GetStandardByStdName(string psStdName)
        {
            return StandardCache.Where(x => x.StandardName == psStdName).FirstOrDefault();
        }

        static int GetThreadCount()
        {
            // NOTE: This code will be implemented later
            return 0;
        }

        public bool IsAttribute(string psAttrName)
        {
            return AttrCache.Any(x => x.AttrName == psAttrName);
        }

        public bool IsSourceField(WonkaRefSource poSource, WonkaRefField poField)
        {
            return (GetSourceField(poSource,poField) != null);
        }

        public void Log(string psLogMessage)
        {
            // NOTE: To be ported later
        }

        public bool ValidateMetadata()
        {
            /*
             * NOTE: Maybe will be implemented later
             */

            return true;
        }

        #endregion

        #region Properties 
        #region Standard Metadata Cache (Minimum Set)

        public List<WonkaRefAttr>             AttrCache { get; }
        
        public List<WonkaRefAttr>             AttrKeys { get; }

        private Dictionary<int, WonkaRefAttr> AttrMap { get; }

        public List<WonkaRefCurrency>         CurrencyCache { get; }

        public int DebugLevel { get; }

        public int DefaultCommitThreshold { get; } 

        public List<WonkaRefField>              FieldCache { get; }

        private Dictionary<int, WonkaRefField>  FieldMap { get; }

        public List<WonkaRefGroup>              GroupCache { get; }

        private Dictionary<int, WonkaRefGroup>  GroupMap { get; }

        public WonkaRefIdXref                   IdXref { get; }

        public List<WonkaRefSource>             SourceCache { get; }

        private Dictionary<int, WonkaRefSource> SourceMap { get; }

        public List<WonkaRefSourceField>        SourceFieldCache { get; }

        private Dictionary<int, WonkaRefSourceField> SourceFieldMap { get; }

        public List<WonkaRefStandard>                StandardCache { get; }

        #endregion

        #region Extended Metadata Cache

        public List<WonkaRefAttrCollection> AttrCollectionCache { get; }

        /*
         * NOTE: To be implemented later
         * 
        public List<WonkaRefCodeDef> CodeDefCache { get; }

        public List<WonkaRefCodeDependency> CodeDependencyCache { get; }

        public List<WonkaRefCodeDependencyDef> CodeDependencyDefCache { get; }
         */

        #endregion
        #endregion
    }
}

