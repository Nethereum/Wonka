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

using WonkaBre;
using WonkaBre.Reporting;
using WonkaBre.RuleTree;
using WonkaRef;
using WonkaPrd;

using WonkaEth.Contracts;
using WonkaEth.Extensions;
using WonkaEth.Misc;

namespace WonkaEth.Orchestration
{
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

    public class OrchestrationInitData
    {
        public IMetadataRetrievable AttributesMetadataSource;

        public WonkaBreSource BlockchainEngine;

        public WonkaBreSource DefaultBlockchainDataSource;

        public Dictionary<string, WonkaBreSource> BlockchainDataSources;

        public Dictionary<string, WonkaBreSource> BlockchainCustomOpFunctions;    
    }

    public abstract class AbstractWonkaOrchestrator<T> where T : ICommand
    {
        public const string CONST_EVENT_CALL_RULE_TREE        = "CallRuleTree";
        public const string CONST_EVENT_CALL_RULE_SET         = "CallRuleSet";
        public const string CONST_EVENT_CALL_RULE             = "CallRule";

        public const string CONST_CONTRACT_FUNCTION_EXECUTE      = "execute";
        public const string CONST_CONTRACT_FUNCTION_EXEC_RPT     = "executeWithReport"; 
        public const string CONST_CONTRACT_FUNCTION_GET_LAST_RPT = "getLastRuleReport";
        public const string CONST_CONTRACT_FUNCTION_HAS_RT       = "hasRuleTree";

        public readonly StringBuilder msRulesContents;

        public readonly OrchestrationInitData moInitData;
        public readonly WonkaBreRulesEngine   moRulesEngine;

        public AbstractWonkaOrchestrator(T poCommand, StringBuilder psRules, OrchestrationInitData poOrchInitData)
        {
            msRulesContents = psRules;

            Init(poCommand, poOrchInitData);

            moInitData = poOrchInitData;

            moRulesEngine = new WonkaBreRulesEngine(msRulesContents, 
                                                    poOrchInitData.BlockchainDataSources, 
                                                    poOrchInitData.BlockchainCustomOpFunctions, 
                                                    poOrchInitData.AttributesMetadataSource);

            if (poOrchInitData.DefaultBlockchainDataSource != null)
                moRulesEngine.DefaultSource = poOrchInitData.DefaultBlockchainDataSource.SourceId;

            SerializeRulesEngineToBlockchain();
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

        protected Nethereum.Contracts.Contract GetContract(WonkaBreSource poBlockchainSource)
        {
            var account = new Account(poBlockchainSource.Password);

            var web3 = new Nethereum.Web3.Web3(account);

            var contract = web3.Eth.GetContract(poBlockchainSource.ContractABI, poBlockchainSource.ContractAddress);

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
            var ruleTreeLog = poRuleTreeEvent.GetFilterChanges<CallRuleTreeEvent>(rtFilter).Result;
            var ruleSetLog  = poRuleSetEvent.GetFilterChanges<CallRuleSetEvent>(rsFilter).Result;
            var ruleLog     = poRuleEvent.GetFilterChanges<CallRuleEvent>(rlFilter).Result;

            // var ruleTreeLog = callRuleTreeEvent.GetAllChanges<CQS.Validation.CallRuleTreeEvent>(filterCRTAll).Result;
            // var ruleSetLog  = callRuleSetEvent.GetAllChanges<CQS.Validation.CallRuleSetEvent>(filterCRSAll).Result;
            // var ruleLog     = callRuleSetEvent.GetAllChanges<CQS.Validation.CallRuleEvent>(filterCRAll).Result;

            /*
            for (int i = 0; i < 5; ++i)
            {
                System.Threading.Thread.Sleep(10000);

                ruleTreeLog = callRuleTreeEvent.GetFilterChanges<CQS.Validation.CallRuleTreeEvent>(filterCRTAll).Result;
                ruleSetLog  = callRuleSetEvent.GetFilterChanges<CQS.Validation.CallRuleSetEvent>(filterCRSAll).Result;
                ruleLog     = callRuleSetEvent.GetFilterChanges<CQS.Validation.CallRuleEvent>(filterCRAll).Result;
            }
            */

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

        private void Init(T poCommand, OrchestrationInitData poOrchInitData)
        {
            WonkaRefEnvironment WonkaRefEnv = null;

            if (poOrchInitData == null)
                throw new WonkaOrchestratorException("ERROR!  Initialization for orchestration has not been provided.");

            if (poOrchInitData.AttributesMetadataSource == null)
                throw new WonkaOrchestratorException("ERROR!  Initialization data for metadata retrieval has not been provided.");

            if ((poOrchInitData.BlockchainDataSources == null) || (poOrchInitData.BlockchainDataSources.Count == 0))
            {
                if (poOrchInitData.DefaultBlockchainDataSource != null) 
                {
                    Dictionary<string, WonkaBreSource> BlockchainDataSources = new Dictionary<string, WonkaBreSource>();

                    Dictionary<PropertyInfo, WonkaRefAttr> PropMap = poCommand.GetPropertyMap();

                    // Set Commentary Attributes
                    foreach (PropertyInfo TmpProperty in PropMap.Keys)
                    {
                        WonkaRefAttr TempAttr = PropMap[TmpProperty];

                        BlockchainDataSources[TempAttr.AttrName] = poOrchInitData.DefaultBlockchainDataSource;
                    }

                    poOrchInitData.BlockchainDataSources = BlockchainDataSources;
                }
            }

            if ((poOrchInitData.BlockchainDataSources == null) || (poOrchInitData.BlockchainDataSources.Count == 0))
                throw new WonkaOrchestratorException("ERROR!  Initialization for data retrieval metadata has not been provided.");

            try
            {
                WonkaRefEnv = WonkaRefEnvironment.GetInstance();
            }
            catch (Exception ex)
            {
                WonkaRefEnv = WonkaRefEnvironment.CreateInstance(false, poOrchInitData.AttributesMetadataSource);

                // NOTE: Should/could contract be deployed here along with metadata (i.e., Attributes)? 
            }
        }

        static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }

        public virtual bool Orchestrate(T instance)
        {
            bool bValid = true;

            var contract = GetContract(moInitData.BlockchainEngine);

            var callRuleTreeEvent = contract.GetEvent(CONST_EVENT_CALL_RULE_TREE);
            var callRuleSetEvent  = contract.GetEvent(CONST_EVENT_CALL_RULE_SET);
            var callRuleEvent     = contract.GetEvent(CONST_EVENT_CALL_RULE);

            var filterCRTAll = callRuleTreeEvent.CreateFilterAsync().Result;
            var filterCRSAll = callRuleSetEvent.CreateFilterAsync().Result;
            var filterCRAll  = callRuleEvent.CreateFilterAsync().Result;

            var gas = new Nethereum.Hex.HexTypes.HexBigInteger(2000000);

            var executeWithReportFunction    = contract.GetFunction(CONST_CONTRACT_FUNCTION_EXEC_RPT);
            var executeGetLastReportFunction = contract.GetFunction(CONST_CONTRACT_FUNCTION_GET_LAST_RPT);

            var receiptAddAttribute =
                executeWithReportFunction.SendTransactionAsync(moInitData.BlockchainEngine.SenderAddress, gas, null, moInitData.BlockchainEngine.SenderAddress).Result;

            var ruleTreeReport = executeGetLastReportFunction.CallDeserializingToObjectAsync<WonkaRuleTreeReport>().Result;

            HandleEvents(callRuleTreeEvent, callRuleSetEvent, callRuleEvent, filterCRTAll, filterCRSAll, filterCRAll);

            if (ruleTreeReport.NumberOfRuleFailures <= 0)
                bValid = true;
            else
                throw new WonkaOrchestratorException(ruleTreeReport);

            return bValid;
        }

        protected void SerializeRulesEngineToBlockchain()
        {                
            var contract = GetContract(moInitData.BlockchainEngine);

            var hasRuleTreeFunction = contract.GetFunction(CONST_CONTRACT_FUNCTION_HAS_RT);

            // Out of gas exception
            var gas = hasRuleTreeFunction.EstimateGasAsync(moInitData.BlockchainEngine.SenderAddress).Result;
            // var gas = new Nethereum.Hex.HexTypes.HexBigInteger(1000000);

            bool bTreeAlreadyExists =
                hasRuleTreeFunction.CallAsync<bool>(moInitData.BlockchainEngine.SenderAddress, gas, null, moInitData.BlockchainEngine.SenderAddress).Result;

            if (!bTreeAlreadyExists)
                    moRulesEngine.Serialize(moInitData.BlockchainEngine.SenderAddress, 
                                            moInitData.BlockchainEngine.Password, 
                                            moInitData.BlockchainEngine.ContractAddress, 
                                            moInitData.BlockchainEngine.ContractABI);
        }

        public static void SetAttribute(WonkaProduct poTargetProduct, WonkaRefAttr poTargetAttr, string psTargetValue)
        {
            if (poTargetProduct.GetProductGroup(poTargetAttr.GroupId).GetRowCount() <= 0)
                poTargetProduct.GetProductGroup(poTargetAttr.GroupId).AppendRow();

            poTargetProduct.GetProductGroup(poTargetAttr.GroupId)[0][poTargetAttr.AttrId] = psTargetValue;
        }

    }
}