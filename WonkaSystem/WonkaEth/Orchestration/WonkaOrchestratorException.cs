using System;

using WonkaEth.Orchestration.BlockchainOutput;

namespace WonkaEth.Orchestration
{
    public class WonkaOrchestratorException : Exception
    {
        public readonly WonkaRuleTreeReport RuleTreeReport = null;

        public WonkaOrchestratorException(string psErrorMessage) : base(psErrorMessage)
        { }

        public WonkaOrchestratorException(WonkaRuleTreeReport poReport)
        {
            RuleTreeReport = poReport;
        }
    }
}