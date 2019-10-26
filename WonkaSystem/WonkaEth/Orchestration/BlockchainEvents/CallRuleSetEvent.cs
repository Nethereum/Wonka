using System;

using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Wonka.Eth.Orchestration.BlockchainEvents
{
    [Event("CallRuleSet")]
    public class CallRuleSetEvent
    {
        [Parameter("address", "ruler", 1, true)]
        public string TreeOwner { get; set; }

        [Parameter("bytes32", "tmpRuleSetId", 2, true)]
        public string RuleSetId { get; set; }
    }
}