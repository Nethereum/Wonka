using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nethereum.ABI.FunctionEncoding;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3.Accounts;

using Wonka.BizRulesEngine;
using Wonka.BizRulesEngine.RuleTree;
using Wonka.Eth.Enums;
using Wonka.Eth.Init;
using Wonka.MetaData;

namespace Wonka.Eth.Extensions
{
    public static class WonkaEthEngineExtensions
    {
        private const int CONST_DEPLOY_ENGINE_CONTRACT_GAS_COST  = (int) GAS_COST.CONST_DEPLOY_ENGINE_CONTRACT_GAS_COST;
		private const int CONST_DEPLOY_DEFAULT_CONTRACT_GAS_COST = (int) GAS_COST.CONST_DEPLOY_DEFAULT_CONTRACT_GAS_COST;

		private const int CONST_GAS_PER_READ_OP  = (int) GAS_COST.CONST_GAS_PER_READ_OP;
		private const int CONST_GAS_PER_WRITE_OP = (int) GAS_COST.CONST_GAS_PER_WRITE_OP;

        public static uint CalculateMinGasEstimate(this WonkaBizGrove poTargetGrove, uint pnWriteOpGasCost = CONST_GAS_PER_WRITE_OP)
        {
            uint nMinGasCost = 0;

            if ((poTargetGrove != null) && (poTargetGrove.ExecuteRuleTreesOnChain != null) && (poTargetGrove.ExecuteRuleTreesOnChain.Count > 0))
            {
                poTargetGrove.ExecuteRuleTreesOnChain.ToList().ForEach(x => nMinGasCost += x.CalculateMinGasEstimate(pnWriteOpGasCost));
            }

            return nMinGasCost;
        }

        public static uint CalculateMinGasEstimate(this WonkaBizRulesEngine poRulesEngine, uint pnWriteOpGasCost = CONST_GAS_PER_WRITE_OP)
        {
            uint nMinGasCost = 50000;

            // NOTE: Do work here
            // 63200 gas per op, based on gas default price
            // 12 ops for Validate, 18 ops Calculate

            if ((poRulesEngine != null) && (poRulesEngine.RuleTreeRoot != null) && (poRulesEngine.RuleTreeRoot.ChildRuleSets != null))
            {
                poRulesEngine.RuleTreeRoot.ChildRuleSets.ForEach(x => nMinGasCost += (uint)(x.EvaluativeRules.Count * CONST_GAS_PER_READ_OP));
                poRulesEngine.RuleTreeRoot.ChildRuleSets.ForEach(x => nMinGasCost += (uint)(x.AssertiveRules.Count * pnWriteOpGasCost));
            }

            return nMinGasCost;
        }

        public static uint CalculateMinGasEstimate(this WonkaEthEngineProps poEngineProps, uint pnWriteOpGasCost = CONST_GAS_PER_WRITE_OP)
		{
			uint nMinGasCost = 50000;

            if ((poEngineProps.RulesEngine != null) && (poEngineProps.RulesEngine.RuleTreeRoot != null))
                nMinGasCost = poEngineProps.RulesEngine.CalculateMinGasEstimate(pnWriteOpGasCost);

            return nMinGasCost;
		}

        public static uint CalculateMaxGasEstimate(this WonkaBizGrove poTargetGrove, uint pnWriteOpGasCost = CONST_GAS_PER_WRITE_OP)
        {
            uint nMaxGasCost = 0;

            if ((poTargetGrove != null) && (poTargetGrove.ExecuteRuleTreesOnChain != null) && (poTargetGrove.ExecuteRuleTreesOnChain.Count > 0))
            {
                poTargetGrove.ExecuteRuleTreesOnChain.ToList().ForEach(x => nMaxGasCost += x.CalculateMaxGasEstimate(pnWriteOpGasCost));
            }

            return nMaxGasCost;
        }

        public static uint CalculateMaxGasEstimate(this WonkaBizRulesEngine poRulesEngine, uint pnWriteOpGasCost = CONST_GAS_PER_WRITE_OP)
        {
            uint nMaxGasCost = 50000;

            if ((poRulesEngine != null) && (poRulesEngine.RuleTreeRoot != null))
            {
                // NOTE: Do work here
                // 63200 gas per op, based on gas default price
                // 12 ops for Validate, 18 ops Calculate

                if (poRulesEngine.RuleTreeRoot.ChildRuleSets != null)
                {
                    poRulesEngine.RuleTreeRoot.ChildRuleSets.ForEach(x => nMaxGasCost += (uint)(x.EvaluativeRules.Count * CONST_GAS_PER_READ_OP));
                    poRulesEngine.RuleTreeRoot.ChildRuleSets.ForEach(x => nMaxGasCost += (uint)(x.AssertiveRules.Count * pnWriteOpGasCost));
                }

                if (poRulesEngine.AllRuleSets != null)
                {
                    poRulesEngine.AllRuleSets.ForEach(x => nMaxGasCost += (uint)(x.EvaluativeRules.Count * CONST_GAS_PER_READ_OP));

                    foreach (WonkaBizRuleSet TempRuleSet in poRulesEngine.AllRuleSets)
                    {
                        foreach (WonkaBizRule TempRule in TempRuleSet.AssertiveRules)
                        {
                            if (TempRule.RuleType == RULE_TYPE.RT_CUSTOM_OP)
                                nMaxGasCost += (uint)(3 * pnWriteOpGasCost);
                            else
                                nMaxGasCost += (uint)(pnWriteOpGasCost);
                        }
                    }
                }
            }

            return nMaxGasCost;
        }

        public static uint CalculateMaxGasEstimate(this WonkaEthEngineProps poEngineProps, uint pnWriteOpGasCost = CONST_GAS_PER_WRITE_OP)
        {
            uint nMaxGasCost = 50000;

            if ((poEngineProps.RulesEngine != null) && (poEngineProps.RulesEngine.RuleTreeRoot != null))
                nMaxGasCost = poEngineProps.RulesEngine.CalculateMaxGasEstimate(pnWriteOpGasCost);

            return nMaxGasCost;
        }

        /// <summary>
        /// 
        /// This method will deploy all contracts needed to run Wonka on-chain, using all the data provided.
        /// 
        /// <returns>None</returns>
        /// </summary>
        public static async Task<bool> DeployContractsAsync(this WonkaEthEngineInitialization poEngineInitData)
        {
            bool bResult = true;

            var EngineProps = poEngineInitData.Engine;

            if (EngineProps == null)
                throw new Exception("ERROR!  No engine properties provided.");

            if ((EngineProps.RulesEngine == null) && !String.IsNullOrEmpty(EngineProps.RulesMarkupXml))
            {
                var account = new Account(poEngineInitData.EthPassword);

                Nethereum.Web3.Web3 web3 = null;
                if (!String.IsNullOrEmpty(poEngineInitData.Web3HttpUrl))
                    web3 = new Nethereum.Web3.Web3(account, poEngineInitData.Web3HttpUrl);
                else
                    web3 = new Nethereum.Web3.Web3(account);

                if (String.IsNullOrEmpty(poEngineInitData.RulesEngineContractAddress))
                {
                    var WonkaLibABI         = Autogen.WonkaLibrary.WonkaLibraryDeployment.ABI;
                    var WonkaLibPlaceHolder = Autogen.WonkaLibrary.WonkaLibraryDeployment.PLACEHOLDER_KEY;

                    var EngineDeployment = new Autogen.WonkaEngine.WonkaEngineDeploymentClassic();

                    var WonkaLibDeployment = new Autogen.WonkaLibrary.WonkaLibraryDeployment();

                    HexBigInteger nDeployGas = new HexBigInteger(CONST_DEPLOY_ENGINE_CONTRACT_GAS_COST);

                    // Deploy the library contract first
                    var LibraryContractAddress =
                        await WonkaLibDeployment.DeployContractAsync(web3, WonkaLibABI, poEngineInitData.EthSenderAddress, nDeployGas, poEngineInitData.Web3HttpUrl).ConfigureAwait(false);

                    var libraryMapping =
                        new ByteCodeLibrary() { Address = LibraryContractAddress, PlaceholderKey = WonkaLibPlaceHolder };

                    // Link main contract byte code with the library, in preparation for deployment
                    var contractByteCode = Autogen.WonkaEngine.WonkaEngineDeployment.BYTECODE;
                    var libraryMappings  = new ByteCodeLibrary[] { libraryMapping };
                    var libraryLinker    = new ByteCodeLibraryLinker();

                    var contractByteCodeLinked = libraryLinker.LinkByteCode(contractByteCode, libraryMappings);

                    // Deploy linked contract
                    var DeployEngineReceipt
                        = await web3.Eth.DeployContract.SendRequestAndWaitForReceiptAsync(contractByteCodeLinked, poEngineInitData.EthSenderAddress, nDeployGas, null, null, null).ConfigureAwait(false);

                    poEngineInitData.RulesEngineContractAddress = DeployEngineReceipt.ContractAddress;
                }

                if (String.IsNullOrEmpty(poEngineInitData.RegistryContractAddress))
                {
                    var RegistryDeployment = new Autogen.WonkaRegistry.WonkaRegistryDeployment();

                    HexBigInteger nDeployGas = new HexBigInteger(CONST_DEPLOY_DEFAULT_CONTRACT_GAS_COST);

                    poEngineInitData.RegistryContractAddress =
                        await RegistryDeployment.DeployContractAsync(web3, poEngineInitData.RegistryContractABI, poEngineInitData.EthSenderAddress, nDeployGas, poEngineInitData.Web3HttpUrl).ConfigureAwait(false);
                }

                if (String.IsNullOrEmpty(poEngineInitData.StorageContractAddress))
                {
                    var TestContractDeployment = new Autogen.WonkaTestContract.WonkaTestContractDeployment();

                    HexBigInteger nDeployGas = new HexBigInteger(CONST_DEPLOY_DEFAULT_CONTRACT_GAS_COST);

                    poEngineInitData.StorageContractAddress =
                        await TestContractDeployment.DeployContractAsync(web3, poEngineInitData.StorageContractABI, poEngineInitData.EthSenderAddress, nDeployGas, poEngineInitData.Web3HttpUrl).ConfigureAwait(false);
                }
            }

            return bResult;
        }

        /// <summary>
        /// 
        /// This method will deploy all contracts needed to run Wonka on-chain, using all the data provided.
        /// 
        /// <returns>None</returns>
        /// </summary>
        public static async Task<bool> DeployContractsClassicAsync(this WonkaEthEngineInitialization poEngineInitData)
        {
			bool bResult = true;

            var EngineProps = poEngineInitData.Engine;

            if (EngineProps == null)
                throw new Exception("ERROR!  No engine properties provided.");

            if ((EngineProps.RulesEngine == null) && !String.IsNullOrEmpty(EngineProps.RulesMarkupXml))
            {
                var account = new Account(poEngineInitData.EthPassword);

                Nethereum.Web3.Web3 web3 = null;
                if (!String.IsNullOrEmpty(poEngineInitData.Web3HttpUrl))
                    web3 = new Nethereum.Web3.Web3(account, poEngineInitData.Web3HttpUrl);
                else
                    web3 = new Nethereum.Web3.Web3(account);

                if (String.IsNullOrEmpty(poEngineInitData.RulesEngineContractAddress))
                {
                    var EngineDeployment = new Autogen.WonkaEngine.WonkaEngineDeploymentClassic();

                    HexBigInteger nDeployGas = new HexBigInteger(CONST_DEPLOY_ENGINE_CONTRACT_GAS_COST);

                    poEngineInitData.RulesEngineContractAddress =
                        await EngineDeployment.DeployContractAsync(web3, poEngineInitData.RulesEngineABI, poEngineInitData.EthSenderAddress, nDeployGas, poEngineInitData.Web3HttpUrl).ConfigureAwait(false);
                }

                if (String.IsNullOrEmpty(poEngineInitData.RegistryContractAddress))
                {
                    var RegistryDeployment = new Autogen.WonkaRegistry.WonkaRegistryDeployment();

                    HexBigInteger nDeployGas = new HexBigInteger(CONST_DEPLOY_DEFAULT_CONTRACT_GAS_COST);

                    poEngineInitData.RegistryContractAddress =
                        await RegistryDeployment.DeployContractAsync(web3, poEngineInitData.RegistryContractABI, poEngineInitData.EthSenderAddress, nDeployGas, poEngineInitData.Web3HttpUrl).ConfigureAwait(false);
                }

                if (String.IsNullOrEmpty(poEngineInitData.StorageContractAddress))
                {
                    var TestContractDeployment = new Autogen.WonkaTestContract.WonkaTestContractDeployment();

                    HexBigInteger nDeployGas = new HexBigInteger(CONST_DEPLOY_DEFAULT_CONTRACT_GAS_COST);

                    poEngineInitData.StorageContractAddress =
                        await TestContractDeployment.DeployContractAsync(web3, poEngineInitData.StorageContractABI, poEngineInitData.EthSenderAddress, nDeployGas, poEngineInitData.Web3HttpUrl).ConfigureAwait(false);
                }
            }

            return bResult;
        }


        /// <summary>
        /// 
        /// This method will initialize an instance of the Wonka.Net engine, using all the data provided.
        /// 
        /// <returns>None</returns>
        /// </summary>
        public static void InitEngine(this WonkaEthEngineInitialization poEngineInitData, bool pbRequireRetrieveValueMethod = true)
		{
			var EngineProps = poEngineInitData.Engine;

			if (EngineProps == null)
				throw new Exception("ERROR!  No engine properties provided.");

			if ((EngineProps.RulesEngine == null) && !String.IsNullOrEmpty(EngineProps.RulesMarkupXml))
			{
				if (pbRequireRetrieveValueMethod && (EngineProps.DotNetRetrieveMethod == null))
					throw new WonkaEthInitException("ERROR!  Retrieve method not provided for the Wonka.NET engine.", poEngineInitData);

				if (EngineProps.MetadataSource == null)
					throw new WonkaEthInitException("ERROR!  No metadata source has been provided.", poEngineInitData);

				// Using the metadata source, we create an instance of a defined data domain
				WonkaRefEnvironment WonkaRefEnv = WonkaRefEnvironment.CreateInstance(false, EngineProps.MetadataSource);

                // The old version of deployment, pushing out a contract with all methods (i.e., library)
                // bool bDeploySuccess = poEngineInitData.DeployContractsClassicAsync().Result;

                // The new version of deployment, pushing out the contract with a link to a library
                bool bDeploySuccess = poEngineInitData.DeployContractsAsync().Result;

                if (bDeploySuccess)
                {
                    if ((EngineProps.SourceMap == null) || (EngineProps.SourceMap.Count == 0))
                    {
                        EngineProps.SourceMap = new Dictionary<string, WonkaBizSource>();

                        // Here a mapping is created, where each Attribute points to a specific contract and its "accessor" methods
                        // - the class that contains this information (contract, accessors, etc.) is of the WonkaBreSource type
                        foreach (WonkaRefAttr TempAttr in WonkaRefEnv.AttrCache)
                        {
                            WonkaBizSource TempSource =
                                new WonkaBizSource(poEngineInitData.StorageDefaultSourceId,
                                                   poEngineInitData.EthSenderAddress,
                                                   poEngineInitData.EthPassword,
                                                   poEngineInitData.StorageContractAddress,
                                                   poEngineInitData.StorageContractABI,
                                                   poEngineInitData.StorageGetterMethod,
                                                   poEngineInitData.StorageSetterMethod,
                                                   EngineProps.DotNetRetrieveMethod);

                            EngineProps.SourceMap[TempAttr.AttrName] = TempSource;
                        }
                    }

                    EngineProps.RulesEngine =
                        new WonkaBizRulesEngine(new StringBuilder(EngineProps.RulesMarkupXml), EngineProps.SourceMap, EngineProps.MetadataSource);

                    EngineProps.RulesEngine.DefaultSource = poEngineInitData.StorageDefaultSourceId;

                    EngineProps.RulesEngine.SetDefaultStdOps(poEngineInitData.EthPassword, poEngineInitData.Web3HttpUrl);
                }
                else
                    throw new Exception("ERROR!  Deployment of Wonka contracts has failed!");
			}
		}

        /// <summary>
        /// 
        /// This method will initialize an instance of the Wonka.Net engine, using all the data provided.
        /// 
        /// <returns>None</returns>
        /// </summary>
        public static async Task<bool> InitEngineAsync(this WonkaEthEngineInitialization poEngineInitData, bool pbRequireRetrieveValueMethod = true)
        {
			bool bResult = true;

            var EngineProps = poEngineInitData.Engine;

            if (EngineProps == null)
                throw new Exception("ERROR!  No engine properties provided.");

            if ((EngineProps.RulesEngine == null) && !String.IsNullOrEmpty(EngineProps.RulesMarkupXml))
            {
                if (pbRequireRetrieveValueMethod && (EngineProps.DotNetRetrieveMethod == null))
                    throw new WonkaEthInitException("ERROR!  Retrieve method not provided for the Wonka.NET engine.", poEngineInitData);

                if (EngineProps.MetadataSource == null)
                    throw new WonkaEthInitException("ERROR!  No metadata source has been provided.", poEngineInitData);

                // Using the metadata source, we create an instance of a defined data domain
                WonkaRefEnvironment WonkaRefEnv = WonkaRefEnvironment.CreateInstance(false, EngineProps.MetadataSource);

                // The old version of deployment, pushing out a contract with all methods (i.e., library)
                // bool bDeploySuccess = poEngineInitData.DeployContractsClassicAsync().Result;

                // The new version of deployment, pushing out the contract with a link to a library
                bool bDeploySuccess = poEngineInitData.DeployContractsAsync().Result;

                if (bDeploySuccess)
                {
                    if ((EngineProps.SourceMap == null) || (EngineProps.SourceMap.Count == 0))
                    {
                        EngineProps.SourceMap = new Dictionary<string, WonkaBizSource>();

                        // Here a mapping is created, where each Attribute points to a specific contract and its "accessor" methods
                        // - the class that contains this information (contract, accessors, etc.) is of the WonkaBreSource type
                        foreach (WonkaRefAttr TempAttr in WonkaRefEnv.AttrCache)
                        {
                            WonkaBizSource TempSource =
                                new WonkaBizSource(poEngineInitData.StorageDefaultSourceId,
                                                   poEngineInitData.EthSenderAddress,
                                                   poEngineInitData.EthPassword,
                                                   poEngineInitData.StorageContractAddress,
                                                   poEngineInitData.StorageContractABI,
                                                   poEngineInitData.StorageGetterMethod,
                                                   poEngineInitData.StorageSetterMethod,
                                                   EngineProps.DotNetRetrieveMethod);

                            EngineProps.SourceMap[TempAttr.AttrName] = TempSource;
                        }
                    }

                    EngineProps.RulesEngine =
                        new WonkaBizRulesEngine(new StringBuilder(EngineProps.RulesMarkupXml), EngineProps.SourceMap, EngineProps.MetadataSource);

                    EngineProps.RulesEngine.DefaultSource = poEngineInitData.StorageDefaultSourceId;

                    //NOTE: These Ethereum ops will not currently execute correctly within .NET during Async mode, since the Wonka.NET must then also be refactored to execute in Async
                    //EngineProps.RulesEngine.SetDefaultStdOps(poEngineInitData.EthPassword, poEngineInitData.Web3HttpUrl);
                }
                else
                    throw new Exception("ERROR!  Deployment of Wonka contracts has failed!");
            }

            return bResult;
        }

        public static Dictionary<string, WonkaBizSource> InitializeTokenOpMap(this WonkaEthEngineInitialization poEngInitData)
		{
            return WonkaEthCustomRuleTokenExtensions.InitializeTokenOperationsMap(poEngInitData.EthSenderAddress,
				                                                                  poEngInitData.EthPassword,
                                                                                  poEngInitData.ERC20ContractAddress,
                                                                                  poEngInitData.ERC721ContractAddress,
																				  poEngInitData.Web3HttpUrl);
        }

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

            if ((poEngineInitData != null) && (poEngineInitData.Engine != null) && (poEngineInitData.Engine.RulesEngine != null))
            {
                if (poEngineInitData.Engine.RulesEngine.RefEnvHandle != null)
                {
                    bResult =
                        poEngineInitData.Engine.RulesEngine.RefEnvHandle.Serialize(poEngineInitData.EthRuleTreeOwnerAddress,
                                                                                   poEngineInitData.EthPassword,
                                                                                   poEngineInitData.EthSenderAddress,
                                                                                   poEngineInitData.RulesEngineContractAddress,
                                                                                   poEngineInitData.RulesEngineABI,
                                                                                   poEngineInitData.Web3HttpUrl);

                    if (bResult)
                    {
                        bResult =
                            poEngineInitData.Engine.RulesEngine.Serialize(poEngineInitData.EthRuleTreeOwnerAddress,
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

        /// <summary>
        /// 
        /// This method will use Nethereum to call upon (or create) an instance of the Ethgine contract and 
        /// to create a RuleTree that will be owned by the Sender.
        /// 
        /// <returns>Indicates whether or not the RuleTree was created to the blockchain</returns>
        /// </summary>
        public static async Task<bool> SerializeAsync(this WonkaEthEngineInitialization poEngineInitData)
        {
            bool bResult = false;

            if ((poEngineInitData != null) && (poEngineInitData.Engine != null) && (poEngineInitData.Engine.RulesEngine != null))
            {
                if (poEngineInitData.Engine.RulesEngine.RefEnvHandle != null)
                {
                    bResult =
                        await poEngineInitData.Engine.RulesEngine.RefEnvHandle.SerializeAsync(poEngineInitData.EthRuleTreeOwnerAddress,
                                                                                              poEngineInitData.EthPassword,
                                                                                              poEngineInitData.EthSenderAddress,
                                                                                              poEngineInitData.RulesEngineContractAddress,
                                                                                              poEngineInitData.RulesEngineABI,
                                                                                              poEngineInitData.Web3HttpUrl).ConfigureAwait(false);

                    if (bResult)
                    {
                        bResult =
                            await poEngineInitData.Engine.RulesEngine.SerializeAsync(poEngineInitData.EthRuleTreeOwnerAddress,
                                                                                     poEngineInitData.EthPassword,
                                                                                     poEngineInitData.EthSenderAddress,
                                                                                     poEngineInitData.RulesEngineContractAddress,
                                                                                     poEngineInitData.RulesEngineABI,
                                                                                     poEngineInitData.RulesEngineContractAddress,
                                                                                     poEngineInitData.Web3HttpUrl).ConfigureAwait(false);
                    }
                }
            }

            return bResult;
        }
    }
}
