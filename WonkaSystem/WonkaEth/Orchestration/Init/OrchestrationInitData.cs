using System.Collections;
using System.Collections.Generic;

using WonkaBre;
using WonkaBre.RuleTree;
using WonkaRef;

namespace WonkaEth.Orchestration.Init
{
    public class OrchestrationInitData
    {
        public OrchestrationInitData()
        {
            Web3HttpUrl                 = null;
            AttributesMetadataSource    = null;
            BlockchainEngine            = null;
            TrxStateContractAddress     = null;
            DefaultBlockchainDataSource = null;
            BlockchainDataSources       = null;
            BlockchainCustomOpFunctions = null;
        }

        public string Web3HttpUrl;

        public IMetadataRetrievable AttributesMetadataSource;

        public WonkaBreSource BlockchainEngine;

        public string TrxStateContractAddress;

        public WonkaBreSource DefaultBlockchainDataSource;

        public Dictionary<string, WonkaBreSource> BlockchainDataSources;

        public Dictionary<string, WonkaBreSource> BlockchainCustomOpFunctions;
    }
}
