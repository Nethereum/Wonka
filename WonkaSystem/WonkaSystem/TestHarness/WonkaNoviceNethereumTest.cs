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

using Wonka.BizRulesEngine;
using WonkaPrd;
using WonkaRef;

using WonkaEth.Extensions;

namespace WonkaSystem.TestHarness
{
    /// <summary>
    /// 
    /// This class represents the aggregated output of the rules engine on the blockchain.
    ///
    /// </summary>
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

    /// <summary>
    /// 
    /// This test will create an instance of the .NET implementation of the rules engine and initialize a 
    /// RuleTree with the rules mentioned in the file 'SimpleAccountCheck.xml'.  It will then populate a 
    /// record with test data, serialize the data to the Ethereum blockchain, and then also serialize the 
    /// RuleTree to the blockchain.  Finally, it will execute the rules engine on the Ethereum blockchain 
    /// and examine the report that is returned.
    ///
    /// NOTE: Even though the rules of the XML markup can reference values from different records (like "O.Price" 
    /// for the existing record's price and "N.Price" for the new record's price), the engine on the blockchain
    /// was not provided any additional direction in this example.  So, by default, it assumes all Attribute
    /// references are for the existing record.
    ///
    /// NOTE: This test does execute the Ethereum implementation of the rules engine.
    ///
    /// NOTE: Like some other tests, the Rules Engine must be deployed by a Solidity script before running this test.
    ///
    /// </summary>
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

        private WonkaBizRulesEngine moRulesEngine = null;

        public WonkaNoviceNethereumTest(string psSenderAddress, string psPassword, string psContractAddress = null, bool bSerializeMetadataAndEngine = true)
        {                       
            msSenderAddress   = psSenderAddress;
            msPassword        = psPassword;

            // Create an instance of the class that will provide us with PmdRefAttributes (i.e., the data domain)
	        // that define our data records
            moMetadataSource = new WonkaMetadataTestSource();

            var TmpAssembly = Assembly.GetExecutingAssembly();

            // Read the ABI of the Ethereum contract for the Wonka rules engine and holds our data record
            using (var AbiReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.WonkaEngine.abi")))
            {
                msAbiWonka = AbiReader.ReadToEnd();
            }

            // Read the bytecodes of the Ethereum contract for the Wonka rules engine and holds our data record
            using (var ByteCodeReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.WonkaEngine.bin")))
            {
                msByteCodeWonka = ByteCodeReader.ReadToEnd();
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

            // We create a target list of the Attributes of the old (i.e., existing) record that currently exists on the blockchain
	        // and which we want to pull back during the engine's execution
            moTargetAttrList = new List<WonkaRefAttr>();

            moTargetAttrList =
                new List<WonkaRefAttr>() { AccountIDAttr, AccountNameAttr, AccountStsAttr, AccountCurrValAttr, AccountTypeAttr, AccountCurrencyAttr };

            if (psContractAddress == null)
                msContractAddress = DeployContract();
            else
                msContractAddress = psContractAddress;
            
            if (bSerializeMetadataAndEngine)
            {
                // Now we serialize the data domain to the blockchain
                SerializeMetadataToBlockchain();

                // And serialize the rules engine to the blockchain
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
            // Using the metadata source, we create an instance of a defined data domain
            WonkaRefEnvironment RefEnv = WonkaRefEnvironment.GetInstance();

            // To test whether the data domain has been created, we plan on retrieving the value
            // of the "AccountStatus" Attribute
            WonkaRefAttr AccountStsAttr = RefEnv.GetAttributeByAttrName("AccountStatus");

            // This collection represents the product key(s) for the record being sought - for now,
            // we do not need to specify one
            Dictionary<string, string> PrdKeys = new Dictionary<string, string>();

            // Gets a predefined data record that will be our analog for new data coming into the system
            WonkaProduct NewProduct = GetNewProduct();

            // Check that the data has been populated correctly on the "new" record - also, we will use
	        // it later for comparison purposes
            string sStatusValueBefore = GetAttributeValue(NewProduct, AccountStsAttr);

            // Write the product to the contract, which will then be used by the rules engine on the 
            // blockchain during the engine's execution
            SerializeProduct(NewProduct);

            /**
             ** Now execute the rules engine on the blockchain.
             **
             ** NOTE: We are only issuing a call() now when we execute the rules engine,
             **       since we are only looking to validate here.  However, there is a chance 
             **       that sendTransaction() might be used in the future because we wish for 
             **       the rules engine to alter the record.  In that case, we might want to 
             **       pull back the record afterwards with a subsequent function call, in order 
             **       to examine the record here.
             **/
            bool bProductIsValid = ExecuteRulesEngineOnTheBlockchain();
            
            // Now we pull back the product from the blockchain
            WonkaProduct ProductOnBlockchain = GetBlockchainRecord(PrdKeys);
            
            // Now retrieve the AccountStatus value and see if the rules have altered it
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

            // Creating an instance of the rules engine using our rules and the metadata
            moRulesEngine =
                new WonkaBizRulesEngine(new StringBuilder(msRulesContents), moMetadataSource);

            moRulesEngine.SetDefaultStdOps(this.msPassword);

            var contract = GetContract();

            var hasRuleTreeFunction = contract.GetFunction(CONST_CONTRACT_FUNCTION_HAS_RT);

            // Out of gas exception
            // var gas = new Nethereum.Hex.HexTypes.HexBigInteger(1000000);

            var gas = hasRuleTreeFunction.EstimateGasAsync(this.msSenderAddress).Result;

            bool bTreeAlreadyExists =
                hasRuleTreeFunction.CallAsync<bool>(this.msSenderAddress, gas, null, this.msSenderAddress).Result;

            if (!bTreeAlreadyExists)
                moRulesEngine.Serialize(msSenderAddress, msPassword, msSenderAddress, msContractAddress, msAbiWonka);
        }

        public void SetAttribute(WonkaProduct poTargetProduct, WonkaRefAttr poTargetAttr, string psTargetValue)
        {
            if (poTargetProduct.GetProductGroup(poTargetAttr.GroupId).GetRowCount() <= 0)
                poTargetProduct.GetProductGroup(poTargetAttr.GroupId).AppendRow();

            poTargetProduct.GetProductGroup(poTargetAttr.GroupId)[0][poTargetAttr.AttrId] = psTargetValue;
        }
    }
}
