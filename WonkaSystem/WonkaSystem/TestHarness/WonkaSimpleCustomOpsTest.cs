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

using Wonka.BizRulesEngine;
using Wonka.BizRulesEngine.Readers;
using Wonka.BizRulesEngine.RuleTree;
using WonkaPrd;
using WonkaRef;

using Wonka.Eth.Extensions;
using Wonka.Eth.Validation;

namespace WonkaSystem.TestHarness
{
    /// <summary>
    /// 
    /// This test will create an instance of the .NET implementation of the rules engine and initialize a 
    /// RuleTree with the rules mentioned in the file 'VATCalculationExample.xml'.  It will then populate a 
    /// record with test data, serialize the data to the Ethereum blockchain, and then also serialize the 
    /// RuleTree to the blockchain.  It will also execute the rules engine on the Ethereum blockchain 
    /// and examine the report that is returned.
    /// 
    /// Finally (and most importantly), this test will showcase the Custom Operator (i.e., CU) functionality, which allows the user 
    /// to declare a 'operator' within the rules markup.  The functionality for this operator comes from either a .NET function
    /// or a Solidity function, depending on where the RuleTree is being executed.  In this way, during the application 
    /// of a RuleTree, the rules engine (i.e., WonkaEngine contract) on the blockchain can interact with other contracts 
    /// in order to do something more complex than simple arithmetic or string operations. In the rules markup of the test below,
    /// the CU used is "INVOKE_VAT_LOOKUP", which is a contract method that accepts multiple arguments and then returns a String.
    ///
    /// NOTE: Like some other tests, the Rules Engine must be deployed by a Solidity script before running this test.
    ///
    /// NOTE: This test does execute the Ethereum implementation of the rules engine.  It also uses the Orchestration functionality.
    ///
    /// NOTE: In order to use the Custom Operator functionality, the current design has requirements that certain functions
    ///       (and their respective signatures) must be placed on the contracts, much like implementing the functions of 
    ///       an interface in C#.
    ///
    /// </summary>
    public class WonkaSimpleCustomOpsTest
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

        public WonkaSimpleCustomOpsTest(string psSenderAddress, string psPassword, string psContractAddress, bool pbSerializeMetadataToBlockchain = true)
        {                       
            msSenderAddress   = psSenderAddress;
            msPassword        = psPassword;
            msContractAddress = psContractAddress; 

            // Create an instance of the class that will provide us with PmdRefAttributes (i.e., the data domain)
            // that define our data record            
            moMetadataSource = new WonkaMetadataVATSource();

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

            // Read the ABI of the Ethereum contract that will demonstrate both our Custom Operator functionality and our Orchestration functionality
            using (var AbiReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.OrchTest.abi")))
            {
                msAbiOrchTest = AbiReader.ReadToEnd();
            }

            // Read the bytecodes of the Ethereum contract that will hold our data record and provide Custom Operator functionality
            using (var ByteCodeReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.OrchTest.bin")))
            {
                msByteCodeOrchTest = ByteCodeReader.ReadToEnd();
            }

            // Read the XML markup that lists the business rules
            using (var RulesReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.VATCalculationExample.xml")))
            {
                msRulesContents = RulesReader.ReadToEnd();
            }

            // Using the metadata source, we create an instance of a defined data domain
            WonkaRefEnvironment WonkaRefEnv =
                WonkaRefEnvironment.CreateInstance(false, moMetadataSource);

            WonkaRefAttr NewSalesTransSeqAttr    = WonkaRefEnv.GetAttributeByAttrName("NewSalesTransSeq");
            WonkaRefAttr NewSaleVATRateDenomAttr = WonkaRefEnv.GetAttributeByAttrName("NewSaleVATRateDenom");
            WonkaRefAttr NewSaleItemTypeAttr     = WonkaRefEnv.GetAttributeByAttrName("NewSaleItemType");
            WonkaRefAttr CountryOfSaleAttr       = WonkaRefEnv.GetAttributeByAttrName("CountryOfSale");
            WonkaRefAttr NewSalePriceAttr        = WonkaRefEnv.GetAttributeByAttrName("NewSalePrice");
            WonkaRefAttr PrevSellTaxAmountAttr   = WonkaRefEnv.GetAttributeByAttrName("PrevSellTaxAmount");
            WonkaRefAttr NewSellTaxAmountAttr    = WonkaRefEnv.GetAttributeByAttrName("NewSellTaxAmount");
            WonkaRefAttr NewVATAmountForHMRCAttr = WonkaRefEnv.GetAttributeByAttrName("NewVATAmountForHMRC");
            WonkaRefAttr NewSaleEANAttr          = WonkaRefEnv.GetAttributeByAttrName("NewSaleEAN");

            // We create a target list of the Attributes of the old (i.e., existing) record that currently exists on the blockchain
            // and which we want to pull back during the engine's execution
            moTargetAttrList = new List<WonkaRefAttr>();

            moTargetAttrList =
                new List<WonkaRefAttr>() { NewSalesTransSeqAttr, NewSaleVATRateDenomAttr, NewSaleItemTypeAttr,  
                                           CountryOfSaleAttr, NewSalePriceAttr, PrevSellTaxAmountAttr, 
                                           NewSellTaxAmountAttr, NewVATAmountForHMRCAttr, NewSaleEANAttr};

            if (pbSerializeMetadataToBlockchain)
            {
                SerializeMetadataToBlockchain();
            }
        }

        public void Execute(string psOrchestrationTestAddress = null, bool pbValidateWithinTransaction = false)
        {
            WonkaRefEnvironment RefEnv = WonkaRefEnvironment.GetInstance();

            Dictionary<string, WonkaBizSource> SourceMap = new Dictionary<string, WonkaBizSource>();

            string sDefaultSourceId    = "S";
            string sContractSourceId   = sDefaultSourceId;
            string sContractAddress    = "";
            string sContractAbi        = "";
            string sOrchGetterMethod   = "";
            string sOrchSetterMethod   = "";
            
            // These values indicate the Custom Operator "INVOKE_VAT_LOOKUP" which has been used in the markup - 
            // its implementation can be found in the method "lookupVATDenominator"
            string sCustomOpId         = "INVOKE_VAT_LOOKUP";
            string sCustomOpMethod     = "lookupVATDenominator";

            // If a 'psOrchestrationTestAddress' value has been provided, it indicates that the user wishes
            // to use Orchestration
            if (!String.IsNullOrEmpty(psOrchestrationTestAddress))
            {
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

            WonkaBizSource DefaultSource =
                new WonkaBizSource(sContractSourceId, msSenderAddress, msPassword, sContractAddress, sContractAbi, sOrchGetterMethod, sOrchSetterMethod, RetrieveValueMethod);

            // Here a mapping is created, where each Attribute points to a specific contract and its "accessor" methods
            // - the class that contains this information (contract, accessors, etc.) is of the WonkaBreSource type
            foreach (WonkaRefAttr TempAttr in moTargetAttrList)
            {                
                SourceMap[TempAttr.AttrName] = DefaultSource;
            }

            Dictionary<string, WonkaBizSource> CustomOpSourceMap = new Dictionary<string, WonkaBizSource>();

            // Here a mapping is created, where each Custom Operator points to a specific contract and its "implementation" method
            // - the class that contains this information (contract, accessors, etc.) is of the WonkaBreSource type
            WonkaBizSource CustomOpSource =
                new WonkaBizSource(sCustomOpId, msSenderAddress, msPassword, sContractAddress, sContractAbi, LookupVATDenominator, sCustomOpMethod);

            CustomOpSourceMap[sCustomOpId] = CustomOpSource;

            // Creating an instance of the rules engine using our rules and the metadata
            WonkaBizRulesEngine RulesEngine = 
                new WonkaBizRulesEngine(new StringBuilder(msRulesContents), SourceMap, CustomOpSourceMap, moMetadataSource, false);

            RulesEngine.DefaultSource = sDefaultSourceId;

            // The contract dictates that the RuleTree (and its other info, like the Orchestration and CU metadata) 
            // is serialized to the blockchain before interacting with it
            SerializeRulesEngineToBlockchain(RulesEngine);

            WonkaRefAttr NewSellTaxAmountAttr    = RefEnv.GetAttributeByAttrName("NewSellTaxAmount");
            WonkaRefAttr NewVATAmountForHMRCAttr = RefEnv.GetAttributeByAttrName("NewVATAmountForHMRC");

            // Gets a predefined data record that will be our analog for new data coming into the system
            // We are only using this record to test the .NET implementation
            WonkaProduct NewProduct = GetNewProduct();

            string sSellAmtBefore = GetAttributeValue(NewProduct, NewSellTaxAmountAttr);
            string sVATAmtBefore  = GetAttributeValue(NewProduct, NewVATAmountForHMRCAttr);

            /**
             ** Test the .NET side
             */
            Wonka.BizRulesEngine.Reporting.WonkaBizRuleTreeReport Report = RulesEngine.Validate(NewProduct);

            string sSellAmtAfter = GetAttributeValue(NewProduct, NewSellTaxAmountAttr);
            string sVATAmtAfter  = GetAttributeValue(NewProduct, NewVATAmountForHMRCAttr);

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
                 ** Now execute the rules engine on the blockchain, using both Orchestration to call accessors 
                 ** on other contract(s) and Custom Operators to invoke the "INVOKE_VAT_LOOKUP" operator 
                 ** (i.e., the "lookupVATDenominator()" method) implemented on a contract.
                 **
                 ** NOTE: Based on the value of the argument 'pbValidateWithinTransaction', we will act accordingly - 
                 **       If set to 'true', we issue a call() when we execute the rules engine, since we are only
                 **       looking to validate here.  However, if the value if 'false', we issue a sendTransaction() 
                 **       so that we can attempts to set values (i.e., change the blockchain) will take effect.
                 **       In that case, we might want to pull back the record afterwards with a subsequent function
                 **       call, in order to examine the record here.
                 **       
                 **/
                var BlockchainReport = ExecuteWithReport(RulesEngine, pbValidateWithinTransaction, SourceMap[NewSellTaxAmountAttr.AttrName], psOrchestrationTestAddress);

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

        public RuleTreeReport ExecuteWithReport(WonkaBizRulesEngine poRulesEngine, bool pbValidateWithinTransaction, WonkaBizSource poSource, string psOrchestrationAddress)
        {
            WonkaRefEnvironment RefEnv = WonkaRefEnvironment.GetInstance();

            WonkaRefAttr NewSellTaxAmountAttr    = RefEnv.GetAttributeByAttrName("NewSellTaxAmount");
            WonkaRefAttr NewVATAmountForHMRCAttr = RefEnv.GetAttributeByAttrName("NewVATAmountForHMRC");
            WonkaRefAttr NewSalesTransSeqAttr    = RefEnv.GetAttributeByAttrName("NewSalesTransSeq");
            WonkaRefAttr NewSaleVATRateDenomAttr = RefEnv.GetAttributeByAttrName("NewSaleVATRateDenom");
            WonkaRefAttr PrevSellTaxAmtAttr      = RefEnv.GetAttributeByAttrName("PrevSellTaxAmount");
            WonkaRefAttr NewSaleItemTypeAttr     = RefEnv.GetAttributeByAttrName("NewSaleItemType");
            WonkaRefAttr CountryOfSaleAttr       = RefEnv.GetAttributeByAttrName("CountryOfSale");

            bool bTestLookupMethod = true;

            Dictionary<string, string> PrdKeys = new Dictionary<string, string>();

            var contract = GetContract();

            var executeWithReportFunction = contract.GetFunction(CONST_CONTRACT_FUNCTION_EXEC_RPT);

            RuleTreeReport ruleTreeReport = null;

            if (bTestLookupMethod)
            {
                var contractOrchTest = GetContractOrchTest(psOrchestrationAddress);

                var gas = new Nethereum.Hex.HexTypes.HexBigInteger(1000000);

                var executeLookupFunction = contractOrchTest.GetFunction("lookupVATDenominator");
                var lookupValue = executeLookupFunction.CallAsync<string>("Widget", "UK", "", "").Result;

                var getValOnRecordFunction = contract.GetFunction("getValueOnRecord");
                var attrVal = getValOnRecordFunction.CallAsync<string>(poSource.SenderAddress, "NewSaleItemType").Result;
            }

            if (pbValidateWithinTransaction)
            {
                var executeGetLastReportFunction = contract.GetFunction(CONST_CONTRACT_FUNCTION_GET_LAST_RPT);

                // NOTE: Caused exception to be thrown
                // var gas = executeWithReportFunction.EstimateGasAsync(msSenderAddress).Result;
                var gas = new Nethereum.Hex.HexTypes.HexBigInteger(2000000);

                WonkaProduct OrchContractCurrValues = poRulesEngine.AssembleCurrentProduct(new Dictionary<string, string>());

                string sNSTABeforeOrchestrationAssignment  = RetrieveValueMethod(poSource, NewSellTaxAmountAttr.AttrName);
                string sNVABeforeOrchestrationAssignment   = RetrieveValueMethod(poSource, NewVATAmountForHMRCAttr.AttrName);
                string sNSTSBeforeOrchestrationAssignment  = RetrieveValueMethod(poSource, NewSalesTransSeqAttr.AttrName);
                string sNSVRDBeforeOrchestrationAssignment = RetrieveValueMethod(poSource, NewSaleVATRateDenomAttr.AttrName);
                string sPSTABeforeOrchestrationAssignment  = RetrieveValueMethod(poSource, PrevSellTaxAmtAttr.AttrName);
                string sNSITDBeforeOrchestrationAssignment = RetrieveValueMethod(poSource, NewSaleItemTypeAttr.AttrName);
                string sCoSBeforeOrchestrationAssignment   = RetrieveValueMethod(poSource, CountryOfSaleAttr.AttrName);

                var receiptAddAttribute = 
                    executeWithReportFunction.SendTransactionAsync(msSenderAddress, gas, null, msSenderAddress).Result;

                string sNSTAfterOrchestrationAssignment    = RetrieveValueMethod(poSource, NewSellTaxAmountAttr.AttrName);
                string sNVAAfterOrchestrationAssignment    = RetrieveValueMethod(poSource, NewVATAmountForHMRCAttr.AttrName);
                string sNSTSAfterOrchestrationAssignment   = RetrieveValueMethod(poSource, NewSalesTransSeqAttr.AttrName);
                string sNSVRDAfterOrchestrationAssignment  = RetrieveValueMethod(poSource, NewSaleVATRateDenomAttr.AttrName);
                string sPSTAAfterOrchestrationAssignment   = RetrieveValueMethod(poSource, PrevSellTaxAmtAttr.AttrName);
                string sNSITDAfterOrchestrationAssignment  = RetrieveValueMethod(poSource, NewSaleItemTypeAttr.AttrName);
                string sCoSAfterOrchestrationAssignment    = RetrieveValueMethod(poSource, CountryOfSaleAttr.AttrName);

                ruleTreeReport = executeGetLastReportFunction.CallDeserializingToObjectAsync<RuleTreeReport>().Result;
            }
            else 
                ruleTreeReport = executeWithReportFunction.CallDeserializingToObjectAsync<RuleTreeReport>(msSenderAddress).Result;

            return ruleTreeReport;
        }

        public string LookupVATDenominator(string psSaleItemType, string psCountryOfSale, string psDummyVal1, string psDummyVal2)
        {
            if (psSaleItemType == "Widget" && psCountryOfSale == "UK")
                return "5";
            else
                return "1";            
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

        public Nethereum.Contracts.Contract GetContractOrchTest(string psOrchContractAddress)
        {
            var account = new Account(msPassword);

            var web3 = new Nethereum.Web3.Web3(account);

            var contractAddress = psOrchContractAddress;

            var contract = web3.Eth.GetContract(msAbiOrchTest, contractAddress);

            return contract;
        }

        public Nethereum.Contracts.Contract GetContract(WonkaBizSource TargetSource)
        {
            var account  = new Account(TargetSource.Password);
            var web3     = new Nethereum.Web3.Web3(account);
            var contract = web3.Eth.GetContract(TargetSource.ContractABI, TargetSource.ContractAddress);

            return contract;
        }

        public WonkaProduct GetNewProduct()
        {
            WonkaRefEnvironment WkaRefEnv               = WonkaRefEnvironment.GetInstance();
            WonkaRefAttr        NewSalesTransSeqAttr    = WkaRefEnv.GetAttributeByAttrName("NewSalesTransSeq");
            WonkaRefAttr        NewSaleEANAttr          = WkaRefEnv.GetAttributeByAttrName("NewSaleEAN");
            WonkaRefAttr        NewSaleVATRateDenomAttr = WkaRefEnv.GetAttributeByAttrName("NewSaleVATRateDenom");
            WonkaRefAttr        NewSaleItemTypeAttr     = WkaRefEnv.GetAttributeByAttrName("NewSaleItemType");
            WonkaRefAttr        CountryOfSaleAttr       = WkaRefEnv.GetAttributeByAttrName("CountryOfSale");
            WonkaRefAttr        NewSalePriceAttr        = WkaRefEnv.GetAttributeByAttrName("NewSalePrice");
            WonkaRefAttr        PrevSellTaxAmountAttr   = WkaRefEnv.GetAttributeByAttrName("PrevSellTaxAmount");
            WonkaRefAttr        NewSellTaxAmountAttr    = WkaRefEnv.GetAttributeByAttrName("NewSellTaxAmount");
            WonkaRefAttr        NewVATAmountForHMRCAttr = WkaRefEnv.GetAttributeByAttrName("NewVATAmountForHMRC");

            WonkaProduct NewProduct = new WonkaProduct();

            NewProduct.SetAttribute(NewSalesTransSeqAttr,    "123456789");
            NewProduct.SetAttribute(NewSaleEANAttr,          "9781234567890");
            NewProduct.SetAttribute(NewSaleVATRateDenomAttr, "2");
            NewProduct.SetAttribute(NewSaleItemTypeAttr,     "Widget");
            NewProduct.SetAttribute(CountryOfSaleAttr,       "UK");
            NewProduct.SetAttribute(NewSalePriceAttr,        "200");
            NewProduct.SetAttribute(PrevSellTaxAmountAttr,   "5");
            NewProduct.SetAttribute(NewSellTaxAmountAttr,    "0");
            NewProduct.SetAttribute(NewVATAmountForHMRCAttr, "0");

            return NewProduct;
        }

        public void ResetValueMethod(string psAttrName, string psAttrValue)
        {
            var contract = GetContract();

            var gas = new Nethereum.Hex.HexTypes.HexBigInteger(1000000);

            var setValueFunction = contract.GetFunction("setValueOnRecord");

            var setReceipt =
                setValueFunction.SendTransactionAsync(msSenderAddress, gas, null, msSenderAddress, psAttrName, psAttrValue).Result;
        }

        public string RetrieveValueMethod(WonkaBizSource poTargetSource, string psAttrName)
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

        private void SerializeRulesEngineToBlockchain(WonkaBizRulesEngine poEngine)
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

