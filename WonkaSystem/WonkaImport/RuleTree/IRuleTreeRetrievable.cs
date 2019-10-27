using System;

using Wonka.BizRulesEngine;
using WonkaRef;

namespace Wonka.Import.RuleTree
{
    public enum BRE_IMPORT_SOURCE
    {
        BRE_SOURCE_TYPE_BIZTALK = 1,
        BRE_SOURCE_TYPE_OTHER,
        BRE_SOURCE_TYPE_MAX
    }

    /// <summary>
    /// 
    /// This interface will be required when instantiating a rules engine from
    /// a third-party source (like a BizTalk configuration file).
    ///     
    /// </summary>
    public interface IRuleTreeRetrievable
    {
        WonkaBizRulesEngine CreateRulesEngine();

        BRE_IMPORT_SOURCE GetImportSourceType();

        IMetadataRetrievable GetMetadata();

        IRuleTreeRetrievable GetRulesParser();

        string GetWonkaRulesXml();
    }

}