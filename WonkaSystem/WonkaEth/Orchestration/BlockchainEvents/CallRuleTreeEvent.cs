using System;

using Nethereum.ABI.FunctionEncoding.Attributes;

namespace WonkaEth.Orchestration.BlockchainEvents
{
    public class CallRuleTreeEvent
    {
        [Parameter("address", "ruler", 1, true)]
        public string TreeOwner { get; set; }
    }
}
