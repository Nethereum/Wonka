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
using Wonka.Product;
using Wonka.MetaData;

namespace WonkaSystem.TestHarness
{
    /// <summary>
    /// 
    /// This test will create an instance of the .NET implementation of the rules engine and initialize a 
    /// RuleTree with the rules mentioned in the file 'SimpleAccountCheck.xml'.  It will then populate a 
    /// record with test data and then apply the RuleTree against the record, for the purpose of validating
    /// the record's contents.  Unlike WonkaSimpleTest, this test will pull the existing record from a contract
    /// on the Ethereum blockchain.
    ///
    /// The rules of the XML markup can reference values from different records, like "O.Price" for the existing
    /// record's price and "N.Price" for the new record's price.
    ///
    /// NOTE: This test does not execute the Ethereum implementation of the rules engine.  It only tests the
    ///       .NET implementation of the engine.
    ///
    /// </summary>
    public class WonkaSimpleNethereumTest
    {
        private readonly string msRulesContents;
        private readonly string msAbiWonka;
        private readonly string msByteCodeWonka;

	    private IMetadataRetrievable moMetadataSource = null;

        private string msSenderAddress   = "";
        private string msPassword        = "";
        private string msContractAddress = "";

        List<WonkaRefAttr> moTargetAttrList = null;

        public WonkaSimpleNethereumTest(string psSenderAddress, string psPassword, string psContractAddress)
        {                       
            msSenderAddress   = psSenderAddress;
            msPassword        = psPassword;
            msContractAddress = psContractAddress; 

            // Create an instance of the class that will provide us with PmdRefAttributes (i.e., the data domain)
	        // that define our data records		
            moMetadataSource = new WonkaMetadataTestSource();

            var TmpAssembly = Assembly.GetExecutingAssembly();

	        // Read the ABI of the Ethereum contract which holds our old (i.e., existing) data record
            using (var AbiReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.WonkaEngine.abi")))
            {
                msAbiWonka = AbiReader.ReadToEnd();
            }

            // Read the bytecodes of the Ethereum contract which holds our old (i.e., existing) data record
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
        }

        public void Execute()
        {
	        // Using the metadata source, we create an instance of a defined data domain
            WonkaRefEnvironment RefEnv = WonkaRefEnvironment.GetInstance();

	        // To test whether the data domain has been created, we plan on retrieving the value
            // of the "AccountStatus" Attribute
            WonkaRefAttr AccountStsAttr = RefEnv.GetAttributeByAttrName("AccountStatus");

            // Creating an instance of the rules engine using our rules and the metadata
            WonkaBizRulesEngine RulesEngine =
                new WonkaBizRulesEngine(new StringBuilder(msRulesContents), moMetadataSource);

	        // Gets a predefined data record that will be our analog for new data coming into the system
            WonkaProduct NewProduct = GetNewProduct();

	        // Check that the data has been populated correctly on the "new" record - also, we will use
	        // it later for comparison purposes
            string sStatusValueBefore = GetAttributeValue(NewProduct, AccountStsAttr);

	        // Since the rules can reference values from different records (like O.Price for the existing
	        // record's price and N.Price for the new record's price), we need to provide the delegate
	        // that can pull the existing (i.e., old) record from the blockchain using a key
            RulesEngine.GetCurrentProductDelegate = GetOldProduct;

            // Validate the new record using our rules engine and its initialized RuleTree		
            Wonka.BizRulesEngine.Reporting.WonkaBizRuleTreeReport Report = RulesEngine.Validate(NewProduct);

            // Now retrieve the AccountStatus value and see if the rules have altered it (which should
            // not be the case)
            string sStatusValueAfter = GetAttributeValue(NewProduct, AccountStsAttr);

	        // We will evaluate whether or not any failures of the rules were detected during the engine's execution
            if (Report.OverallRuleTreeResult == ERR_CD.CD_SUCCESS)
            {
                // If successful, we will write the record back into the contract on the blockchain
                Serialize(NewProduct);
            }
            else if (Report.GetRuleSetFailureCount() > 0)                
            {
                throw new Exception("Oh heavens to Betsy! Something bad happened!"); 
            }
            else
            {
                throw new Exception("What in the world is happening?!");
            }
        }

        public WonkaProduct GetOldProduct(Dictionary<string,string> poProductKeys)
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

            WonkaProduct NewProduct = new WonkaProduct();

            SetAttribute(NewProduct, AccountIDAttr,       "1234567890");
            SetAttribute(NewProduct, AccountNameAttr,     "JohnSmithFirstCheckingAccount");
            SetAttribute(NewProduct, AccountStsAttr,      "ACT");
            SetAttribute(NewProduct, AccountCurrValAttr,  "101.00");
            SetAttribute(NewProduct, AccountCurrencyAttr, "USD");
            SetAttribute(NewProduct, AccountTypeAttr,     "Checking");
            // SetAttribute(NewProduct, AccountTypeAttr,     "CompletelyBogusTypeThatWillCauseAnError");

            return NewProduct;
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

        public void Serialize(WonkaProduct poTargetProduct)
        {
            string sSenderAddress = msSenderAddress;

            var account = new Account(msPassword);

            var web3 = new Nethereum.Web3.Web3(account);

            var contractAddress = msContractAddress;

            var contract = web3.Eth.GetContract(msAbiWonka, contractAddress);

            var setValueOnRecordFunction = contract.GetFunction("setValueOnRecord");

            var gas = setValueOnRecordFunction.EstimateGasAsync(sSenderAddress, "SomeAttr", "SomeValue").Result;

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

        public void SetAttribute(WonkaProduct poTargetProduct, WonkaRefAttr poTargetAttr, string psTargetValue)
        {
            if (poTargetProduct.GetProductGroup(poTargetAttr.GroupId).GetRowCount() <= 0)
                poTargetProduct.GetProductGroup(poTargetAttr.GroupId).AppendRow();

            poTargetProduct.GetProductGroup(poTargetAttr.GroupId)[0][poTargetAttr.AttrId] = psTargetValue;
        }
    }
}
