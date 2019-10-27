using Wonka.BizRulesEngine.RuleTree;

namespace Wonka.Import.RuleTree
{
    #region Delegates

    public delegate bool ProvideRuleSet(WonkaBizRuleSet poNewRuleSet);

    #endregion

    /// <summary>
    /// 
    /// This interface will be required when creating a parsing class that will
    /// read a third-party rules file (like BizTalk BRL) and convert it into the
    /// Wonka rules markup.
    ///     
    /// </summary>
    public interface IRuleTreeParser
    {
        string ConvertIntoWonkaRuleTree(string psThirdPartyRuleTree);

        int GetChildRuleSetCount();

        void SetRuleTreeProvideDelegate(ProvideRuleSet poRuleTreeCallback);

        void SetChildRuleSetProvideDelegate(ProvideRuleSet poRuleTreeDirectChildCallback);
    }
}
