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
using WonkaBre.Readers;
using WonkaBre.RuleTree;
using WonkaPrd;
using WonkaRef;

using WonkaEth.Extensions;
using WonkaEth.Validation;

namespace WonkaSystem.TestHarness
{
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

            moMetadataSource = new WonkaMetadataVATSource();

            var TmpAssembly = Assembly.GetExecutingAssembly();

            using (var AbiReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.WonkaEngine.abi")))
            {
                msAbiWonka = AbiReader.ReadToEnd();
            }

            using (var ByteCodeReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.WonkaEngine.bin")))
            {
                msByteCodeWonka = ByteCodeReader.ReadToEnd();
            }

            using (var AbiReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.OrchTest.abi")))
            {
                msAbiOrchTest = AbiReader.ReadToEnd();
            }

            using (var ByteCodeReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.OrchTest.bin")))
            {
                msByteCodeOrchTest = ByteCodeReader.ReadToEnd();
            }

            using (var RulesReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.VATCalculationExample.xml")))
            {
                msRulesContents = RulesReader.ReadToEnd();
            }

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

            Dictionary<string, WonkaBre.RuleTree.WonkaBreSource> SourceMap =
                new Dictionary<string, WonkaBre.RuleTree.WonkaBreSource>();

            string sDefaultSourceId    = "S";
            string sContractSourceId   = sDefaultSourceId;
            string sContractAddress    = "";
            string sContractAbi        = "";
            string sOrchGetterMethod   = "";
            string sOrchSetterMethod   = "";
            string sCustomOpId         = "INVOKE_VAT_LOOKUP";
            string sCustomOpMethod     = "lookupVATDenominator";

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

            WonkaBreSource DefaultSource =
                new WonkaBreSource(sContractSourceId, msSenderAddress, msPassword, sContractAddress, sContractAbi, sOrchGetterMethod, sOrchSetterMethod, RetrieveValueMethod);

            foreach (WonkaRefAttr TempAttr in moTargetAttrList)
            {                
                SourceMap[TempAttr.AttrName] = DefaultSource;
            }

            Dictionary<string, WonkaBreSource> CustomOpSourceMap = new Dictionary<string, WonkaBreSource>();

            WonkaBreSource CustomOpSource =
                new WonkaBreSource(sCustomOpId, msSenderAddress, msPassword, sContractAddress, sContractAbi, LookupVATDenominator, sCustomOpMethod);

            CustomOpSourceMap[sCustomOpId] = CustomOpSource;

            // Cue the rules engine
            WonkaBreRulesEngine RulesEngine = new WonkaBreRulesEngine(new StringBuilder(msRulesContents), SourceMap, CustomOpSourceMap, moMetadataSource);

            RulesEngine.DefaultSource = sDefaultSourceId;

            // The contract dictates that a rules engine is serialized to the blockchain before interacting with it
            SerializeRulesEngineToBlockchain(RulesEngine);

            WonkaRefAttr NewSellTaxAmountAttr    = RefEnv.GetAttributeByAttrName("NewSellTaxAmount");
            WonkaRefAttr NewVATAmountForHMRCAttr = RefEnv.GetAttributeByAttrName("NewVATAmountForHMRC");

            WonkaProduct NewProduct = GetNewProduct();

            /*
            string sStatusValueBefore = GetAttributeValue(NewProduct, AccountStsAttr);
            string sFlagValueBefore   = GetAttributeValue(NewProduct, RvwFlagAttr);
             */

            string sSellAmtBefore = GetAttributeValue(NewProduct, NewSellTaxAmountAttr);
            string sVATAmtBefore  = GetAttributeValue(NewProduct, NewVATAmountForHMRCAttr);

            /**
             ** Test the .NET side
             */
            WonkaBre.Reporting.WonkaBreRuleTreeReport Report = RulesEngine.Validate(NewProduct);

            string sSellAmtAfter = GetAttributeValue(NewProduct, NewSellTaxAmountAttr);
            string sVATAmtAfter  = GetAttributeValue(NewProduct, NewVATAmountForHMRCAttr);

            if (Report.OverallRuleTreeResult == ERR_CD.CD_SUCCESS)
            {
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

            if (!String.IsNullOrEmpty(psOrchestrationTestAddress))
            {
                var BlockchainReport = ExecuteWithReport(RulesEngine, pbValidateWithinTransaction, SourceMap[NewSellTaxAmountAttr.AttrName], psOrchestrationTestAddress);

                if (BlockchainReport.NumberOfRuleFailures == 0)
                {
                    // Serialize(NewProduct);
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

        public RuleTreeReport ExecuteWithReport(WonkaBreRulesEngine poRulesEngine, bool pbValidateWithinTransaction, WonkaBreSource poSource, string psOrchestrationAddress)
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

        public Nethereum.Contracts.Contract GetContract(WonkaBre.RuleTree.WonkaBreSource TargetSource)
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
                poEngine.Serialize(msSenderAddress, msPassword, msContractAddress, msAbiWonka);
        }

    }
}

