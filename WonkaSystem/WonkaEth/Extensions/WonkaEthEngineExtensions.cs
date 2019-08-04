using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3.Accounts;

using WonkaEth.Contracts;
using WonkaEth.Init;

namespace WonkaEth.Extensions
{
    public static class WonkaEthEngineExtensions
    {
        private const int CONST_DEPLOY_ENGINE_CONTRACT_GAS_COST = 8388608;

        /// <summary>
        /// 
        /// This method will use Nethereum to call upon (or create) an instance of the Ethgine contract and 
        /// to create a RuleTree that will be owned by the Sender.
        /// 
        /// <returns>Indicates whether or not the RuleTree was created to the blockchain</returns>
        /// </summary>
        public static bool Serialize(this WonkaEthEngineInitialization poEngineInitData)
        {
            bool bResult = false;

            if ((poEngineInitData != null) && (poEngineInitData.RulesEngine != null))
            {
                var account = new Account(poEngineInitData.EthPassword);

                Nethereum.Web3.Web3 web3 = null;
                if (!String.IsNullOrEmpty(poEngineInitData.Web3HttpUrl))
                    web3 = new Nethereum.Web3.Web3(account, poEngineInitData.Web3HttpUrl);
                else
                    web3 = new Nethereum.Web3.Web3(account);

                if (String.IsNullOrEmpty(poEngineInitData.RulesEngineContractAddress))
                {
                    var EngineDeployment = new Autogen.WonkaEngine.WonkaEngineDeployment();

                    HexBigInteger nDeployGas = new HexBigInteger(CONST_DEPLOY_ENGINE_CONTRACT_GAS_COST);

                    poEngineInitData.RulesEngineContractAddress = 
                        EngineDeployment.DeployContract(web3, poEngineInitData.RulesEngineABI, poEngineInitData.EthSenderAddress, nDeployGas, poEngineInitData.Web3HttpUrl);
                }

                if (String.IsNullOrEmpty(poEngineInitData.RegistryContractAddress))
                {
                    var RegistryDeployment = new Autogen.WonkaRegistry.WonkaRegistryDeployment();

                    poEngineInitData.RegistryContractAddress =
                        RegistryDeployment.DeployContract(web3, poEngineInitData.RegistryContractABI, poEngineInitData.EthSenderAddress, poEngineInitData.Web3HttpUrl);
                }

                if (String.IsNullOrEmpty(poEngineInitData.TestContractAddress))
                {
                    var TestContractDeployment = new Autogen.WonkaTestContract.WonkaTestContractDeployment();

                    poEngineInitData.TestContractAddress =
                        TestContractDeployment.DeployContract(web3, poEngineInitData.TestContractABI, poEngineInitData.EthSenderAddress, poEngineInitData.Web3HttpUrl);
                }

                if (poEngineInitData.RulesEngine.RefEnvHandle != null)
                {
                    bResult =
                        poEngineInitData.RulesEngine.RefEnvHandle.Serialize(poEngineInitData.EthRuleTreeOwnerAddress,
                                                                            poEngineInitData.EthPassword,
                                                                            poEngineInitData.EthSenderAddress,
                                                                            poEngineInitData.RulesEngineContractAddress,
                                                                            poEngineInitData.RulesEngineABI,
                                                                            poEngineInitData.Web3HttpUrl);

                    if (bResult)
                    {
                        bResult =
                            poEngineInitData.RulesEngine.Serialize(poEngineInitData.EthRuleTreeOwnerAddress,
                                                                   poEngineInitData.EthPassword,
                                                                   poEngineInitData.EthSenderAddress,
                                                                   poEngineInitData.RulesEngineContractAddress,
                                                                   poEngineInitData.RulesEngineABI,
                                                                   poEngineInitData.RulesEngineContractAddress,
                                                                   poEngineInitData.Web3HttpUrl);
                    }
                }
            }

            return bResult;
        }
    }
}
