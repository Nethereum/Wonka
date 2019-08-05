using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WonkaBre;

namespace WonkaEth.Init
{
    public class WonkaEthEngineInitialization
    {
        public WonkaEthEngineInitialization()
        {
            RulesEngine = null;
            Web3HttpUrl = "";

            EthSenderAddress = EthPassword = EthRuleTreeOwnerAddress = "";

            RulesEngineABI      = WonkaEth.Autogen.WonkaEngine.WonkaEngineDeployment.ABI;
            RegistryContractABI = WonkaEth.Autogen.WonkaRegistry.WonkaRegistryDeployment.ABI;
            TestContractABI     = WonkaEth.Autogen.WonkaTestContract.WonkaTestContractDeployment.ABI;

            RulesEngineContractAddress = "";
            RegistryContractAddress    = "";

            UsingTestContract   = true;
            TestContractAddress = "";

            UsingTrxStateContract   = false;
            TrxStateContractAddress = null;
        }

        public WonkaBreRulesEngine RulesEngine { get; set; }

        public string Web3HttpUrl { get; set; }

        public string EthSenderAddress { get; set; }

        public string EthPassword { get; set; }

        public string EthRuleTreeOwnerAddress { get; set; }

        public string RulesEngineContractAddress { get; set; }

        public string RulesEngineABI { get; set; }

        public string RegistryContractAddress { get; set; }

        public string RegistryContractABI { get; set; }

        public bool UsingTestContract { get; set; }

        public string TestContractAddress { get; set; }

        public string TestContractABI { get; set; }

        public bool UsingTrxStateContract { get; set; }

        public string TrxStateContractAddress { get; set; }

    }
}
