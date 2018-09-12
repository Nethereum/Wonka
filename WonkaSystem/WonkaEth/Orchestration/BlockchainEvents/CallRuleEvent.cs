using System;

using Nethereum.ABI.FunctionEncoding.Attributes;

namespace WonkaEth.Orchestration.BlockchainEvents
{
    public class CallRuleEvent
    {
        [Parameter("address", "ruler", 1, true)]
        public string TreeOwner { get; set; }

        [Parameter("bytes32", "ruleSetId", 2, true)]
        public string RuleSetId { get; set; }

        [Parameter("bytes32", "ruleId", 3, true)]
        public string RuleId { get; set; }

        [Parameter("uint", "ruleType", 4, false)]
        public uint RuleType { get; set; }
    }
}
