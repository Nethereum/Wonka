using System;

using Nethereum.ABI.FunctionEncoding.Attributes;

namespace WonkaEth.Orchestration.BlockchainEvents
{
    [Event("RuleSetError")]
    public class RuleSetErrorEvent
    {
        [Parameter("address", "ruler", 1, true)]
        public string TreeOwner { get; set; }

        [Parameter("bytes32", "ruleSetId", 2, true)]
        public string RuleSetId { get; set; }

        [Parameter("bool", "severeFailure", 3, false)]
        public bool SevereFailure { get; set; }
    }
}
