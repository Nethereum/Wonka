using System.Collections;
using System.Collections.Generic;

using Wonka.BizRulesEngine;
using Wonka.BizRulesEngine.RuleTree;
using Wonka.MetaData;

namespace Wonka.Eth.Orchestration.Init
{
    public class OrchestrationInitData
    {
        public OrchestrationInitData()
        {
            Web3HttpUrl                 = null;
            AttributesMetadataSource    = null;
            BlockchainEngineOwner       = null;
            BlockchainEngine            = null;
            TrxStateContractAddress     = null;
            DefaultBlockchainDataSource = null;
            BlockchainDataSources       = null;
            BlockchainCustomOpFunctions = null;
        }

        public string Web3HttpUrl;

        public IMetadataRetrievable AttributesMetadataSource;

        public string BlockchainEngineOwner;

        public WonkaBizSource BlockchainEngine;

        public string TrxStateContractAddress;

        public WonkaBizSource DefaultBlockchainDataSource;

        public Dictionary<string, WonkaBizSource> BlockchainDataSources;

        public Dictionary<string, WonkaBizSource> BlockchainCustomOpFunctions;
    }
}
