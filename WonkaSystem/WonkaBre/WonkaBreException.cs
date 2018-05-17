using System;

namespace WonkaBre
{
    /// <summary>
    /// 
    /// This exception should be used when encountering any issue with using
    /// the Business Rules Engine.
    /// 
    /// </summary>
    class WonkaBreException : Exception
    {
        public WonkaBreException(int pnRuleSetId, int pnRuleId, string psErrorMessage)
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