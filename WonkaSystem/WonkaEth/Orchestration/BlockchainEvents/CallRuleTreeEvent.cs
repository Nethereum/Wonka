using System;

using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Wonka.Eth.Orchestration.BlockchainEvents
{
    [Event("CallRuleTree")]
    public class CallRuleTreeEvent
    {
        [Parameter("address", "ruler", 1, true)]
        public string TreeOwner { get; set; }
    }
}
