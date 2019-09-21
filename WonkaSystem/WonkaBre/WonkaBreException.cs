using System;

namespace WonkaBre
{
    /// <summary>
    /// 
    /// This exception should be used when encountering any issue with using
    /// the Business Rules Engine.
    /// 
    /// </summary>
    public class WonkaBreException : Exception
    {
        public WonkaBreException(string psErrorMessage)
        {
            RuleSetId = RuleId = 0;

            Msg = psErrorMessage;
        }

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