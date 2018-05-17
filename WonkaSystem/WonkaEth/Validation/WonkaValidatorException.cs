using System;

namespace WonkaEth.Validation
{
    public class WonkaValidatorException : Exception
    {
        public readonly WonkaRuleTreeReport RuleTreeReport = null;

        public WonkaValidatorException(WonkaRuleTreeReport poReport)
        {
            RuleTreeReport = poReport;
        }
    }
}
