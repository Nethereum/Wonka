using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3.Accounts;

using WonkaBre.RuleTree;
using WonkaRef;
using WonkaPrd;

using WonkaEth.Contracts;
using WonkaEth.Orchestration.BlockchainEvents;
using WonkaEth.Orchestration.BlockchainOutput;
using WonkaEth.Orchestration.Init;

namespace WonkaEth.Orchestration
{
    /// <summary>
    /// 
    /// This abstract Facade will know how to utilize an instance of ICommand data (especially by serializing/deserializing 
    /// to the blockchain) and then invoke the blockchain execution of a target RuleTree on the data. 
    /// 
    /// NOTE: If the RuleTree (i.e., parameter 'psRules') does not yet exist on the blockchain, this Facade will serialize
    /// the RuleTree to the blockchain during its construction. 
    /// 
    /// NOTE: The intended goal is to make this class the base class for AbstractWonkaOrchestrator.
    /// 
    /// </summary>
    public abstract class WonkaOrchestratorProxy<T> : IOrchestrate where T : ICommand
    {
        public const string CONST_EVENT_CALL_RULE_TREE = "CallRuleTree";
        public const string CONST_EVENT_CALL_RULE_SET  = "CallRuleSet";
        public const string CONST_EVENT_CALL_RULE      = "CallRule";

        public const string CONST_CONTRACT_FUNCTION_EXECUTE      = "execute";
        public const string CONST_CONTRACT_FUNCTION_EXEC_RPT     = "executeWithReport";
        public const string CONST_CONTRACT_FUNCTION_GET_LAST_RPT = "getLastRuleReport";
        public const string CONST_CONTRACT_FUNCTION_HAS_RT       = "hasRuleTree";

        public readonly OrchestrationInitData moInitData;

        public WonkaOrchestratorProxy(T poCommand, OrchestrationInitData poOrchInitData, string psGroveId = "", uint pnGroveIndex = 0)
        {
            Init(poCommand, poOrchInitData);

            moInitData = poOrchInitData;
        }

        public static void AssignPropertiesViaReflection(ICommand poCommand, Hashtable poDataValues)
        {
            PropertyInfo[] Props = poCommand.GetProperties();

            Dictionary<PropertyInfo, WonkaRefAttr> PropMap = poCommand.GetPropertyMap();

            // Set Commentary Attributes
            foreach (PropertyInfo TmpProperty in Props)
            {
                object oPropAttrValue = null;

                Type oAttrType = TmpProperty.PropertyType;
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

        public virtual void DeserializeRecordFromBlockchain(ICommand poCommand)
        {
            Hashtable DataValues = new Hashtable();

            Dictionary<string, Contract> Sources = new Dictionary<string, Contract>();

            foreach (string sAttrName in moInitData.BlockchainDataSources.Keys)
            {
                var contract = this.GetContract(moInitData.BlockchainDataSources[sAttrName]);

                Sources[moInitData.BlockchainDataSources[sAttrName].SourceId] = contract;
            }

            // Out of gas exception
            // var gas = setValueOnRecordFunction.EstimateGasAsync(sSenderAddress, "SomeAttr", "SomeValue").Result;
            var gas = new Nethereum.Hex.HexTypes.HexBigInteger(1000000);

            foreach (String sTempAttrName in moInitData.BlockchainDataSources.Keys)
            {
                WonkaBreSource TempSource = moInitData.BlockchainDataSources[sTempAttrName];

                string sSenderAddr = TempSource.SenderAddress;

                var contract = Sources[TempSource.SourceId];

                var getValueOnRecordFunction = contract.GetFunction(TempSource.MethodName);

                string sAttrValue = getValueOnRecordFunction.CallAsync<string>(sTempAttrName).Result;

                if (!String.IsNullOrEmpty(sAttrValue))
                    DataValues[sTempAttrName] = sAttrValue;
            }

            AssignPropertiesViaReflection(poCommand, DataValues);
        }

        public Nethereum.Contracts.Contract GetContract(WonkaBreSource poBlockchainSource)
        {
            var account = new Account(poBlockchainSource.Password);

            var web3 = new Nethereum.Web3.Web3(account);

            var contract = web3.Eth.GetContract(poBlockchainSource.ContractABI, poBlockchainSource.ContractAddress);

            return contract;
        }

        public static void GetPropertiesViaReflection(ICommand poCommand, Hashtable poDataValues)
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

        public virtual bool Orchestrate(T instance, bool pbSimulationMode = false)
        {
            bool bValid = true;

            bValid = Orchestrate((ICommand)instance, pbSimulationMode);

            return bValid;
        }

        public virtual bool Orchestrate(ICommand instance, bool pbSimulationMode)
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

            WonkaRuleTreeReport ruleTreeReport = new WonkaRuleTreeReport();

            if (pbSimulationMode)
            {
                // Now, we get a full report on the execution of the rules engine, including the possibility of any failures
                ruleTreeReport = executeWithReportFunction.CallDeserializingToObjectAsync<WonkaRuleTreeReport>(moInitData.BlockchainEngine.SenderAddress, gas, null, moInitData.BlockchainEngine.SenderAddress).Result;
            }
            else
            {
                // Next, we execute the rules engine within a transaction, so that the any persistence will actually change the state of the blockchain
                var receiptAddAttribute =
                    executeWithReportFunction.SendTransactionAsync(moInitData.BlockchainEngine.SenderAddress, gas, null, moInitData.BlockchainEngine.SenderAddress).Result;

                // Now, we get a full report on the execution of the rules engine, including the possibility of any failures
                ruleTreeReport = executeGetLastReportFunction.CallDeserializingToObjectAsync<WonkaRuleTreeReport>().Result;
            }

            // Finally, we handle any events that have been issued during the execution of the rules engine
            HandleEvents(callRuleTreeEvent, callRuleSetEvent, callRuleEvent, filterCRTAll, filterCRSAll, filterCRAll);

            if (ruleTreeReport.NumberOfRuleFailures <= 0)
                bValid = true;
            else
                throw new WonkaOrchestratorException(ruleTreeReport);

            return bValid;
        }

        public virtual void SerializeRecordToBlockchain(ICommand poCommand)
        {
            Hashtable DataValues = new Hashtable();

            GetPropertiesViaReflection(poCommand, DataValues);

            Dictionary<string, Contract> Sources = new Dictionary<string, Contract>();

            foreach (string sAttrName in moInitData.BlockchainDataSources.Keys)
            {
                var contract = this.GetContract(moInitData.BlockchainDataSources[sAttrName]);

                Sources[moInitData.BlockchainDataSources[sAttrName].SourceId] = contract;
            }

            // Out of gas exception
            // var gas = setValueOnRecordFunction.EstimateGasAsync(sSenderAddress, "SomeAttr", "SomeValue").Result;
            var gas = new Nethereum.Hex.HexTypes.HexBigInteger(1000000);

            foreach (String sTempAttrName in DataValues.Keys)
            {
                WonkaBreSource TempSource = moInitData.BlockchainDataSources[sTempAttrName];

                string sSenderAddr = TempSource.SenderAddress;
                string sAttrValue = (string)DataValues[sTempAttrName];

                var contract = Sources[TempSource.SourceId];

                var setValueOnRecordFunction = contract.GetFunction(TempSource.SetterMethodName);

                if (!String.IsNullOrEmpty(sAttrValue))
                {
                    var receiptSetValueOnRecord =
                        setValueOnRecordFunction.SendTransactionAsync(sSenderAddr, gas, null, sTempAttrName, sAttrValue).Result;
                }
            }
        }

        public static void SetAttribute(WonkaProduct poTargetProduct, WonkaRefAttr poTargetAttr, string psTargetValue)
        {
            if (poTargetProduct.GetProductGroup(poTargetAttr.GroupId).GetRowCount() <= 0)
                poTargetProduct.GetProductGroup(poTargetAttr.GroupId).AppendRow();

            poTargetProduct.GetProductGroup(poTargetAttr.GroupId)[0][poTargetAttr.AttrId] = psTargetValue;
        }

        public abstract bool ValidateCommand(T poCommand);

    }
}