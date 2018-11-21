using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

using WonkaRef;

namespace WonkaBre.Import
{
    /// <summary>
    /// 
    /// This extensions class provides additional functionality for the Rules Engine, including the ability to
    /// import a schema from a database table and designate that as an IMetadataRetrievable instance.
    /// 
    /// </summary>
    public class WonkaBreImportFactory
    {
        #region CONSTANTS

        public const int CONST_DEFAULT_GROUP_ID = 1;

        #endregion

        private static object mLock = new object();
        private static object mIdLock = new object();

        private static int mGenAttrId = 1;

        private static WonkaBreImportFactory mInstance = null;

        Dictionary<string, IMetadataRetrievable> moCachedImports;

        private WonkaBreImportFactory()
        {
            moCachedImports = new Dictionary<string, IMetadataRetrievable>();
        }

        static private WonkaBreImportFactory CreateInstance()
        {
            lock (mLock)
            {
                if (mInstance == null)
                    mInstance = new WonkaBreImportFactory();

                return mInstance;
            }
        }

        static public WonkaBreImportFactory GetInstance()
        {
            lock (mLock)
            {
                if (mInstance == null)
                    mInstance = CreateInstance();

                return mInstance;
            }
        }

        #region Instance Methods

        private void CacheImport(string psDatabaseTable, IMetadataRetrievable poSource)
        {
            if (!String.IsNullOrEmpty(psDatabaseTable) && (poSource != null))
                moCachedImports[psDatabaseTable] = poSource;
            else
                throw new WonkaBreException(0, 0, "ERROR!  Could not cache the schema for the database table.");
        }

        private int GenerateNewAttrId()
        {
            lock (mIdLock)
            {
                return mGenAttrId++;
            }
        }

        public IMetadataRetrievable ImportSource(string psDatabaseTable, System.Data.Entity.DbContext poDbContext)
        {
            var adapter       = (System.Data.Entity.Infrastructure.IObjectContextAdapter) poDbContext;
            var objectContext = adapter.ObjectContext;

            return ImportSource(psDatabaseTable, objectContext);
        }

        public IMetadataRetrievable ImportSource(string psDatabaseTable, ObjectContext poDbContext)
        {
            WonkaBreImportSource NewImportSource = new WonkaBreImportSource();

            HashSet<string> KeyColNames = new HashSet<string>();

            if (!String.IsNullOrEmpty(psDatabaseTable) && (poDbContext != null))
            {
                if (moCachedImports.ContainsKey(psDatabaseTable))
                    return moCachedImports[psDatabaseTable];

                var tables =
                    poDbContext.MetadataWorkspace.GetItems(DataSpace.CSpace).Where(m => m.BuiltInTypeKind == BuiltInTypeKind.EntityType);

                foreach (var TmpTable in tables)
                {
                    EntityType TmpEntityType = (EntityType) TmpTable;

                    if (TmpEntityType.Name == psDatabaseTable)
                    {
                        var KeyCols = TmpEntityType.KeyMembers;
                        foreach (var KeyCol in KeyCols)
                            KeyColNames.Add(KeyCol.Name);

                        break;
                    }
                }

                var columns =
                    from meta in poDbContext.MetadataWorkspace.GetItems(DataSpace.CSpace).Where(m => m.BuiltInTypeKind == BuiltInTypeKind.EntityType)
                    from p in (meta as EntityType).Properties.Where(p => p.DeclaringType.Name == psDatabaseTable)
                    select new
                    {
                        colName   = p.Name,                       
                        colType   = p.TypeUsage.EdmType,
                        doc       = p.Documentation,
                        maxLength = p.MaxLength,
                        precision = p.Precision,
                        scale     = p.Scale,
                        defValue  = p.DefaultValue,
                        props     = p.MetadataProperties
                    };

                foreach (var TmpCol in columns)
                {
                    string sTmpColName = TmpCol.colName;
                    var    Props       = TmpCol.props;

                    /*
                    var propertyInfo = entity.Entity.GetType().GetProperty(propertyName);
                    var propertyType = propertyInfo.PropertyType;                    
                    string sTmpColName = poReader.GetName(i);
                    */

                    WonkaRefAttr TmpWonkaAttr = new WonkaRefAttr();

                    TmpWonkaAttr.AttrId   = GenerateNewAttrId();
                    TmpWonkaAttr.AttrName = sTmpColName;
                    TmpWonkaAttr.ColName  = sTmpColName;
                    TmpWonkaAttr.TabName  = psDatabaseTable;

                    TmpWonkaAttr.DefaultValue = Convert.ToString(TmpCol.defValue);
                    TmpWonkaAttr.Description  = (TmpCol.doc != null) ? TmpCol.doc.LongDescription : "";

                    TmpWonkaAttr.IsDate    = IsTypeDate(TmpCol.colType);
                    TmpWonkaAttr.IsNumeric = IsTypeNumeric(TmpCol.colType);
                    TmpWonkaAttr.IsDecimal = IsTypeDecimal(TmpCol.colType);

                    if (TmpWonkaAttr.IsNumeric || TmpWonkaAttr.IsDecimal)
                    {
                        TmpWonkaAttr.Precision = (int) ((TmpCol.precision != null) ? TmpCol.precision : 0);
                        TmpWonkaAttr.Scale     = (int) ((TmpCol.scale != null) ? TmpCol.scale : 0);
                    }

                    TmpWonkaAttr.MaxLength = (TmpCol.maxLength != null) ? (int)TmpCol.maxLength : 0;

                    TmpWonkaAttr.FieldId   = TmpWonkaAttr.AttrId + 1000;
                    TmpWonkaAttr.GroupId   = CONST_DEFAULT_GROUP_ID;
                    TmpWonkaAttr.IsAudited = true;

                    TmpWonkaAttr.IsKey = KeyColNames.Contains(TmpWonkaAttr.AttrName);

                    NewImportSource.AddAttribute(TmpWonkaAttr);
                }

                if (NewImportSource.GetAttrCache().Count <= 0)
                    throw new WonkaBreException(0, 0, "ERROR!  Could not import the schema because the Reader's field count was zero.");
            }
            else
                throw new WonkaBreException(0, 0, "ERROR!  Could not import the schema for the database table.");

            PopulateDefaults();

            return NewImportSource;
        }

        public void PopulateDefaults()
        {
            // NOTE: Do work here
        }

        public static bool IsTypeDate(EdmType edmType)
        {
            switch (edmType.Name)
            {
                case "DateTime":
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsTypeDecimal(EdmType edmType)
        {
            switch (edmType.Name)
            {
                case "Float":
                case "Double":
                case "Decimal":
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsTypeNumeric(EdmType edmType)
        {
            switch (edmType.Name)
            {
                case "String":
                case "Guid":
                case "DateTime":
                    return false;

                case "Int32":
                    return true;

                case "Single":
                case "Double":
                    return true;

                default:
                    return false;
            }
        }

        #endregion

    }
}

