using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Nethereum.Geth;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Web3.Accounts.Managed;
using Nethereum.Geth.RPC.Miner;
using Nethereum.RPC.Eth.DTOs;

using Xunit;

using WonkaBre;
using WonkaBre.RuleTree;
using WonkaPrd;
using WonkaRef;

using WonkaEth.Extensions;
using WonkaEth.Validation;

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
    /// Finally (and most importantly), this test will showcase the Orchestration functionality, which allows the user 
    /// to declare each Attribute as originating from a different contract.  (In this case, they all point to the same
    /// contract).  In this way, during the application of a RuleTree, the rules engine (i.e., WonkaEngine contract) 
    /// on the blockchain can interact with other contracts in order to obtain (and set) Attribute values.
    ///
    /// NOTE: This test does execute the Ethereum implementation of the rules engine.
    ///
    /// NOTE: In order to use the Orchestration functionality, the current design has requirements that certain functions
    ///       (and their respective signatures) must be placed on the contracts, much like implementing the functions of 
    ///       an interface in C#.
    ///
    /// </summary>
    public class WonkaSimpleOrchestrationTest
    {
        private const int CONST_CONTRACT_ATTR_NUM_ON_START = 3;

        public const string CONST_CONTRACT_FUNCTION_EXEC_RPT     = "executeWithReport";
        public const string CONST_CONTRACT_FUNCTION_GET_LAST_RPT = "getLastRuleReport";
        public const string CONST_CONTRACT_FUNCTION_HAS_RT       = "hasRuleTree";

        private readonly string msRulesContents;
        private readonly string msAbiWonka;
        private readonly string msByteCodeWonka;
        private readonly string msAbiOrchTest;
        private readonly string msByteCodeOrchTest;

        private IMetadataRetrievable moMetadataSource = null;

        private string msSenderAddress   = "";
        private string msPassword        = "";
        private string msContractAddress = "";

        List<WonkaRefAttr> moTargetAttrList = null;

        public WonkaSimpleOrchestrationTest(string psSenderAddress, string psPassword, string psContractAddress, bool pbSerializeMetadataToBlockchain = true)
        {                       
            msSenderAddress   = psSenderAddress;
            msPassword        = psPassword;
            msContractAddress = psContractAddress; 

            // Create an instance of the class that will provide us with PmdRefAttributes (i.e., the data domain)
            // that define our data record            
            moMetadataSource = new WonkaMetadataTestSource();

            var TmpAssembly = Assembly.GetExecutingAssembly();

            // Read the ABI of the Ethereum contract for the Wonka rules engine
            using (var AbiReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.WonkaEngine.abi")))
            {
                msAbiWonka = AbiReader.ReadToEnd();
            }

            // Read the bytecodes of the Ethereum contract for the Wonka rules engine
            using (var ByteCodeReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.WonkaEngine.bin")))
            {
                msByteCodeWonka = ByteCodeReader.ReadToEnd();
            }

            // Read the ABI of the Ethereum contract that will demonstrate our Orchestration functionality by getting/setting Attribute values
            using (var AbiReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.OrchTest.abi")))
            {
                msAbiOrchTest = AbiReader.ReadToEnd();
            }

            // Read the bytecodes of the Ethereum contract that will hold our data record
            using (var ByteCodeReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.OrchTest.bin")))
            {
                msByteCodeOrchTest = ByteCodeReader.ReadToEnd();
            }

            // Read the XML markup that lists the business rules
            using (var RulesReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.SimpleAccountCheck.xml")))
            {
                msRulesContents = RulesReader.ReadToEnd();
            }

            // Using the metadata source, we create an instance of a defined data domain
            WonkaRefEnvironment WonkaRefEnv =
                WonkaRefEnvironment.CreateInstance(false, moMetadataSource);

            WonkaRefAttr        AccountIDAttr       = WonkaRefEnv.GetAttributeByAttrName("BankAccountID");
            WonkaRefAttr        AccountNameAttr     = WonkaRefEnv.GetAttributeByAttrName("BankAccountName");
            WonkaRefAttr        AccountStsAttr      = WonkaRefEnv.GetAttributeByAttrName("AccountStatus");
            WonkaRefAttr        AccountCurrValAttr  = WonkaRefEnv.GetAttributeByAttrName("AccountCurrValue");
            WonkaRefAttr        AccountTypeAttr     = WonkaRefEnv.GetAttributeByAttrName("AccountType");
            WonkaRefAttr        AccountCurrencyAttr = WonkaRefEnv.GetAttributeByAttrName("AccountCurrency");
            WonkaRefAttr        RvwFlagAttr         = WonkaRefEnv.GetAttributeByAttrName("AuditReviewFlag");

            // We create a target list of the Attributes of the old (i.e., existing) record that currently exists on the blockchain
            // and which we want to pull back during the engine's execution
            moTargetAttrList = new List<WonkaRefAttr>();

            moTargetAttrList =
                new List<WonkaRefAttr>() { AccountIDAttr, AccountNameAttr, AccountStsAttr, AccountCurrValAttr, AccountTypeAttr, AccountCurrencyAttr, RvwFlagAttr };

            // Serialize the data domain to the blockchain
            if (pbSerializeMetadataToBlockchain)
            {
                SerializeMetadataToBlockchain();
            }
        }

        public void Execute(string psOrchestrationTestAddress = null, bool pbValidateWithinTransaction = false)
        {
            WonkaRefEnvironment RefEnv = WonkaRefEnvironment.GetInstance();

            Dictionary<string, WonkaBre.RuleTree.WonkaBreSource> SourceMap =
                new Dictionary<string, WonkaBre.RuleTree.WonkaBreSource>();

            string sDefaultSource      = "S";
            string sContractSourceId   = sDefaultSource;
            string sContractAddress    = "";
            string sContractAbi        = "";
            string sOrchGetterMethod   = "";
            string sOrchSetterMethod   = "";

            // If a 'psOrchestrationTestAddress' value has been provided, it indicates that the user wishes
            // to use Orchestration
            if (!String.IsNullOrEmpty(psOrchestrationTestAddress))
            {
                // Here we set the values for Orchestration (like the target contract) and the implemented 
                // methods that have the expected function signatures for getting/setting Attribute values
                sContractAddress  = psOrchestrationTestAddress;
                sContractAbi      = msAbiOrchTest;
                sOrchGetterMethod = "getAttrValueBytes32";
                sOrchSetterMethod = "setAttrValueBytes32";
            }
            else 
            {
                sContractAddress  = msContractAddress;
                sContractAbi      = msAbiWonka;
                sOrchGetterMethod = "getValueOnRecord";
                sOrchSetterMethod = "";
            }

            // Here a mapping is created, where each Attribute points to a specific contract and its "accessor" methods
            // - the class that contains this information (contract, accessors, etc.) is of the WonkaBreSource type
            foreach (WonkaRefAttr TempAttr in moTargetAttrList)
            {                
                WonkaBreSource TempSource =
                    new WonkaBreSource(sContractSourceId, msSenderAddress, msPassword, sContractAddress, sContractAbi, sOrchGetterMethod, sOrchSetterMethod, RetrieveValueMethod);

                SourceMap[TempAttr.AttrName] = TempSource;
            }

            // Creating an instance of the rules engine using our rules and the metadata
            WonkaBreRulesEngine RulesEngine = null;

            if (psOrchestrationTestAddress == null)
                RulesEngine = new WonkaBreRulesEngine(new StringBuilder(msRulesContents), moMetadataSource);
            else
            {
                RulesEngine = new WonkaBreRulesEngine(new StringBuilder(msRulesContents), SourceMap, moMetadataSource);
                RulesEngine.DefaultSource = sDefaultSource;
            }

            RulesEngine.SetDefaultStdOps(msPassword);

            // The contract dictates that the RuleTree (and its other info, like the Source mapping) is serialized 
            // to the blockchain before interacting with it
            SerializeRulesEngineToBlockchain(RulesEngine);

            WonkaRefAttr AccountStsAttr = RefEnv.GetAttributeByAttrName("AccountStatus");
            WonkaRefAttr RvwFlagAttr    = RefEnv.GetAttributeByAttrName("AuditReviewFlag");

            // Gets a predefined data record that will be our analog for new data coming into the system
            // We are only using this record to test the .NET implementation
            WonkaProduct NewProduct = GetNewProduct();

            string sStatusValueBefore = GetAttributeValue(NewProduct, AccountStsAttr);
            string sFlagValueBefore   = GetAttributeValue(NewProduct, RvwFlagAttr);

            // SerializeProductToBlockchain(NewProduct);

            // Validate that the .NET implementation and the rules markup are both working properly
            WonkaBre.Reporting.WonkaBreRuleTreeReport Report = RulesEngine.Validate(NewProduct);

            string sStatusValueAfter = GetAttributeValue(NewProduct, AccountStsAttr);
            string sFlagValueAfter   = GetAttributeValue(NewProduct, RvwFlagAttr);

            if (Report.OverallRuleTreeResult == ERR_CD.CD_SUCCESS)
            {
                // NOTE: This should only be used for further testing
                // Serialize(NewProduct);
            }
            else if (Report.GetRuleSetFailureCount() > 0)
            {
                System.Console.WriteLine(".NET Engine says \"Oh heavens to Betsy! Something bad happened!\"");
            }
            else
            {
                System.Console.WriteLine(".NET Engine says \"What in the world is happening?\"");
            }

            // If a 'psOrchestrationTestAddress' value has been provided, it indicates that the user wishes
            // to use Orchestration
            if (!String.IsNullOrEmpty(psOrchestrationTestAddress))
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
                var BlockchainReport = ExecuteWithReport(RulesEngine, pbValidateWithinTransaction, SourceMap[RvwFlagAttr.AttrName]);

                if (BlockchainReport.NumberOfRuleFailures == 0)
                {
                    // Indication of a success
                }
                else if (BlockchainReport.NumberOfRuleFailures > 0)
                {
                    throw new Exception("Oh heavens to Betsy! Something bad happened!");
                }
                else
                {
                    throw new Exception("Seriously, what in the world is happening?!");
                }
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

        public WonkaProduct GetOldProduct(Dictionary<string,string> poProductKeys)
        {
            WonkaProduct OldProduct = new WonkaProduct();

            Dictionary<WonkaRefAttr, string> EthereumRecord = GetOldRecordViaEthereum(moTargetAttrList, poProductKeys);

            foreach (WonkaRefAttr TempAttr in EthereumRecord.Keys)
            {
                OldProduct.SetAttribute(TempAttr, EthereumRecord[TempAttr]);
            }

            return OldProduct;            
        }

        public Dictionary<WonkaRefAttr, string> GetOldRecordViaEthereum(List<WonkaRefAttr> poTargetAttrList, Dictionary<string, string> poProductKeys)
        {
            Dictionary<WonkaRefAttr, string> EthereumRecord = new Dictionary<WonkaRefAttr, string>();

            string sSenderAddress = msSenderAddress;

            var account = new Account(msPassword);

            var web3 = new Nethereum.Web3.Web3(account);

            var contractAddress = msContractAddress;
             
            var contract = web3.Eth.GetContract(msAbiWonka, contractAddress);

            var getAttrNumFunction     = contract.GetFunction("getNumberOfAttributes");
            var getRecordValueFunction = contract.GetFunction("getValueOnRecord");

            var attrNum = getAttrNumFunction.CallAsync<uint>().Result;

            foreach (WonkaRefAttr TempAttr in poTargetAttrList)
            {
                var result = getRecordValueFunction.CallAsync<string>(sSenderAddress, TempAttr.AttrName).Result;
                EthereumRecord[TempAttr] = result;
            }

            return EthereumRecord;
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

        public string GetAttributeValue(WonkaProduct poTargetProduct, WonkaRefAttr poTargetAttr)
        {
            if (poTargetProduct.GetProductGroup(poTargetAttr.GroupId).GetRowCount() <= 0)
                throw new Exception("ERROR!  Provided incoming product has empty group.");

            string sAttrValue = "";
            if (poTargetProduct.GetProductGroup(poTargetAttr.GroupId)[0].ContainsKey(poTargetAttr.AttrId))
                sAttrValue = poTargetProduct.GetProductGroup(poTargetAttr.GroupId)[0][poTargetAttr.AttrId];

            if (String.IsNullOrEmpty(sAttrValue))
                throw new Exception("ERROR!  Provided incoming product has no value for needed key(" + poTargetAttr.AttrName + ").");

            return sAttrValue;
        }

        public Nethereum.Contracts.Contract GetContract()
        {
            var account = new Account(msPassword);

            var web3 = new Nethereum.Web3.Web3(account);

            var contractAddress = msContractAddress;

            var contract = web3.Eth.GetContract(msAbiWonka, contractAddress);

            return contract;
        }

        public Nethereum.Contracts.Contract GetContract(WonkaBre.RuleTree.WonkaBreSource TargetSource)
        {
            var account  = new Account(TargetSource.Password);
            var web3     = new Nethereum.Web3.Web3(account);
            var contract = web3.Eth.GetContract(TargetSource.ContractABI, TargetSource.ContractAddress);

            return contract;
        }

        public string RetrieveValueMethod(WonkaBre.RuleTree.WonkaBreSource poTargetSource, string psAttrName)
        {
            var contract = GetContract(poTargetSource);

            var getRecordValueFunction = contract.GetFunction(poTargetSource.MethodName);

            // var result = getRecordValueFunction.CallAsync<string>(poTargetSource.SenderAddress, psAttrName).Result;
            var result = getRecordValueFunction.CallAsync<string>(psAttrName).Result;

            return result;
        }

        private void SerializeMetadataToBlockchain()
        {
            uint nAttrNum = 3;

            string sSenderAddress = msSenderAddress;

            var contract = GetContract();

            var getAttrNumFunction = contract.GetFunction("getNumberOfAttributes");
            var addAttrFunction    = contract.GetFunction("addAttribute");

            nAttrNum = getAttrNumFunction.CallAsync<uint>().Result;

            if (nAttrNum <= CONST_CONTRACT_ATTR_NUM_ON_START)
            {
                foreach (WonkaRefAttr TempAttr in moTargetAttrList)
                {
                    var sAttrName = "";

                    if (TempAttr.AttrName.Length > 32)
                        sAttrName = TempAttr.AttrName.Trim().Replace(" ", "").Substring(0, 31);
                    else
                        sAttrName = TempAttr.AttrName.Trim().Replace(" ", "");

                    uint MaxLen    = (uint) TempAttr.MaxLength;
                    uint MaxNumVal = 999999; // TempAttr.MaxValue;
                    string DefVal  = !String.IsNullOrEmpty(TempAttr.DefaultValue) ? TempAttr.DefaultValue : "";
                    bool IsString  = !TempAttr.IsNumeric;
                    bool IsNumeric = TempAttr.IsNumeric;

                    // NOTE: Caused exception to be thrown
                    // var gas = addAttrFunction.EstimateGasAsync("SomeAttr", 0, 0, "SomeVal", false, false).Result;
                    var gas = new Nethereum.Hex.HexTypes.HexBigInteger(1000000);

                    var receiptAddAttribute =
                        addAttrFunction.SendTransactionAsync(sSenderAddress, gas, null, sAttrName, MaxLen, MaxNumVal, DefVal, IsString, IsNumeric).Result;
                }
            }
        }

        public void SerializeProductToBlockchain(WonkaProduct poTargetProduct)
        {
            string sSenderAddress = msSenderAddress;

            var contract = GetContract();

            var setValueOnRecordFunction = contract.GetFunction("setValueOnRecord");

            // Exception thrown
            // var gas = setValueOnRecordFunction.EstimateGasAsync()sSenderAddress, "SomeAttr", "SomeValue").Result;
            var gas = new Nethereum.Hex.HexTypes.HexBigInteger(1000000);

            foreach (WonkaRefAttr TempAttr in moTargetAttrList)
            {
                string sAttrValue = GetAttributeValue(poTargetProduct, TempAttr);

                /*
                 * base fee exceeds gas limit?
                var receiptSetValueOnRecord = 
                    setValueOnRecordFunction.SendTransactionAndWaitForReceiptAsync(sSenderAddress, null, sSenderAddress, TempAttr.AttrName, sAttrValue).Result;
                 */

                var receiptSetValueOnRecord =
                    setValueOnRecordFunction.SendTransactionAsync(sSenderAddress, gas, null, sSenderAddress, TempAttr.AttrName, sAttrValue).Result;
            }
        }

        private void SerializeRulesEngineToBlockchain(WonkaBreRulesEngine poEngine)
        {
            var contract = GetContract();

            var hasRuleTreeFunction = contract.GetFunction(CONST_CONTRACT_FUNCTION_HAS_RT);

            var gas = hasRuleTreeFunction.EstimateGasAsync(this.msSenderAddress).Result;

            bool bTreeAlreadyExists =
                hasRuleTreeFunction.CallAsync<bool>(this.msSenderAddress, gas, null, this.msSenderAddress).Result;

            if (!bTreeAlreadyExists)
                poEngine.Serialize(msSenderAddress, msPassword, msSenderAddress, msContractAddress, msAbiWonka);
        }

    }
}
