using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.ABI.Model;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3.Accounts;

using Nethereum.ABI.FunctionEncoding.AttributeEncoding;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.Eth.Filters;
using Nethereum.Hex.HexConvertors.Extensions;

using Wonka.BizRulesEngine;
using Wonka.BizRulesEngine.Reporting;
using Wonka.MetaData;
using Wonka.Product;

using Wonka.Eth.Contracts;
using Wonka.Eth.Extensions;
using Wonka.Eth.Misc;

namespace Wonka.Eth.Validation
{

    /// <summary>
    /// 
    /// This base class serves as the first iteration of the executor class for the rules engine, namely 
    /// its more simple functionality (i.e., without orchestration).
    /// 
    /// </summary>
    public abstract class AbstractWonkaValidator<T> where T : ICommand
    {
        public const string CONST_EVENT_CALL_RULE_TREE        = "CallRuleTree";
        public const string CONST_EVENT_CALL_RULE_SET         = "CallRuleSet";
        public const string CONST_EVENT_CALL_RULE             = "CallRule";

        public const string CONST_CONTRACT_FUNCTION_EXECUTE   = "execute";
        public const string CONST_CONTRACT_FUNCTION_EXEC_RPT  = "executeWithReport"; 

        public const string CONST_CONTRACT_FUNCTION_HAS_RT    = "hasRuleTree";
        public const string CONST_CONTRACT_FUNCTION_SET_VALUE = "setValueOnRecord";

        private WonkaBlockchainEngine moBlockchainEngine;

        public readonly string        msRulesFilepath;
        public readonly StringBuilder msRulesContents;

        public readonly string              msWeb3HttpUrl;
        public readonly WonkaBizRulesEngine moRulesEngine;

        public AbstractWonkaValidator(T poCommand, string psRulesFilepath, string psWeb3HttpUrl = null, bool bDeployEngineToBlockchain = false)
         {
            BlockchainEngine      = new WonkaBlockchainEngine();
            BlockchainEngineOwner = "";

            msRulesFilepath = psRulesFilepath;
            msRulesContents = null;
            msWeb3HttpUrl   = psWeb3HttpUrl;

            Init();

            moRulesEngine = new WonkaBizRulesEngine(msRulesFilepath);
        }

        public AbstractWonkaValidator(T poCommand, StringBuilder psRules, string psWeb3HttpUrl = null, bool bDeployEngineToBlockchain = false)
        {
            BlockchainEngine      = new WonkaBlockchainEngine();
            BlockchainEngineOwner = "";

            msRulesFilepath = null;
            msRulesContents = psRules;
            msWeb3HttpUrl   = psWeb3HttpUrl;

            Init();

            moRulesEngine = new WonkaBizRulesEngine(msRulesContents);
        }

        public static void AssignPropertiesViaReflection(T poCommand, Hashtable poDataValues)
        {
            PropertyInfo[] Props = poCommand.GetProperties();

            Dictionary<PropertyInfo, WonkaRefAttr> PropMap = poCommand.GetPropertyMap();

            // Set Commentary Attributes
            foreach (PropertyInfo TmpProperty in Props)
            {
                object oPropAttrValue = null;

                Type   oAttrType = TmpProperty.PropertyType;
                string sAttrName = PropMap.ContainsKey(TmpProperty) ? PropMap[TmpProperty].AttrName : "";

                if (poDataValues.ContainsKey(sAttrName))
                {
                    object oAttrValue = poDataValues[sAttrName];

                    if (!String.IsNullOrEmpty((string)oAttrValue))
                    {
                        var targetType = IsNullableType(oAttrType) ? Nullable.GetUnderlyingType(oAttrType) : oAttrType;

                        oPropAttrValue = Convert.ChangeType(oAttrValue, targetType);

                        TmpProperty.SetValue(poCommand, oPropAttrValue, null);
                    }
                }
            }
        }

        protected Nethereum.Contracts.Contract GetContract()
        {
            var account = new Account(BlockchainEngine.Password);

            Nethereum.Web3.Web3 web3 = null;
            if (!String.IsNullOrEmpty(msWeb3HttpUrl))
                web3 = new Nethereum.Web3.Web3(account, msWeb3HttpUrl);
            else
                web3 = new Nethereum.Web3.Web3(account);

            var contract = web3.Eth.GetContract(BlockchainEngine.ContractABI, BlockchainEngine.ContractAddress);

            return contract;
        }

        protected WonkaProduct GetEmptyProduct(Dictionary<string,string> poKeys)
        {
            return new WonkaProduct();
        }

        public static void GetPropertiesViaReflection(T poCommand, Hashtable poDataValues)
        {
            PropertyInfo[] Props = poCommand.GetProperties();

            Dictionary<PropertyInfo, WonkaRefAttr> PropMap = poCommand.GetPropertyMap();

            // Set Commentary Attributes
            foreach (PropertyInfo TmpProperty in Props)
            {
                Type   oAttrType  = TmpProperty.PropertyType;
                string sAttrName  = PropMap.ContainsKey(TmpProperty) ? PropMap[TmpProperty].AttrName : "";
                string sAttrValue = Convert.ToString(TmpProperty.GetValue(poCommand));

                if (!String.IsNullOrEmpty(sAttrValue))
                    poDataValues[sAttrName] = sAttrValue;
            }
        }

        public static WonkaProduct GetWonkaProductViaReflection(T poCommand)
        {
            PropertyInfo[] Props = poCommand.GetProperties();

            Dictionary<PropertyInfo, WonkaRefAttr> PropMap = poCommand.GetPropertyMap();

            WonkaProduct TransformedProduct = new WonkaProduct();

            // Set Commentary Attributes
            foreach (PropertyInfo TmpProperty in Props)
            {
                Type   oAttrType  = TmpProperty.PropertyType;
                string sAttrName  = PropMap.ContainsKey(TmpProperty) ? PropMap[TmpProperty].AttrName : "";
                string sAttrValue = Convert.ToString(TmpProperty.GetValue(poCommand));

                if (!String.IsNullOrEmpty(sAttrValue))
                    SetAttribute(TransformedProduct, PropMap[TmpProperty], sAttrValue);
            }

            return TransformedProduct;
        }

        protected void HandleEvents(Event poRuleTreeEvent, Event poRuleSetEvent, Event poRuleEvent, HexBigInteger rtFilter, HexBigInteger rsFilter, HexBigInteger rlFilter)
        {            
            var ruleTreeLog = poRuleTreeEvent.GetFilterChanges<Wonka.Eth.Validation.CallRuleTreeEvent>(rtFilter).Result;
            var ruleSetLog  = poRuleSetEvent.GetFilterChanges<Wonka.Eth.Validation.CallRuleSetEvent>(rsFilter).Result;
            var ruleLog     = poRuleEvent.GetFilterChanges<Wonka.Eth.Validation.CallRuleEvent>(rlFilter).Result;

            // Assert.Equal(1, ruleTreeLog.Count);

            if (ruleTreeLog.Count > 0)
                System.Console.WriteLine("RuleTree Called that Belongs to : (" + ruleTreeLog[0].Event.TreeOwner + ")");

            if (ruleSetLog.Count > 0)
            {
                foreach (EventLog<CallRuleSetEvent> TmpRuleSetEvent in ruleSetLog)
                {
                    System.Console.WriteLine("RuleSet Called with ID : (" + TmpRuleSetEvent.Event.RuleSetId + ")");
                }
            }

            if (ruleLog.Count > 0)
            {
                foreach (EventLog<CallRuleEvent> TmpRuleEvent in ruleLog)
                {
                    System.Console.WriteLine("Rule Called with ID : (" + TmpRuleEvent.Event.RuleId + ") and RuleType(" + TmpRuleEvent.Event.RuleType + ")");
                }
            }

        }

        private void Init()
        {
            WonkaRefEnvironment WonkaRefEnv = null;

            try
            {
                WonkaRefEnv = WonkaRefEnvironment.GetInstance();
            }
            catch (Exception ex)
            {
                WonkaRefEnv = WonkaRefEnvironment.CreateInstance(false, new WonkaMetadataTestSource());

                // NOTE: Should/could contract be deployed here along with metadata (i.e., Attributes)? 

                /*
                WonkaRefAttr AccountIDAttr       = WonkaRefEnv.GetAttributeByAttrName("BankAccountID");
                WonkaRefAttr AccountNameAttr     = WonkaRefEnv.GetAttributeByAttrName("BankAccountName");
                WonkaRefAttr AccountStsAttr      = WonkaRefEnv.GetAttributeByAttrName("AccountStatus");
                WonkaRefAttr AccountCurrValAttr  = WonkaRefEnv.GetAttributeByAttrName("AccountCurrValue");
                WonkaRefAttr AccountTypeAttr     = WonkaRefEnv.GetAttributeByAttrName("AccountType");
                WonkaRefAttr AccountCurrencyAttr = WonkaRefEnv.GetAttributeByAttrName("AccountCurrency");
                */
            }
        }

        static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }

        protected virtual void SerializeRecordToBlockchain(T poCommand)
        {
            Hashtable DataValues = new Hashtable();

            GetPropertiesViaReflection(poCommand, DataValues);

            var contract = GetContract();

            var setValueOnRecordFunction = contract.GetFunction(CONST_CONTRACT_FUNCTION_SET_VALUE);

            // Out of gas exception
            // var gas = setValueOnRecordFunction.EstimateGasAsync(sSenderAddress, "SomeAttr", "SomeValue").Result;
            var gas = new Nethereum.Hex.HexTypes.HexBigInteger(1000000);

            foreach (String sTempAttrName in DataValues.Keys)
            {
                string sSenderAddr = BlockchainEngine.SenderAddress;
                string sAttrValue  = (string) DataValues[sTempAttrName];

                var receiptSetValueOnRecord =
                    setValueOnRecordFunction.SendTransactionAsync(sSenderAddr, gas, null, sSenderAddr, sTempAttrName, sAttrValue).Result;
            }
        }

        protected virtual void SerializeRulesEngineToBlockchain()
        {
            var contract = GetContract();

            var hasRuleTreeFunction = contract.GetFunction(CONST_CONTRACT_FUNCTION_HAS_RT);

            // Out of gas exception
            var gas = hasRuleTreeFunction.EstimateGasAsync(BlockchainEngineOwner).Result;
            // var gas = new Nethereum.Hex.HexTypes.HexBigInteger(1000000);

            bool bTreeAlreadyExists =
                hasRuleTreeFunction.CallAsync<bool>(BlockchainEngineOwner, gas, null, BlockchainEngine.SenderAddress).Result;

			if (!bTreeAlreadyExists)
			{
				moRulesEngine.Serialize(BlockchainEngineOwner,
										BlockchainEngine.Password,
										BlockchainEngine.SenderAddress,
										BlockchainEngine.ContractAddress,
										BlockchainEngine.ContractABI,
										null,
										msWeb3HttpUrl);
			}
        }

        public static void SetAttribute(WonkaProduct poTargetProduct, WonkaRefAttr poTargetAttr, string psTargetValue)
        {
            if (poTargetProduct.GetProductGroup(poTargetAttr.GroupId).GetRowCount() <= 0)
                poTargetProduct.GetProductGroup(poTargetAttr.GroupId).AppendRow();

            poTargetProduct.GetProductGroup(poTargetAttr.GroupId)[0][poTargetAttr.AttrId] = psTargetValue;
        }

        public virtual WonkaBizRuleTreeReport SimulateValidate(T instance) { return null; }

        public virtual bool Validate(T instance) 
        {
            bool bValid = true;

            var contract = GetContract();

            var callRuleTreeEvent = contract.GetEvent(CONST_EVENT_CALL_RULE_TREE);
            var callRuleSetEvent  = contract.GetEvent(CONST_EVENT_CALL_RULE_SET);
            var callRuleEvent     = contract.GetEvent(CONST_EVENT_CALL_RULE);

            var filterCRTAll = callRuleTreeEvent.CreateFilterAsync().Result;
            var filterCRSAll = callRuleSetEvent.CreateFilterAsync().Result;
            var filterCRAll  = callRuleEvent.CreateFilterAsync().Result;

            var gas = new Nethereum.Hex.HexTypes.HexBigInteger(1000000);

            var executeWithReportFunction = contract.GetFunction(CONST_CONTRACT_FUNCTION_EXEC_RPT);

            var ruleTreeReport = executeWithReportFunction.CallDeserializingToObjectAsync<WonkaRuleTreeReport>(BlockchainEngine.SenderAddress).Result;

            HandleEvents(callRuleTreeEvent, callRuleSetEvent, callRuleEvent, filterCRTAll, filterCRSAll, filterCRAll);

			if (ruleTreeReport.NumberOfRuleFailures <= 0)
			{
				bValid = true;
			}
			else
			{
				throw new WonkaValidatorException(ruleTreeReport);
			}

            return bValid;
        }

        #region Properties

        public string BlockchainEngineOwner { get; set; }

        public WonkaBlockchainEngine BlockchainEngine
        {
            get
            {
                return moBlockchainEngine;
            }

            set
            {
                moBlockchainEngine = value;

				if (moRulesEngine != null)
				{
					moRulesEngine.SetDefaultStdOps(moBlockchainEngine.Password, this.msWeb3HttpUrl);
				}
            }
        }

        #endregion
    }

	public class WonkaBlockchainEngine
	{
		public string SenderAddress { get; set; }

		public string Password { get; set; }

		public string ContractAddress { get; set; }

		public string ContractABI { get; set; }
	}

	public class CallRuleTreeEvent
	{
		[Parameter("address", "ruler", 1, true)]
		public string TreeOwner { get; set; }
	}

	public class CallRuleSetEvent
	{
		[Parameter("address", "ruler", 1, true)]
		public string TreeOwner { get; set; }

		[Parameter("bytes32", "tmpRuleSetId", 2, true)]
		public string RuleSetId { get; set; }
	}

	public class CallRuleEvent
	{
		[Parameter("address", "ruler", 1, true)]
		public string TreeOwner { get; set; }

		[Parameter("bytes32", "ruleSetId", 2, true)]
		public string RuleSetId { get; set; }

		[Parameter("bytes32", "ruleId", 3, true)]
		public string RuleId { get; set; }

		[Parameter("uint", "ruleType", 4, false)]
		public uint RuleType { get; set; }
	}

	[FunctionOutput]
	public class WonkaRuleTreeReport
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

}
