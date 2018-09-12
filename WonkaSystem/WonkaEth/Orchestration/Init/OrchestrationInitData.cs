using System.Collections;
using System.Collections.Generic;

using WonkaBre;
using WonkaBre.RuleTree;
using WonkaRef;

namespace WonkaEth.Orchestration.Init
{
    public class OrchestrationInitData
    {
        public IMetadataRetrievable AttributesMetadataSource;

        public WonkaBreSource BlockchainEngine;

        public WonkaBreSource DefaultBlockchainDataSource;

        public Dictionary<string, WonkaBreSource> BlockchainDataSources;

        public Dictionary<string, WonkaBreSource> BlockchainCustomOpFunctions;
    }
}
