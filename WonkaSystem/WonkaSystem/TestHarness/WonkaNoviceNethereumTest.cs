using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.ABI.Model;
using Nethereum.Geth;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

using Nethereum.Contracts;
using Nethereum.Hex;
using Nethereum.Web3.Accounts.Managed;
using Nethereum.Geth.RPC.Miner;

using Xunit;

using WonkaBre;
using WonkaPrd;
using WonkaRef;

using WonkaEth.Extensions;

namespace WonkaSystem.TestHarness
{
    [FunctionOutput]
    public class RuleTreeReport
    {
        [Parameter("uint", "fails", 1)]
        public uint NumberOfRuleFailures { get; set; }

        [Parameter("bytes32[]", "rsets", 2)]
        public List<string> RuleSetIds { get; set; }

        [Parameter("bytes32[]", "rules", 3)]
        public List<string> RuleIds { get; set; }

        /*
        [Parameter("bytes32[]", "values", 4)]
        public List<string> RecordValues { get; set; }
        */
    }

    public class WonkaNoviceNethereumTest
    {
        private const int CONST_CONTRACT_ATTR_NUM_ON_START = 3;

        private readonly string msRulesContents;
        private readonly string msAbiWonka;
        private readonly string msByteCodeWonka;

        public const string CONST_CONTRACT_FUNCTION_HAS_RT   = "hasRuleTree";
        public const string CONST_CONTRACT_FUNCTION_EXEC_RPT = "executeWithReport"; 

        private IMetadataRetrievable moMetadataSource = null;

        private string msSenderAddress   = "";
        private string msPassword        = "";
        private string msContractAddress = "";

        private List<WonkaRefAttr> moTargetAttrList = null;

        private WonkaBreRulesEngine moRulesEngine = null;

        public WonkaNoviceNethereumTest(string psSenderAddress, string psPassword, string psContractAddress = null, bool bSerializeMetadataAndEngine = true)
        {                       
            msSenderAddress   = psSenderAddress;
            msPassword        = psPassword;

            moMetadataSource = new WonkaMetadataTestSource();

            var TmpAssembly = Assembly.GetExecutingAssembly();

            using (var AbiReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.WonkaEngine.abi")))
            {
                msAbiWonka = AbiReader.ReadToEnd();
            }

            using (var ByteCodeReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.WonkaEngine.bin")))
            {
                msByteCodeWonka = ByteCodeReader.ReadToEnd();
            }

            using (var RulesReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.SimpleAccountCheck.xml")))
            {
                msRulesContents = RulesReader.ReadToEnd();
            }

            WonkaRefEnvironment WonkaRefEnv =
                WonkaRefEnvironment.CreateInstance(false, moMetadataSource);

            WonkaRefAttr        AccountIDAttr       = WonkaRefEnv.GetAttributeByAttrName("BankAccountID");
            WonkaRefAttr        AccountNameAttr     = WonkaRefEnv.GetAttributeByAttrName("BankAccountName");
            WonkaRefAttr        AccountStsAttr      = WonkaRefEnv.GetAttributeByAttrName("AccountStatus");
            WonkaRefAttr        AccountCurrValAttr  = WonkaRefEnv.GetAttributeByAttrName("AccountCurrValue");
            WonkaRefAttr        AccountTypeAttr     = WonkaRefEnv.GetAttributeByAttrName("AccountType");
            WonkaRefAttr        AccountCurrencyAttr = WonkaRefEnv.GetAttributeByAttrName("AccountCurrency");

            moTargetAttrList = new List<WonkaRefAttr>();

            moTargetAttrList =
                new List<WonkaRefAttr>() { AccountIDAttr, AccountNameAttr, AccountStsAttr, AccountCurrValAttr, AccountTypeAttr, AccountCurrencyAttr };

            if (psContractAddress == null)
                msContractAddress = DeployContract();
            else
                msContractAddress = psContractAddress;

            if (bSerializeMetadataAndEngine)
            {
                SerializeMetadataToBlockchain();

                SerializeRulesEngineToBlockchain();
            }
        }

        public string DeployContract()
        {
            string sSenderAddress   = msSenderAddress;
            string sContractAddress = "blah";

            var account = new Account(msPassword);
            var web3    = new Nethereum.Web3.Web3(account);

            System.Numerics.BigInteger totalSupply = System.Numerics.BigInteger.Parse("100000000000");

            /**
             ** NOTE: Deployment issues have not yet been resolved - more work needs to be done
             **
             // System.Exception: Too many arguments: 1 > 0
             // at Nethereum.ABI.FunctionEncoding.ParametersEncoder.EncodeParameters (Nethereum.ABI.Model.Parameter[] parameters, System.Object[] values) [0x00078] in <b4e1e3b6a7e947da9576619c2d31bafc>:0 
             // var receipt = 
             // web3.Eth.DeployContract.SendRequestAndWaitForReceiptAsync(msAbiWonka, msByteCodeWonka, sSenderAddress, new Nethereum.Hex.HexTypes.HexBigInteger(900000), null, totalSupply).Result;
             // sContractAddress = receipt.ContractAddress;

             // var unlockReceipt = web3.Personal.UnlockAccount.SendRequestAsync(sSenderAddress, msPassword, 120).Result;

             // base fee exceeds gas limit?
             // https://gitter.im/Nethereum/Nethereum?at=5a15318e540c78242d34505f
             // sContractAddress = web3.Eth.DeployContract.SendRequestAsync(msAbiWonka, msByteCodeWonka, sSenderAddress, new Nethereum.Hex.HexTypes.HexBigInteger(totalSupply)).Result;
             **        
             **/

            return sContractAddress;
        }

        public bool Execute()
        {
            WonkaRefEnvironment RefEnv = WonkaRefEnvironment.GetInstance();

            WonkaRefAttr AccountStsAttr = RefEnv.GetAttributeByAttrName("AccountStatus");

            Dictionary<string, string> PrdKeys = new Dictionary<string, string>();

            WonkaProduct NewProduct = GetNewProduct();

            string sStatusValueBefore = GetAttributeValue(NewProduct, AccountStsAttr);

            // Write the product to the contract, so that we can validate the product 
            // using the ruletree that has already been written to the blockchain
            SerializeProduct(NewProduct);

            bool bProductIsValid = ExecuteRulesEngineOnTheBlockchain();

            /**
             ** Now we pull back the product from the blockchain
             **
             ** NOTE: We are only issuing a call() now when we execute the rules engine,
             **       since we are only looking to validate here.  However, there is a chance 
             **       that sendTransaction() might be used in the future because we wish for 
             **       the rules engine to alter the record.  In that case, we might want to 
             **       pull back the record afterwards in order to examine the record here.
             **/
            WonkaProduct ProductOnBlockchain = GetBlockchainRecord(PrdKeys);
            string sStatusValueAfter = GetAttributeValue(NewProduct, AccountStsAttr);

            return bProductIsValid;
        }

        public RuleTreeReport ExecuteWithReport()
        {
            WonkaRefEnvironment RefEnv = WonkaRefEnvironment.GetInstance();

            WonkaRefAttr AccountStsAttr = RefEnv.GetAttributeByAttrName("AccountStatus");

            Dictionary<string, string> PrdKeys = new Dictionary<string, string>();

            WonkaProduct NewProduct = GetNewProduct();

            // Write the product to the contract, so that we can validate the product 
            // using the ruletree that has already been written to the blockchain
            SerializeProduct(NewProduct);

            var contract = GetContract();

            var executeWithReportFunction = contract.GetFunction(CONST_CONTRACT_FUNCTION_EXEC_RPT);

            var ruleTreeReport = executeWithReportFunction.CallDeserializingToObjectAsync<RuleTreeReport>(msSenderAddress).Result;

            return ruleTreeReport;
        }

        private bool ExecuteRulesEngineOnTheBlockchain()
        {
            bool bValid = true;

            var contract = GetContract();

            var executeRulesEngineFunction = contract.GetFunction("execute");

            // var gas = executeRulesEngineFunction.EstimateGasAsync(sSenderAddress).Result;
            var gas = new Nethereum.Hex.HexTypes.HexBigInteger(1000000);

            // var receiptExecuteEngine = 
            //    executeRulesEngineFunction.SendTransactionAsync(msSenderAddress, gas, null, msSenderAddress).Result;

            bValid = executeRulesEngineFunction.CallAsync<bool>(msSenderAddress, gas, null, msSenderAddress).Result;

            return bValid;
        }

        public WonkaProduct GetBlockchainRecord(Dictionary<string,string> poProductKeys)
        {
            WonkaProduct OldProduct = new WonkaProduct();

            Dictionary<WonkaRefAttr, string> EthereumRecord = GetOldRecordViaEthereum(moTargetAttrList, poProductKeys);

            foreach (WonkaRefAttr TempAttr in EthereumRecord.Keys)
            {
                SetAttribute(OldProduct, TempAttr, EthereumRecord[TempAttr]);
            }

            return OldProduct;            
        }

        public Dictionary<WonkaRefAttr, string> GetOldRecordViaEthereum(List<WonkaRefAttr> poTargetAttrList, Dictionary<string, string> poProductKeys)
        {
            Dictionary<WonkaRefAttr, string> EthereumRecord = new Dictionary<WonkaRefAttr, string>();

            string sSenderAddress = msSenderAddress;

            var contract = GetContract();

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

            WonkaProduct NewProduct = new WonkaProduct();

            SetAttribute(NewProduct, AccountIDAttr,       "1234567890");
            SetAttribute(NewProduct, AccountNameAttr,     "JohnSmithFirstCheckingAccount");
            SetAttribute(NewProduct, AccountStsAttr,      "ACT");
            SetAttribute(NewProduct, AccountCurrValAttr,  "101.00");
            SetAttribute(NewProduct, AccountCurrencyAttr, "USD");
            // SetAttribute(NewProduct, AccountTypeAttr,     "Checking");
            SetAttribute(NewProduct, AccountTypeAttr,     "CompletelyBogusTypeThatWillCauseAnError");

            return NewProduct;
        }

        public Nethereum.Contracts.Contract GetContract()
        {
            var account = new Account(msPassword);

            var web3 = new Nethereum.Web3.Web3(account);

            var contractAddress = msContractAddress;

            var contract = web3.Eth.GetContract(msAbiWonka, contractAddress);

            return contract;
        }

        public string GetAttributeValue(WonkaProduct poTargetProduct, WonkaRefAttr poTargetAttr)
        {
            if (poTargetProduct.GetProductGroup(poTargetAttr.GroupId).GetRowCount() <= 0)
                throw new Exception("ERROR!  Provided incoming product has empty group.");

            string sAttrValue = poTargetProduct.GetProductGroup(poTargetAttr.GroupId)[0][poTargetAttr.AttrId];

            if (String.IsNullOrEmpty(sAttrValue))
                throw new Exception("ERROR!  Provided incoming product has no value for needed key(" + poTargetAttr.AttrName + ").");

            return sAttrValue;
        }

        public async Task<TransactionReceipt> MineAndGetReceiptAsync(Web3Geth poWeb3, string transactionHash)
        {
            var miningResult = await poWeb3.Miner.Start.SendRequestAsync(6);
            Assert.True(miningResult);

            var receipt = await poWeb3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);

            while (receipt == null)
            {
                System.Threading.Thread.Sleep(1000);
                receipt = await poWeb3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
            }

            miningResult = await poWeb3.Miner.Stop.SendRequestAsync();
            Assert.True(miningResult);
            return receipt;
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

        public void SerializeProduct(WonkaProduct poTargetProduct)
        {
            string sSenderAddress = msSenderAddress;

            var contract = GetContract();

            var setValueOnRecordFunction = contract.GetFunction("setValueOnRecord");

            // Out of gas exception
            // var gas = setValueOnRecordFunction.EstimateGasAsync(sSenderAddress, "SomeAttr", "SomeValue").Result;
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

        private void SerializeRulesEngineToBlockchain()
        {
            WonkaRefEnvironment RefEnv = WonkaRefEnvironment.GetInstance();

            // Cue the rules engine
            moRulesEngine =
                new WonkaBreRulesEngine(new StringBuilder(msRulesContents), moMetadataSource);

            var contract = GetContract();

            var hasRuleTreeFunction = contract.GetFunction(CONST_CONTRACT_FUNCTION_HAS_RT);

            // Out of gas exception
            // var gas = new Nethereum.Hex.HexTypes.HexBigInteger(1000000);

            var gas = hasRuleTreeFunction.EstimateGasAsync(this.msSenderAddress).Result;

            bool bTreeAlreadyExists =
                hasRuleTreeFunction.CallAsync<bool>(this.msSenderAddress, gas, null, this.msSenderAddress).Result;

            if (!bTreeAlreadyExists)
                moRulesEngine.Serialize(msSenderAddress, msPassword, msContractAddress, msAbiWonka);
        }

        public void SetAttribute(WonkaProduct poTargetProduct, WonkaRefAttr poTargetAttr, string psTargetValue)
        {
            if (poTargetProduct.GetProductGroup(poTargetAttr.GroupId).GetRowCount() <= 0)
                poTargetProduct.GetProductGroup(poTargetAttr.GroupId).AppendRow();

            poTargetProduct.GetProductGroup(poTargetAttr.GroupId)[0][poTargetAttr.AttrId] = psTargetValue;
        }
    }
}
