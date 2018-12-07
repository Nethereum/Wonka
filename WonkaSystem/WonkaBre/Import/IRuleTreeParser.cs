using System;

using WonkaBre.RuleTree;

namespace WonkaBre.Import
{
    /// <summary>
    /// 
    /// This interface will be required when creating a parsing class that will
    /// read a third-party rules file (like BizTalk BRL) and convert it into the
    /// Wonka rules markup.
    ///     
    /// </summary>
    public interface IRuleTreeParser
    {
        WonkaBreRuleSet GetRuleTree();

        WonkaBreRuleSet GetNextChildRuleSet();

        int GetChildRuleSetCount();
    }
}
