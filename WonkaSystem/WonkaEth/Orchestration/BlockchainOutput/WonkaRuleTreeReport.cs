using System;
using System.Collections.Generic;

using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Wonka.Eth.Orchestration.BlockchainOutput
{
    [FunctionOutput]
    public class WonkaRuleTreeReport
    {
        [Parameter("uint", "fails", 1)]
        public uint NumberOfRuleFailures { get; set; }

        [Parameter("bytes32[]", "rsets", 2)]
        public List<string> RuleSetIds { get; set; }

        [Parameter("bytes32[]", "rules", 3)]
        public List<string> RuleIds { get; set; }
    }
}