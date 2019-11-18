using System;

namespace Wonka.BizRulesEngine
{
    /// <summary>
    /// 
    /// This exception should be used when encountering any issue with using
    /// the Business Rules Engine.
    /// 
    /// </summary>
    public class WonkaBizRuleException : Exception
    {
        public WonkaBizRuleException(string psErrorMessage)
        {
            RuleSetId = RuleId = 0;

            Msg = psErrorMessage;
        }

        public WonkaBizRuleException(int pnRuleSetId, int pnRuleId, string psErrorMessage)
        {
            RuleSetId = pnRuleSetId;
            RuleId    = pnRuleId;
            Msg       = psErrorMessage;
        }

        public int RuleSetId { get; }

        public int RuleId { get; }

        public string Msg { get; }
    }
}