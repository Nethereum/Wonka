using System;

using WonkaRef;

namespace WonkaBre.Import
{
    /// <summary>
    /// 
    /// This interface will be required when instantiating a rules engine from
    /// a third-party source (like a BizTalk configuration file).
    ///     
    /// </summary>
    public interface IRuleTreeRetrievable
    {
        IMetadataRetrievable GetMetadata();

        string GetWonkaRulesXml();

        WonkaBreRulesEngine CreateRulesEngine();
    }

}
