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
using WonkaPrd;
using WonkaRef;

namespace WonkaSystem.TestHarness
{
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
        }

        public void Execute()
        {
            WonkaRefEnvironment RefEnv = WonkaRefEnvironment.GetInstance();

            WonkaRefAttr AccountStsAttr = RefEnv.GetAttributeByAttrName("AccountStatus");

            // Cue the rules engine
            WonkaBreRulesEngine RulesEngine =
                new WonkaBreRulesEngine(new StringBuilder(msRulesContents), moMetadataSource);

            WonkaProduct NewProduct = GetNewProduct();

            string sStatusValueBefore = GetAttributeValue(NewProduct, AccountStsAttr);

            RulesEngine.GetCurrentProductDelegate = GetOldProduct;

            WonkaBre.Reporting.WonkaBreRuleTreeReport Report = RulesEngine.Validate(NewProduct);

            string sStatusValueAfter = GetAttributeValue(NewProduct, AccountStsAttr);

            if (Report.OverallRuleTreeResult == ERR_CD.CD_SUCCESS)
            {
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
