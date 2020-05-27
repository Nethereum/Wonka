using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Wonka.BizRulesEngine;
using Wonka.BizRulesEngine.RuleTree;
using Wonka.Eth.Init;
using Wonka.Product;
using Wonka.MetaData;

namespace WonkaSystem.TestHarness
{
    /// <summary>
    /// 
    /// This test will create an instance of the .NET implementation of the rules engine and initialize a 
    /// RuleTree with the rules mentioned in the file 'SimpleAccountCheck.xml'.  It will then populate a 
    /// record with test data and then apply the RuleTree against the record, for the purpose of validating
    /// the record's contents.
    /// 
    /// NOTE: There is no interaction with the Ethereum blockchain in this example.  It only tests the
    ///       .NET implementation of the engine.
    ///
    /// </summary>
    public class WonkaSimpleEthTest
    {
        private readonly string msRulesContents;

        private IMetadataRetrievable moMetadataSource = null;

        public WonkaSimpleEthTest()
        {
            var TmpAssembly = Assembly.GetExecutingAssembly();

	        // Read the XML markup that lists the business rules
            using (var RulesReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.SimpleEthCheck.xml")))
            {
                msRulesContents = RulesReader.ReadToEnd();
            }

            // Create an instance of the class that will provide us with PmdRefAttributes (i.e., the data domain)
	        // that define our data records
            moMetadataSource = new WonkaMetadataTestSource();
        }

	    public void Execute()
        {
	        // Using the metadata source, we create an instance of a defined data domain
            WonkaRefEnvironment RefEnv =
                WonkaRefEnvironment.CreateInstance(false, moMetadataSource);

			var sWeb3Url       = "https://mainnet.infura.io/v3/7238211010344719ad14a89db874158c";
            var sSenderAddress = "0x12890D2cce102216644c59daE5baed380d84830c";
            var sPassword      = "0xb5b1870957d373ef0eeffecc6e4812c0fd08f554b37b233526acc331bf1544f7";
            var sERC20Address  = "0x9f8f72aa9304c8b593d555f12ef6589cc3a579a2";

            // To test whether the data domain has been created, we pull back one attribute
            WonkaRefAttr AccountStsAttr   = RefEnv.GetAttributeByAttrName("AccountStatus");
            WonkaRefAttr AuditRvwFlagAttr = RefEnv.GetAttributeByAttrName("AuditReviewFlag");

            Dictionary<string, WonkaBizSource> SourceMap = new Dictionary<string, WonkaBizSource>();

            /*
            WonkaBizSource DefaultSource = 
                new WonkaBizSource(sContractSourceId, msSenderAddress, msPassword, sContractAddress, sContractAbi, sOrchGetterMethod, sOrchSetterMethod, RetrieveValueMethod);
		    */

            WonkaBizSource DefaultSource =
                new WonkaBizSource("", "", "", "", null);

            // Here a mapping is created, where each Attribute points to a specific contract and its "accessor" methods
            // - the class that contains this information (contract, accessors, etc.) is of the WonkaBreSource type
            foreach (WonkaRefAttr TempAttr in RefEnv.AttrCache)
            {
                SourceMap[TempAttr.AttrName] = DefaultSource;
            }

            var WonkaEthEngInit =
                new WonkaEthEngineInitialization() { EthSenderAddress = sSenderAddress,
                                                     EthPassword = sPassword,
                                                     ERC20ContractAddress = sERC20Address,
                                                     ERC721ContractAddress = "",
                                                     Web3HttpUrl = sWeb3Url };

            // Creating an instance of the rules engine using our rules and the metadata
            WonkaBizRulesEngine RulesEngine =
                new WonkaEthRulesEngine(new StringBuilder(msRulesContents), SourceMap, WonkaEthEngInit, moMetadataSource);

	        // Gets a predefined data record that will be our analog for new data coming into the system
            WonkaProduct NewProduct = GetNewProduct();

	        // Check that the data has been populated correctly on the "new" record
            string sStatusValueBefore = GetAttributeValue(NewProduct, AccountStsAttr);

            // RulesEngine.GetCurrentProductDelegate = GetOldProduct;
    
	        // Validate the new record using our rules engine and its initialized RuleTree
            Wonka.BizRulesEngine.Reporting.WonkaBizRuleTreeReport Report = RulesEngine.Validate(NewProduct);

	        // Now retrieve the AccountStatus value and see if the rules have altered it (which should
            // not be the case)
	        string sStatusValueAfter  = GetAttributeValue(NewProduct, AccountStsAttr);
            string sAuditRvwFlagAfter = GetAttributeValue(NewProduct, AuditRvwFlagAttr);

            if (Report.GetRuleSetFailureCount() > 0)
            {
                throw new Exception("Oh heavens to Betsy! Something bad happened!"); 
            }
        }

        public WonkaProduct GetOldProduct(Dictionary<string,string> poProductKeys)
        {
            WonkaRefEnvironment WkaRefEnv           = WonkaRefEnvironment.GetInstance();
            WonkaRefAttr        AccountIDAttr       = WkaRefEnv.GetAttributeByAttrName("BankAccountID");
            WonkaRefAttr        AccountNameAttr     = WkaRefEnv.GetAttributeByAttrName("BankAccountName");
            WonkaRefAttr        AccountStsAttr      = WkaRefEnv.GetAttributeByAttrName("AccountStatus");
            WonkaRefAttr        AccountCurrValAttr  = WkaRefEnv.GetAttributeByAttrName("AccountCurrValue");
            WonkaRefAttr        AccountTypeAttr     = WkaRefEnv.GetAttributeByAttrName("AccountType");
            WonkaRefAttr        AccountCurrencyAttr = WkaRefEnv.GetAttributeByAttrName("AccountCurrency");

            WonkaProduct OldProduct = new WonkaProduct();

            SetAttribute(OldProduct, AccountIDAttr,       "1234567890");
            SetAttribute(OldProduct, AccountNameAttr,     "JohnSmithFirstCheckingAccount");
            // SetAttribute(OldProduct, AccountStsAttr,      "ACT");
            SetAttribute(OldProduct, AccountStsAttr,      "OOS");
            SetAttribute(OldProduct, AccountCurrValAttr,  "100.00");
            SetAttribute(OldProduct, AccountCurrencyAttr, "USD");
            SetAttribute(OldProduct, AccountTypeAttr,     "Checking");

            return OldProduct;            
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
            SetAttribute(NewProduct, AccountCurrValAttr,  "100.00");
            SetAttribute(NewProduct, AccountCurrencyAttr, "USD");
	        // SetAttribute(NewProduct, AccountTypeAttr,     "Checking");
            SetAttribute(NewProduct, AccountTypeAttr,     "CompletelyBogusTypeThatWillCauseAnError");

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

	    public void SetAttribute(WonkaProduct poTargetProduct, WonkaRefAttr poTargetAttr, string psTargetValue)
        {
                if (poTargetProduct.GetProductGroup(poTargetAttr.GroupId).GetRowCount() <= 0)
                    poTargetProduct.GetProductGroup(poTargetAttr.GroupId).AppendRow();

                poTargetProduct.GetProductGroup(poTargetAttr.GroupId)[0][poTargetAttr.AttrId] = psTargetValue;
	    }
    }
}
