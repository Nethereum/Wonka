using System;

using Wonka.Eth.Orchestration.BlockchainOutput;

namespace Wonka.Eth.Orchestration
{
    public class WonkaOrchestratorException : Exception
    {
        public readonly WonkaRuleTreeReport RuleTreeReport = null;

        public WonkaOrchestratorException(string psErrorMessage) 
            : base(psErrorMessage)
        { }

        public WonkaOrchestratorException(WonkaRuleTreeReport poReport)
        {
            RuleTreeReport = poReport;
        }
    }
}