using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Wonka.BizRulesEngine;
using Wonka.BizRulesEngine.RuleTree;
using WonkaRef;

namespace WonkaEth.Init
{
	public class WonkaEthEngineProps
	{
		public WonkaEthEngineProps()
		{
			MetadataSource = null;
			RulesMarkupXml = null; 
			RulesEngine    = null;
			SourceMap      = null;

			DotNetRetrieveMethod = null;
		}

		public IMetadataRetrievable MetadataSource { get; set; }

		public string RulesMarkupXml { get; set; }

		public WonkaBizRulesEngine RulesEngine { get; set; }

		public Dictionary<string, WonkaBizSource> SourceMap { get; set; }

		public WonkaBizSource.RetrieveDataMethod DotNetRetrieveMethod { get; set; }
	}

	public class WonkaEthEngineInitialization
    {
        public WonkaEthEngineInitialization()
        {
            Engine      = new WonkaEthEngineProps();
            Web3HttpUrl = "";

            EthSenderAddress = EthPassword = EthRuleTreeOwnerAddress = "";

            RulesEngineABI      = WonkaEth.Autogen.WonkaEngine.WonkaEngineDeployment.ABI;
            RegistryContractABI = WonkaEth.Autogen.WonkaRegistry.WonkaRegistryDeployment.ABI;
            StorageContractABI  = WonkaEth.Autogen.WonkaTestContract.WonkaTestContractDeployment.ABI;

            RulesEngineContractAddress = "";
            RegistryContractAddress    = "";

            UsingStorageContract   = true;
            StorageContractAddress = "";
			StorageDefaultSourceId = StorageGetterMethod = StorageSetterMethod = "";

            UsingTrxStateContract   = false;
            TrxStateContractAddress = null;
        }

		public WonkaEthEngineProps Engine { get; set; }

        public string Web3HttpUrl { get; set; }

        public string EthSenderAddress { get; set; }

        public string EthPassword { get; set; }

        public string EthRuleTreeOwnerAddress { get; set; }

        public string RulesEngineContractAddress { get; set; }

        public string RulesEngineABI { get; set; }

        public string RegistryContractAddress { get; set; }

        public string RegistryContractABI { get; set; }

        public bool UsingStorageContract { get; set; }

        public string StorageContractAddress { get; set; }

        public string StorageContractABI { get; set; }

		public string StorageDefaultSourceId { get; set; }

		public string StorageGetterMethod { get; set; }

		public string StorageSetterMethod { get; set; }

        public bool UsingTrxStateContract { get; set; }

        public string TrxStateContractAddress { get; set; }

    }
}
