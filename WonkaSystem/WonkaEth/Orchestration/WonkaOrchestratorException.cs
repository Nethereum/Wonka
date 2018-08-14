using System;

namespace WonkaEth.Validation
{
    public class WonkaOrchestratorException : Exception
    {
        public readonly WonkaRuleTreeReport RuleTreeReport = null;

        public WonkaOrchestratorException(WonkaRuleTreeReport poReport)
        {
            RuleTreeReport = poReport;
        }
    }
}
