using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wonka.BizRulesEngine.Reporting
{
    /// <summary>
    /// 
    /// This interface will be required for any report containing details about the execution
    /// of a RuleTree
    ///     
    /// </summary>
    public interface IRuleTreeReport
    {
        string GetErrorString();

        int GetRuleSetSevereFailureCount();

        int GetRuleSetWarningCount();

        DateTime GetRuleTreeStartTime();

        DateTime GetRuleTreeEndTime();

        bool WasExecutedOnChain();

        bool WasExecutedSuccessfully();
    }
}
