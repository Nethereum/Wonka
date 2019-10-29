using System.Collections.Generic;

namespace Wonka.MetaData
{
    /// <summary>
    /// 
    /// This interface will need to be implemented by a provider
    /// in order for the WonkaRefEnvironment singleton to be initialized.
    ///     
    /// </summary>
    public interface IMetadataRetrievable
    {
        #region Standard Metadata Cache (Minimum Set)

        List<WonkaRefAttr>             GetAttrCache();

        List<WonkaRefCurrency>         GetCurrencyCache();

        List<WonkaRefField>             GetFieldCache();

        List<WonkaRefGroup>             GetGroupCache();

        List<WonkaRefSource>            GetSourceCache();

        List<WonkaRefSourceField>       GetSourceFieldCache();

        List<WonkaRefStandard>          GetStandardCache();

        #endregion

        #region Extended Metadata Cache

        List<WonkaRefAttrCollection>    GetAttrCollectionCache();

        /*
         * NOTE: Will be implemented later
        List<WonkaRefCodeDef>           GetCodeDefCache();

        List<WonkaRefCodeDependency>    GetCodeDependencyCache();

        List<WonkaRefCodeDependencyDef> GetCodeDependencyDefCache();
         */

        #endregion

    }
}

