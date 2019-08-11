using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Nethereum.Web3.Accounts;

using Xunit;

using WonkaBre;
using WonkaBre.RuleTree;
using WonkaPrd;
using WonkaRef;

using WonkaEth.Extensions;

namespace WonkaSystem.TestHarness
{
    /// <summary>
    /// 
    /// This test will create an instance of the .NET implementation of the rules engine and initialize a 
    /// RuleTree with the rules mentioned in the file 'SimpleAccountCheck.xml'.  It will then populate a 
    /// record with test data, serialize the data to the Ethereum blockchain, and then also serialize the 
    /// RuleTree to the blockchain.  It will also execute the rules engine on the Ethereum blockchain 
    /// and examine the report that is returned.
    /// 
    /// Finally (and most importantly), this test will showcase the deployment functionality, orchestration, and usage
    /// of the online test chain.
    ///
    /// NOTE: In order to use the Orchestration functionality, the current design has requirements that certain functions
    ///       (and their respective signatures) must be placed on the contracts, much like implementing the functions of 
    ///       an interface in C#.
    ///
    /// </summary>
    public class WonkaNoviceOnlineChainTest
    {
        public const string CONST_ONLINE_TEST_CHAIN_URL = "http://testchain.nethereum.com:8545";

        public const string CONST_CONTRACT_FUNCTION_EXEC_RPT     = "executeWithReport";
        public const string CONST_CONTRACT_FUNCTION_GET_LAST_RPT = "getLastRuleReport";

        private readonly string msRulesContents;
        private readonly string msAbiWonka;
        private readonly string msByteCodeWonka;
        private readonly string msAbiRegistry;
        private readonly string msByteCodeRegistry;
        private readonly string msAbiOrchTest;
        private readonly string msByteCodeOrchTest;

        private IMetadataRetrievable moMetadataSource = null;

        private string msSenderAddress           = "";
        private string msPassword                = "";
        private string msEngineContractAddress   = "";
        private string msRegistryContractAddress = "";
        private string msTestContractAddress     = "";

		WonkaEth.Init.WonkaEthEngineInitialization moEthEngineInit = null;

		public WonkaNoviceOnlineChainTest(string psContractAddress, bool pbInitChainEnv = true, bool pbRetrieveMarkupFromIpfs = false)
        {                       
            msSenderAddress = "0x12890D2cce102216644c59daE5baed380d84830c";
            msPassword      = "0xb5b1870957d373ef0eeffecc6e4812c0fd08f554b37b233526acc331bf1544f7";

            msAbiWonka         = WonkaEth.Autogen.WonkaEngine.WonkaEngineDeployment.ABI;
            msByteCodeWonka    = WonkaEth.Autogen.WonkaEngine.WonkaEngineDeployment.BYTECODE;
            msAbiRegistry      = WonkaEth.Autogen.WonkaRegistry.WonkaRegistryDeployment.ABI;
            msByteCodeRegistry = WonkaEth.Autogen.WonkaRegistry.WonkaRegistryDeployment.BYTECODE;
            msAbiOrchTest      = WonkaEth.Autogen.WonkaTestContract.WonkaTestContractDeployment.ABI;
            msByteCodeOrchTest = WonkaEth.Autogen.WonkaTestContract.WonkaTestContractDeployment.BYTECODE;

            // Create an instance of the class that will provide us with PmdRefAttributes (i.e., the data domain)
            // that define our data record            
            moMetadataSource = new WonkaMetadataTestSource();            
            WonkaRefEnvironment.CreateInstance(false, moMetadataSource);
            
            var TmpAssembly = Assembly.GetExecutingAssembly();

			// NOTE: As a reminder, you must have a IPFS daemon configured and running (perhaps on your machine)
			// in order for the Ipfs.Api to work successfully
			if (pbRetrieveMarkupFromIpfs)
			{
				var IpfsEnv = WonkaIpfs.WonkaIpfsEnvironment.CreateInstance();

				IpfsEnv.Test();

				msRulesContents = IpfsEnv.GetFile("QmQtQNKMTUoypYLvRj5kvUvSXmoPXP4LWbAD251rJSambd");
            }
			else
			{
				// Read the XML markup that lists the business rules
				using (var RulesReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.SimpleAccountCheck.xml")))
				{
					msRulesContents = RulesReader.ReadToEnd();
				}
			}

			if (psContractAddress == null)
            {
                msEngineContractAddress = DeployWonka();
            }
            else
            {
                if (psContractAddress == "")
                    msEngineContractAddress = "0xfB419DEA1f28283edAD89103fc1f1272f7573E6A";
                else
                    msEngineContractAddress = psContractAddress;

                msRegistryContractAddress = "0x7E618a3948F6a5D2EA6b92D8Ce6723a468540CaA";
                msTestContractAddress     = "0x4092bc250ef6c384804af2f871Af9c679b672d0B";
            }

            Init(pbInitChainEnv);
        }

        public string DeployWonka()
        {
            var web3               = GetWeb3();
            var EngineDeployment   = new WonkaEth.Autogen.WonkaEngine.WonkaEngineDeployment();
            var RegistryDeployment = new WonkaEth.Autogen.WonkaRegistry.WonkaRegistryDeployment();
            var TestCntDeployment  = new WonkaEth.Autogen.WonkaTestContract.WonkaTestContractDeployment();

            msEngineContractAddress   = EngineDeployment.DeployContract(web3, msAbiWonka, msSenderAddress, CONST_ONLINE_TEST_CHAIN_URL);
            msRegistryContractAddress = RegistryDeployment.DeployContract(web3, msAbiRegistry, msSenderAddress, CONST_ONLINE_TEST_CHAIN_URL);
            msTestContractAddress     = TestCntDeployment.DeployContract(web3, msAbiOrchTest, msSenderAddress, CONST_ONLINE_TEST_CHAIN_URL);

            return msEngineContractAddress;
        }

        public void Execute(bool pbValidateTransaction = false)
        {
            var RefEnv      = WonkaRefEnvironment.GetInstance();
			var RulesEngine = moEthEngineInit.Engine.RulesEngine;
			var SourceMap   = moEthEngineInit.Engine.SourceMap;

            WonkaRefAttr AccountStsAttr = RefEnv.GetAttributeByAttrName("AccountStatus");
            WonkaRefAttr RvwFlagAttr    = RefEnv.GetAttributeByAttrName("AuditReviewFlag");

            // Gets a predefined data record that will be our analog for new data coming into the system
            // We are only using this record to test the .NET implementation
            WonkaProduct NewProduct = GetNewProduct();

            string sStatusValueBefore = NewProduct.GetAttributeValue(AccountStsAttr);
            string sFlagValueBefore   = NewProduct.GetAttributeValue(RvwFlagAttr);

            // SerializeProductToBlockchain(NewProduct);

            // Validate that the .NET implementation and the rules markup are both working properly
            WonkaBre.Reporting.WonkaBreRuleTreeReport Report = RulesEngine.Validate(NewProduct);

            string sStatusValueAfter = NewProduct.GetAttributeValue(AccountStsAttr);
            string sFlagValueAfter   = NewProduct.GetAttributeValue(RvwFlagAttr);

            if (Report.OverallRuleTreeResult == ERR_CD.CD_SUCCESS)
            {
                // NOTE: This should only be used for further testing
                // Serialize(NewProduct);
            }
            else if (Report.GetRuleSetFailureCount() > 0)
                System.Console.WriteLine(".NET Engine says \"Oh heavens to Betsy! Something bad happened!\"");
            else
                System.Console.WriteLine(".NET Engine says \"What in the world is happening?\"");

            // If a 'psOrchestrationTestAddress' value has been provided, it indicates that the user wishes
            // to use Orchestration
            if (!String.IsNullOrEmpty(msTestContractAddress))
            {
                /**
                 ** Now execute the rules engine on the blockchain.
                 **
                 ** NOTE: Based on the value of the argument 'pbValidateWithinTransaction', we will act accordingly - 
                 **       If set to 'true', we issue a call() when we execute the rules engine, since we are only
                 **       looking to validate here.  However, if the value if 'false', we issue a sendTransaction() 
                 **       so that we can attempts to set values (i.e., change the blockchain) will take effect.
                 **       In that case, we might want to pull back the record afterwards with a subsequent function
                 **       call, in order to examine the record here.
                 **       
                 **/
                var BlockchainReport = ExecuteWithReport(RulesEngine, pbValidateTransaction, SourceMap[RvwFlagAttr.AttrName]);

                if (BlockchainReport.NumberOfRuleFailures == 0)
                {
                    // Indication of a success
                }
                else if (BlockchainReport.NumberOfRuleFailures > 0)
                    throw new Exception("Oh heavens to Betsy! Something bad happened!");
                else
                    throw new Exception("Seriously, what in the world is happening?!");
            }
        }

        public RuleTreeReport ExecuteWithReport(WonkaBreRulesEngine poRulesEngine, bool pbValidateWithinTransaction, WonkaBreSource poFlagSource)
        {
            WonkaRefEnvironment RefEnv = WonkaRefEnvironment.GetInstance();

            WonkaRefAttr CurrValueAttr  = RefEnv.GetAttributeByAttrName("AccountCurrValue");
            WonkaRefAttr ReviewFlagAttr = RefEnv.GetAttributeByAttrName("AuditReviewFlag");

            Dictionary<string, string> PrdKeys = new Dictionary<string, string>();

            var contract = GetContract();

            var executeWithReportFunction = contract.GetFunction(CONST_CONTRACT_FUNCTION_EXEC_RPT);

            RuleTreeReport ruleTreeReport = null;

            if (pbValidateWithinTransaction)
            {
                var executeGetLastReportFunction = contract.GetFunction(CONST_CONTRACT_FUNCTION_GET_LAST_RPT);

                // NOTE: Caused exception to be thrown
                // var gas = executeWithReportFunction.EstimateGasAsync(msSenderAddress).Result;
                var gas = new Nethereum.Hex.HexTypes.HexBigInteger(1000000);

                WonkaProduct OrchContractCurrValues = poRulesEngine.AssembleCurrentProduct(new Dictionary<string, string>());

                string sFlagBeforeOrchestrationAssignment  = RetrieveValueMethod(poFlagSource, ReviewFlagAttr.AttrName);
                string sValueBeforeOrchestrationAssignment = RetrieveValueMethod(poFlagSource, CurrValueAttr.AttrName);

                var receiptAddAttribute = executeWithReportFunction.SendTransactionAsync(msSenderAddress, gas, null, msSenderAddress).Result;

                string sFlagAfterOrchestrationAssignment  = RetrieveValueMethod(poFlagSource, ReviewFlagAttr.AttrName);
                string sValueAfterOrchestrationAssignment = RetrieveValueMethod(poFlagSource, CurrValueAttr.AttrName);

                ruleTreeReport = executeGetLastReportFunction.CallDeserializingToObjectAsync<RuleTreeReport>().Result;
            }
            else 
                ruleTreeReport = executeWithReportFunction.CallDeserializingToObjectAsync<RuleTreeReport>(msSenderAddress).Result;

            return ruleTreeReport;
        }

        public WonkaProduct GetNewProduct()
        {
            WonkaRefEnvironment WkaRefEnv           = WonkaRefEnvironment.GetInstance();
            WonkaRefAttr        AccountIDAttr       = WkaRefEnv.GetAttributeByAttrName("BankAccountID");
            WonkaRefAttr        AccountNameAttr     = WkaRefEnv.GetAttributeByAttrName("BankAccountName");
            WonkaRefAttr        AccountStsAttr      = WkaRefEnv.GetAttributeByAttrName("AccountStatus");
            WonkaRefAttr        AccountCurrValAttr  = WkaRefEnv.GetAttributeByAttrName("AccountCurrValue");
            WonkaRefAttr        AccountTypeAttr     = WkaRefEnv.GetAttributeByAttrName("AccountType");
            WonkaRefAttr        AccountCurrencyAttr = WkaRefEnv.GetAttributeByAttrName("AccountCurrency");
            WonkaRefAttr        ReviewFlagAttr      = WkaRefEnv.GetAttributeByAttrName("AuditReviewFlag");

            WonkaProduct NewProduct = new WonkaProduct();

            NewProduct.SetAttribute(AccountIDAttr,       "1234567890");
            NewProduct.SetAttribute(AccountNameAttr,     "JohnSmithFirstCheckingAccount");
            NewProduct.SetAttribute(AccountStsAttr,      "ACT");
            NewProduct.SetAttribute(AccountCurrValAttr,  "101.00");
            NewProduct.SetAttribute(AccountCurrencyAttr, "USD");
            NewProduct.SetAttribute(ReviewFlagAttr,      "N");
            NewProduct.SetAttribute(AccountTypeAttr,     "Checking");
            // NewProduct.SetAttribute(AccountTypeAttr,     "CompletelyBogusTypeThatWillCauseAnError");

            return NewProduct;
        }
        
        public Nethereum.Contracts.Contract GetContract()
        {
            var web3 = GetWeb3();

            var contractAddress = msEngineContractAddress;

            var contract = web3.Eth.GetContract(msAbiWonka, contractAddress);

            return contract;
        }

        public Nethereum.Contracts.Contract GetContract(WonkaBre.RuleTree.WonkaBreSource TargetSource)
        {
            var web3     = GetWeb3();
            var contract = web3.Eth.GetContract(TargetSource.ContractABI, TargetSource.ContractAddress);

            return contract;
        }

        public Nethereum.Web3.Web3 GetWeb3()
        {
            var account = new Account(msPassword);

            var web3 = new Nethereum.Web3.Web3(account, CONST_ONLINE_TEST_CHAIN_URL);

            return web3;
        }

        private void Init(bool pbInitChainEnv)
        {
			string sDefaultSource = "S";

			moEthEngineInit = new WonkaEth.Init.WonkaEthEngineInitialization();

			// EthEngineInit.Engine.RulesEngine         = moRulesEngine;
			moEthEngineInit.Engine.MetadataSource       = moMetadataSource;
			moEthEngineInit.Engine.RulesMarkupXml       = msRulesContents;
			moEthEngineInit.Engine.DotNetRetrieveMethod = RetrieveValueMethod;
			moEthEngineInit.EthSenderAddress            = moEthEngineInit.EthRuleTreeOwnerAddress = msSenderAddress;
			moEthEngineInit.EthPassword                 = msPassword;
			moEthEngineInit.Web3HttpUrl                 = CONST_ONLINE_TEST_CHAIN_URL;
			moEthEngineInit.RulesEngineContractAddress  = msEngineContractAddress;
			moEthEngineInit.RegistryContractAddress     = msRegistryContractAddress;
			moEthEngineInit.StorageContractAddress      = msTestContractAddress;
			moEthEngineInit.StorageDefaultSourceId      = sDefaultSource;
			moEthEngineInit.StorageContractABI          = msAbiOrchTest;
			moEthEngineInit.StorageGetterMethod         = "getAttrValueBytes32";
			moEthEngineInit.StorageSetterMethod         = "setAttrValueBytes32";
			moEthEngineInit.UsingStorageContract        = true;
			moEthEngineInit.UsingTrxStateContract       = false;

			// NOTE: Optional here
			// EthEngineInit.RegistryContractABI        = msAbiRegistry;
			// EthEngineInit.RulesEngineABI             = msAbiWonka;
			// EthEngineInit.TestContractABI            = msAbiOrchTest;

			moEthEngineInit.InitEngine();

			// Serialize the data domain to the blockchain
			if (pbInitChainEnv)
            {
				moEthEngineInit.Serialize();
            }
        }

        public string RetrieveValueMethod(WonkaBre.RuleTree.WonkaBreSource poTargetSource, string psAttrName)
        {
            var contract = GetContract(poTargetSource);

            var getRecordValueFunction = contract.GetFunction(poTargetSource.MethodName);

            var result = getRecordValueFunction.CallAsync<string>(psAttrName).Result;

            return result;
        }

    }
}
