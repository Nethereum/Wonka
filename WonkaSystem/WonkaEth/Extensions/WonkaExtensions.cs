using System;
using System.Collections.Generic;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.ABI.Model;
using Nethereum.Web3.Accounts;

using Nethereum.Contracts;
using Nethereum.Geth;
using Nethereum.Web3;
using Nethereum.Web3.Accounts.Managed;
using Nethereum.Geth.RPC.Miner;
using Nethereum.RPC.Eth.DTOs;

using WonkaBre;
using WonkaBre.RuleTree;
using WonkaEth.Contracts;
using WonkaRef;

namespace WonkaEth.Extensions
{
    public class CallAddRuleTreeEvent
    {
        [Parameter("address", "ruler", 1, true)]
        public string TreeOwner { get; set; }
    }

    [FunctionOutput]
    public class RuleTreeRegistryIndex
    {
        [Parameter("bytes32", "rtid", 1)]
        public string RuleTreeId { get; set; }

        [Parameter("string", "rtdesc", 2)]
        public string RuleTreeDesc { get; set; }

        [Parameter("address", "hostaddr", 3)]
        public string RuleTreeHostEngineAddr { get; set; }

        [Parameter("address", "owner", 4)]
        public string RuleTreeOwner { get; set; }

        [Parameter("uint", "maxGasCost", 5)]
        public uint MaxGasCost { get; set; }

        [Parameter("uint", "createTime", 6)]
        public uint CreationEpochTime { get; set; }

        [Parameter("bytes32[]", "attributes", 7)]
        public List<string> RequiredAttributes { get; set; }

        public DateTime CreationTime 
        {
            get 
            {
                DateTime ct = new DateTime(1970, 1, 1);

                ct = ct.AddSeconds(CreationEpochTime);

                return ct;
            }
        }
    }

    /// <summary>
    /// 
    /// This extensions class provides the functionality to "serialize" a Wonka RuleTree into an
    /// into an Ethereum blockchain by using Nethereum to contact and call an already created instance 
    /// of the Ethgine contract.
    /// 
    /// </summary>
    public static class WonkaExtensions
    {
        private static int mnChildCounter = 1;
        private static int mnLeafCounter  = 1;

        public const string CONST_EVENT_CALL_RULE_TREE = "CallAddRuleTree";

        private const int CONST_CONTRACT_ATTR_NUM_ON_START = 3;
        private const int CONST_CONTRACT_BYTE32_MAX        = 32;
        private const int CONST_CUSTOM_OP_ARG_COUNT        = 4;
        private const int CONST_MIN_GAS_COST_DEFAULT       = 100000;
        private const int CONST_MAX_GAS_COST_DEFAULT       = 2000000;
        private const int CONST_MAX_RULE_TREE_ID_LEN       = 16;

        private static WonkaRefEnvironment moWonkaRevEnv = WonkaRefEnvironment.GetInstance();

        public enum CONTRACT_RULE_TYPES
        {
            EQUAL_TO_RULE = 0,
            LESS_THAN_RULE,
            GREATER_THAN_RULE,
            POPULATED_RULE,
            IN_DOMAIN_RULE,
            ASSIGN_RULE,
            ARITH_OP_SUM,
            ARITH_OP_DIFF,
            ARITH_OP_PROD,
            ARITH_OP_QUOT,
            CUSTOM_OP_RULE,
            MODE_MAX
        }

        /// <summary>
        /// 
        /// This method will use Nethereum to call upon the Registry and compare data about the RuleTree that is 
        /// held by 'poEngine'.
        /// 
        /// <param name="poEngine">The instance of an engine which contains the root node of the RuleTree</param>
        /// <param name="psSenderAddress">The Ethereum address of the sender (i.e., owner) account</param>
        /// <returns>None</returns>
        /// </summary>
        public static void CompareRuleTrees(this WonkaBreRulesEngine poEngine, string psSenderAddress)
        {
            var ruleTreeID = poEngine.DetermineRuleTreeID();

            var RuleTreeInfo = GetRuleTreeIndex(ruleTreeID);

            if (RuleTreeInfo.RuleTreeOwner != psSenderAddress)
                throw new Exception("ERROR!  You are attempting to save a RuleTree with an ID that's already been registered by a different owner.");

            string sCreateDateTime = RuleTreeInfo.CreationTime.ToString();
        }

        /// <summary>
        /// 
        /// This method will determine the RuleTree ID of the RuleTree held by the instance of the engine.
        /// 
        /// <param name="poEngine">The instance of an engine which contains the root node of the RuleTree</param>
        /// <returns>Provides the ID of the RuleTree contained by the engine</returns>
        /// </summary>
        private static string DetermineRuleTreeID(this WonkaBreRulesEngine poEngine)
        {
            string sRuleTreeId = poEngine.RuleTreeRoot.Description.Replace(" ", "");
            if (sRuleTreeId.Length > CONST_MAX_RULE_TREE_ID_LEN)
                sRuleTreeId = sRuleTreeId.Substring(0, CONST_MAX_RULE_TREE_ID_LEN);

            return sRuleTreeId;
        }

        /// <summary>
        /// 
        /// This method will return the metadata about a RuleTree that is registered within the blockchain.
        /// 
        /// <param name="psRuleTreeId">The ID of the RuleTree of interest</param>
        /// <returns>Provides the metadata (about the RuleTree) held within the registry</returns>
        /// </summary>
        public static RuleTreeRegistryIndex GetRuleTreeIndex(string psRuleTreeId)
        {
            var WonkaRegistry = WonkaRuleTreeRegistry.GetInstance();

            var sPassword     = WonkaRegistry.RegistryPassword;
            var sABI          = WonkaRegistry.RegistryAbi;
            var sContractAddr = WonkaRegistry.RegistryContractAddress;

            var account    = new Account(sPassword);
            var web3       = new Nethereum.Web3.Web3(account);
            var contract   = web3.Eth.GetContract(sABI, sContractAddr);

            var getRuleTreeIndexFunction = contract.GetFunction("getRuleTreeIndex"); 

            return getRuleTreeIndexFunction.CallDeserializingToObjectAsync<RuleTreeRegistryIndex>(psRuleTreeId).Result;
        }

        /// <summary>
        /// 
        /// This method will use Nethereum to call upon the Registry and detect whether or not the RuleTree
        /// has already been added to it.
        /// 
        /// <param name="poEngine">The instance of an engine which contains the root node of the RuleTree</param>
        /// <returns>Indicates whether or not the RuleTree has already been registered on the blockchain</returns>
        /// </summary>
        public static bool IsRuleTreeRegistered(this WonkaBreRulesEngine poEngine)
        {
            var WonkaRegistry = WonkaRuleTreeRegistry.GetInstance();

            var sPassword     = WonkaRegistry.RegistryPassword;
            var sABI          = WonkaRegistry.RegistryAbi;
            var sContractAddr = WonkaRegistry.RegistryContractAddress;

            var account    = new Account(sPassword);
            var web3       = new Nethereum.Web3.Web3(account);
            var contract   = web3.Eth.GetContract(sABI, sContractAddr);
            var ruleTreeID = poEngine.DetermineRuleTreeID();

            var isRegisteredFunction = contract.GetFunction("isRuleTreeRegistered");

            return isRegisteredFunction.CallAsync<bool>(poEngine.DetermineRuleTreeID()).Result;
        }

        /// <summary>
        /// 
        /// This method will use Nethereum to call upon an instance of the Ethgine contract and 
        /// to create a RuleTree that will be owned by the Sender.
        /// 
        /// <param name="poEngine">The instance of an engine which contains the root node of the RuleTree</param>
        /// <param name="psSenderAddress">The Ethereum address of the sender (i.e., owner) account</param>
        /// <param name="psPassword">The password for the sender</param>
        /// <param name="psContractAddress">The address of the instance of the Ethgine contract</param>
        /// <param name="psAbi">The ABI interface for the Ethgine contract</param>
        /// <returns>Indicates whether or not the RuleTree was created to the blockchain</returns>
        /// </summary>
        public static bool Serialize(this WonkaBreRulesEngine poEngine, string psSenderAddress, string psPassword, string psContractAddress, string psAbi)
        {
            bool bResult = true;

            WonkaBre.RuleTree.WonkaBreRuleSet treeRoot = poEngine.RuleTreeRoot;

            string sSenderAddress = psSenderAddress;

            var account = new Account(psPassword);

            var web3 = new Nethereum.Web3.Web3(account);

            var contractAddress = psContractAddress;

            var contract = web3.Eth.GetContract(psAbi, contractAddress);

            treeRoot.SerializeTreeRoot(sSenderAddress, contract);

            if (poEngine.UsingOrchestrationMode)
                poEngine.SerializeOrchestrationInfo(sSenderAddress, contract);

            if (poEngine.AddToRegistry && !poEngine.IsRuleTreeRegistered())
                poEngine.SerializeRegistryInfo(psSenderAddress, psContractAddress);
            else
                poEngine.CompareRuleTrees(psSenderAddress);

            return bResult;
        }

        /// <summary>
        /// 
        /// This method will use Nethereum to call upon an instance of the Ethgine contract and 
        /// to establish the Attributes (i.e., data points) that our intended RuleTree will examine.
        /// 
        /// <param name="poInstance">The instance of an Environment which contains the Attributes that we will want to share with the Ethgine contract</param>
        /// <param name="psSenderAddress">The Ethereum address of the sender account</param>
        /// <param name="psPassword">The password for the sender/param>
        /// <param name="psContractAddress">The address of the instance of the Ethgine contract</param>
        /// <param name="psAbi">The ABI interface for the Ethgine contract</param>
        /// <returns>Indicates whether or not the Attributes were submitted to the blockchain</returns>
        /// </summary>
        public static bool Serialize(this WonkaRefEnvironment poInstance, string psSenderAddress, string psPassword, string psContractAddress, string psAbi)
        {
            uint nAttrNum = 3;

            var account = new Account(psPassword);

            var web3 = new Nethereum.Web3.Web3(account);

            var contract = web3.Eth.GetContract(psAbi, psContractAddress);

            var getAttrNumFunction = contract.GetFunction("getNumberOfAttributes");
            var addAttrFunction    = contract.GetFunction("addAttribute");

            nAttrNum = getAttrNumFunction.CallAsync<uint>().Result;

            if (nAttrNum <= CONST_CONTRACT_ATTR_NUM_ON_START)
            {
                foreach (WonkaRefAttr TempAttr in poInstance.AttrCache)
                {
                    var sAttrName = "";

                    if (TempAttr.AttrName.Length > CONST_CONTRACT_BYTE32_MAX)
                        sAttrName = TempAttr.AttrName.Trim().Replace(" ", "").Substring(0, 31);
                    else
                        sAttrName = TempAttr.AttrName.Trim().Replace(" ", "");

                    uint   MaxLen    = (uint)TempAttr.MaxLength;
                    uint   MaxNumVal = 999999; // TempAttr.MaxValue;
                    string DefVal    = !String.IsNullOrEmpty(TempAttr.DefaultValue) ? TempAttr.DefaultValue : "";
                    bool   IsString  = !TempAttr.IsNumeric;
                    bool   IsNumeric = TempAttr.IsNumeric;

                    // NOTE: Caused exception to be thrown
                    // var gas = addAttrFunction.EstimateGasAsync("SomeAttr", 0, 0, "SomeVal", false, false).Result;
                    var gas = new Nethereum.Hex.HexTypes.HexBigInteger(1000000);

                    var receiptAddAttribute =
                        addAttrFunction.SendTransactionAsync(psSenderAddress, gas, null, sAttrName, MaxLen, MaxNumVal, DefVal, IsString, IsNumeric).Result;
                }
            }

            return true;
        }

        ///
        /// <summary>
        /// 
        /// This method will use Nethereum to register a RuleTree in the blockchain's registry.
        /// 
        /// <returns>Indicates whether or not the registry info was submitted to the blockchain</returns>
        /// </summary>
        public static bool Serialize(this WonkaRegistryItem poRegistryItem)
        {
            HashSet<string> SourcesAdded   = new HashSet<string>();
            HashSet<string> CustomOpsAdded = new HashSet<string>();

            var WonkaRegistry = WonkaRuleTreeRegistry.GetInstance();

            var sPassword     = WonkaRegistry.RegistryPassword;
            var sABI          = WonkaRegistry.RegistryAbi;
            var sContractAddr = WonkaRegistry.RegistryContractAddress;

            string sGroveId  = "";
            int    nGroveIdx = 0;

            foreach (string TmpGroveId in poRegistryItem.RuleTreeGroveIds.Keys)
            {
                sGroveId  = TmpGroveId;
                nGroveIdx = poRegistryItem.RuleTreeGroveIds[sGroveId];

                break;
            }

            var account  = new Account(sPassword);
            var web3     = new Nethereum.Web3.Web3(account);
            var contract = web3.Eth.GetContract(sABI, sContractAddr);

            uint nSecondsSinceEpoch = 0;

            // function addRuleTreeIndex(address ruler, bytes32 rsId, string desc, bytes32 ruleTreeGrpId, uint grpIdx, address host, uint minCost, uint maxCost, address[] associates, bytes32[] attributes, bytes32[] ops, uint createTime) public {
            var addRegistryItemFunction = contract.GetFunction("addRuleTreeIndex");

            // NOTE: Causes "out of gas" exception to be thrown?
            // var gas = addRegistryItemFunction.EstimateGasAsync(etc,etc,etc).Result;
            var gas = new Nethereum.Hex.HexTypes.HexBigInteger(1000000);

            if (poRegistryItem.creationTime > 0)
                nSecondsSinceEpoch = poRegistryItem.creationTime;
            else
            {
                TimeSpan span = DateTime.UtcNow - new DateTime(1970, 1, 1);

                nSecondsSinceEpoch = Convert.ToUInt32(span.TotalSeconds);
            }

            var result =
                addRegistryItemFunction.SendTransactionAsync(WonkaRegistry.RegistrySender, 
                                                             gas, 
                                                             null, 
                                                             WonkaRegistry.RegistrySender,
                                                             poRegistryItem.RuleTreeId,
                                                             poRegistryItem.Description,
                                                             sGroveId,
                                                             nGroveIdx,
                                                             poRegistryItem.HostContractAddress,
                                                             poRegistryItem.MinGasCost,
                                                             poRegistryItem.MaxGasCost,
                                                             poRegistryItem.AssociateContractAddresses,
                                                             poRegistryItem.RequiredAttributes,
                                                             poRegistryItem.UsedCustomOps,
                                                             nSecondsSinceEpoch).Result;
            
            return true;
        }

        /// <summary>
        /// 
        /// This method will use Nethereum to call upon an instance of the Ethgine contract and 
        /// to set the Orchestration mode information.
        /// 
        /// <param name="poEngine">The instance of an engine which contains the Orchestration info</param>
        /// <param name="psSenderAddress">The Ethereum address of the sender account</param>
        /// <param name="poContract">The Ethgine contract in which we are adding the RuleTree</param>
        /// <returns>Indicates whether or not the Orchestration info was submitted to the blockchain</returns>
        /// </summary>
        private static bool SerializeOrchestrationInfo(this WonkaBreRulesEngine poEngine, string psSenderAddress, Nethereum.Contracts.Contract poContract)
        {
            var addSourceFunction   = poContract.GetFunction("addSource");
            var addCustomOpFunction = poContract.GetFunction("addCustomOp");
            var setOrchModeFunction = poContract.GetFunction("setOrchestrationMode");

            HashSet<string> SourcesAdded   = new HashSet<string>();
            HashSet<string> CustomOpsAdded = new HashSet<string>();

            if (poEngine.UsingOrchestrationMode)
            {
                string result = "";
                string defSrc = (!String.IsNullOrEmpty(poEngine.DefaultSource)) ? poEngine.DefaultSource : "";

                // NOTE: Causes "out of gas" exception to be thrown?
                // var gas = setOrchModeFunction.EstimateGasAsync(true).Result;
                var gas = new Nethereum.Hex.HexTypes.HexBigInteger(1000000);

                result =
                    setOrchModeFunction.SendTransactionAsync(psSenderAddress, gas, null, true, defSrc).Result;

                foreach (string sTmpAttrId in poEngine.SourceMap.Keys)
                {
                    WonkaBreSource TmpSource = poEngine.SourceMap[sTmpAttrId];

                    // NOTE: Causes "out of gas" exception to be thrown?
                    // var gas = addSourceFunction.EstimateGasAsync("Something", "Something", "Something", "Something", "Something").Result;
                    var addSrcGas = new Nethereum.Hex.HexTypes.HexBigInteger(1000000);

                    if (!SourcesAdded.Contains(TmpSource.SourceId))
                    {
                        result =
                            addSourceFunction.SendTransactionAsync(psSenderAddress, 
                                                                   addSrcGas, 
                                                                   null, 
                                                                   TmpSource.SourceId, 
                                                                   "ACT", 
                                                                   TmpSource.ContractAddress, 
                                                                   TmpSource.MethodName, 
                                                                   TmpSource.SetterMethodName).Result;

                        SourcesAdded.Add(TmpSource.SourceId);
                    }
                }

                foreach (string sCustomOpName in poEngine.CustomOpMap.Keys)
                {
                    WonkaBreSource TmpSource = poEngine.CustomOpMap[sCustomOpName];

                    // NOTE: Causes "out of gas" exception to be thrown?
                    // var gas = addSourceFunction.EstimateGasAsync("Something", "Something", "Something", "Something", "Something").Result;
                    var addSrcGas = new Nethereum.Hex.HexTypes.HexBigInteger(1500000);

                    if (!CustomOpsAdded.Contains(TmpSource.SourceId))
                    {
                        result =
                            addCustomOpFunction.SendTransactionAsync(psSenderAddress,
                                                                     addSrcGas,
                                                                     null,
                                                                     TmpSource.SourceId,
                                                                     "ACT",
                                                                     TmpSource.ContractAddress,
                                                                     TmpSource.CustomOpMethodName).Result;

                        CustomOpsAdded.Add(TmpSource.SourceId);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 
        /// This method will use Nethereum to call upon an instance of the Registry contract and 
        /// to submit info about the RuleTree contained within the instance of the engine.
        /// 
        /// <param name="poEngine">The instance of an engine/RuleTree whose info we wish to write to the Registry on the blockchain</param>
        /// <param name="psSenderAddress">The Ethereum address of the sender account</param>
        /// <param name="psPassword">The password for the sender</param>
        /// <param name="psContractAddress">The address of the instance of the Ethgine contract</param>
        /// <returns>Indicates whether or not the Attributes were submitted to the blockchain</returns>
        /// </summary>
        private static bool SerializeRegistryInfo(this WonkaBreRulesEngine poEngine, string psSenderAddress, string psContractAddress)
        {
            string sRuleTreeId = poEngine.DetermineRuleTreeID();

            HashSet<string> RequiredAttributes = new HashSet<string>();
            HashSet<string> ContractAssociates = new HashSet<string>();
            HashSet<string> CustomOpsList      = new HashSet<string>();

            poEngine.RefEnvHandle.AttrCache.ForEach(x => RequiredAttributes.Add(x.AttrName));

            foreach (string sTmpAttrName in poEngine.SourceMap.Keys)
                ContractAssociates.Add(poEngine.SourceMap[sTmpAttrName].ContractAddress);

            foreach (string sTmpCustomOp in poEngine.CustomOpMap.Keys)
                CustomOpsList.Add(sTmpCustomOp);

            WonkaRegistryItem newRegistryItem = new WonkaRegistryItem();

            newRegistryItem.RuleTreeId  = sRuleTreeId;
            newRegistryItem.Description = poEngine.RuleTreeRoot.Description;

            newRegistryItem.HostContractAddress = psContractAddress;
            newRegistryItem.OwnerId             = psSenderAddress;
            newRegistryItem.RuleTreeGroveIds    = new Dictionary<string, int>();
            newRegistryItem.MinGasCost          = CONST_MIN_GAS_COST_DEFAULT;
            newRegistryItem.MaxGasCost          = CONST_MAX_GAS_COST_DEFAULT;
            newRegistryItem.RequiredAttributes  = RequiredAttributes;

            if (!String.IsNullOrEmpty(poEngine.GroveId) && (poEngine.GroveIndex > 0))
                newRegistryItem.RuleTreeGroveIds[poEngine.GroveId] = (int) poEngine.GroveIndex;

            newRegistryItem.AssociateContractAddresses = ContractAssociates;
            newRegistryItem.UsedCustomOps              = CustomOpsList;

            newRegistryItem.Serialize();

            return true;
        }

        /// <summary>
        /// 
        /// This method will use Nethereum to call upon an instance of the Ethgine contract and 
        /// to add the root node for a RuleTree (as well as the rest of the RuleTree).
        /// 
        /// <param name="poRuleSet">The root node of the RuleTree that we are creating in the Ethgine contract</param>
        /// <param name="psSenderAddress">The Ethereum address of the sender account who owns the RuleTree</param>
        /// <param name="poContract">The Ethgine contract in which we are adding the RuleTree</param>
        /// <returns>Indicates whether or not all of the RuleTree's nodes were submitted to the blockchain</returns>
        /// </summary>
        private static bool SerializeTreeRoot(this WonkaBre.RuleTree.WonkaBreRuleSet poRuleSet, string psSenderAddress, Nethereum.Contracts.Contract poContract)
        {
            var addRuleTreeFunction = poContract.GetFunction("addRuleTree");

            var callAddRuleTreeEvent = poContract.GetEvent(CONST_EVENT_CALL_RULE_TREE);

            var filterARTAll = callAddRuleTreeEvent.CreateFilterAsync().Result;

            var gas = addRuleTreeFunction.EstimateGasAsync(psSenderAddress, "SomeName", "SomeDesc", true, true, true).Result;

            //
            // base fee exceeds gas limit?
            //var receiptSetValueOnRecord = 
            //    setValueOnRecordFunction.SendTransactionAndWaitForReceiptAsync(sSenderAddress, null, sSenderAddress, TempAttr.AttrName, sAttrValue).Result;
            //

            var sRootName      = "";
            var sDesc          = "Root Node of the Tree";
            var severeFailFlag = (poRuleSet.ErrorSeverity == RULE_SET_ERR_LVL.ERR_LVL_SEVERE);
            var andOpFlag      = (poRuleSet.RulesEvalOperator == RULE_OP.OP_AND);

            if (poRuleSet.Description.Length > CONST_CONTRACT_BYTE32_MAX)
                sRootName = poRuleSet.Description.Replace(" ", "").Trim().Substring(0, 27) + "Root";
            else
                sRootName = poRuleSet.Description.Replace(" ", "").Trim() + "Root";

            var result =
                addRuleTreeFunction.SendTransactionAsync(psSenderAddress, gas, null, psSenderAddress, sRootName, sDesc, severeFailFlag, andOpFlag, false).Result;

            poRuleSet.SerializeRules(psSenderAddress, poContract, sRootName);

            foreach (WonkaBre.RuleTree.WonkaBreRuleSet TempChildRuleSet in poRuleSet.ChildRuleSets)
            {
                TempChildRuleSet.SerializeRuleSet(psSenderAddress, poContract, sRootName);
            }

            var ruleTreeLog = callAddRuleTreeEvent.GetFilterChanges<CallAddRuleTreeEvent>(filterARTAll).Result;

            if (ruleTreeLog.Count > 0)
                System.Console.WriteLine("RuleTree Added that Belongs to : (" + ruleTreeLog[0].Event.TreeOwner + ")");

            return true;
        }

        /// <summary>
        /// 
        /// This method will use Nethereum to call upon an instance of the Ethgine contract and 
        /// to add another node for a RuleTree.
        /// 
        /// <param name="poRuleSet">The current node of the RuleTree that we are creating in the Ethgine contract</param>
        /// <param name="psSenderAddress">The Ethereum address of the sender account who owns the RuleTree</param>
        /// <param name="poContract">The Ethgine contract in which we are adding the RuleTree</param>
        /// <param name="psRSParentName">The parent node of the current node that we are adding to the RuleTree</param>
        /// <returns>Indicates whether or not the current node was submitted to the blockchain</returns>
        /// </summary>
        private static bool SerializeRuleSet(this WonkaBre.RuleTree.WonkaBreRuleSet poRuleSet, string psSenderAddress, Nethereum.Contracts.Contract poContract, string psRSParentName)
        {
            var addRuleSetFunction = poContract.GetFunction("addRuleSet");

            // NOTE: Causes exception to be thrown?
            // var gas = addRuleSetFunction.EstimateGasAsync(psSenderAddress, "SomeName", "SomeDesc", "SomeParentName", true, true, true).Result;
            var gas = new Nethereum.Hex.HexTypes.HexBigInteger(1000000);

            var sResultSetID   = "";
            var sDescription   = "";
            var severeFailFlag = (poRuleSet.ErrorSeverity == RULE_SET_ERR_LVL.ERR_LVL_SEVERE);
            var andOpFlag      = (poRuleSet.RulesEvalOperator == RULE_OP.OP_AND);

            if (String.IsNullOrEmpty(poRuleSet.Description) && (poRuleSet.ChildRuleSets.Count == 0))
            {
                if (psRSParentName.Length >= 24)
                    sResultSetID = psRSParentName.Substring(0, 24) + "_Leaf" + mnLeafCounter++;
                else
                    sResultSetID = psRSParentName + "_Leaf" + mnLeafCounter++;

                sDescription = "None Available";
            }
            else if (String.IsNullOrEmpty(poRuleSet.Description))
            {
                if (psRSParentName.Length >= 25)
                    sResultSetID = psRSParentName.Substring(0, 25) + "_Child" + mnChildCounter++;
                else
                    sResultSetID = psRSParentName + "_Child" + mnChildCounter++;

                sDescription = "None Available";
            }
            else if (poRuleSet.Description.Length > CONST_CONTRACT_BYTE32_MAX)
            {
                sResultSetID = poRuleSet.Description.Replace(" ", "").Trim().Substring(0, 31);
                sDescription = poRuleSet.Description;
            }
            else
            {
                sResultSetID = poRuleSet.Description.Replace(" ", "").Trim();
                sDescription = poRuleSet.Description;
            }

            var result =
                addRuleSetFunction.SendTransactionAsync(psSenderAddress, gas, null, psSenderAddress, sResultSetID, sDescription, psRSParentName, severeFailFlag, andOpFlag, false).Result;

            poRuleSet.SerializeRules(psSenderAddress, poContract, sResultSetID);

            foreach (WonkaBre.RuleTree.WonkaBreRuleSet TempChildRuleSet in poRuleSet.ChildRuleSets)
            {
                TempChildRuleSet.SerializeRuleSet(psSenderAddress, poContract, sResultSetID);
            }

            return true;
        }

        /// <summary>
        /// 
        /// This method will use Nethereum to call upon an instance of the Ethgine contract and 
        /// to add all of the rules that belong to a RuleSet node of the RuleTree.
        /// 
        /// <param name="poRuleSet">The current node of the RuleTree whose rules we are creating in the Ethgine contract</param>
        /// <param name="psSenderAddress">The Ethereum address of the sender account who owns the RuleTree</param>
        /// <param name="poContract">The Ethgine contract in which we are adding the RuleTree</param>
        /// <param name="psRuleSetId">The name of the current node in the blockchain whose rules we are adding to the RuleTree</param>
        /// <returns>Indicates whether or not the rules of the current node were submitted to the blockchain</returns>
        /// </summary>
        private static bool SerializeRules(this WonkaBre.RuleTree.WonkaBreRuleSet poRuleSet, string psSenderAddress, Nethereum.Contracts.Contract poContract, string psRuleSetId)
        {
            var addRuleTreeFunction     = poContract.GetFunction("addRule");
            var addCustomOpArgsFunction = poContract.GetFunction("addRuleCustomOpArgs");

            // NOTE: Caused exception to be thrown
            // var gas = addRuleTreeFunction.EstimateGasAsync(psSenderAddress, "SomeRSID", "SomeRuleName", "SomeAttrName", 0, "SomeVal", false, false).Result;
            var gas = new Nethereum.Hex.HexTypes.HexBigInteger(1500000);

            // NOTE: ADD RULES HERE
            foreach (WonkaBre.RuleTree.WonkaBreRule TempRule in poRuleSet.EvaluativeRules)
            {
                var    sRuleName    = "";
                var    sAltRuleName = "Rule" + TempRule.RuleId;
                var    sAttrName    = TempRule.TargetAttribute.AttrName;
                uint   nRuleType    = 0;
                string sValue       = "";
                var    passFlag     = TempRule.IsPassive;
                var    notFlag      = TempRule.NotOperator;

                if (TempRule.RuleType == RULE_TYPE.RT_ARITH_LIMIT)
                {
                    var ArithLimitRule = 
                            (WonkaBre.RuleTree.RuleTypes.ArithmeticLimitRule) TempRule;

                    if (ArithLimitRule.MinValue <= -999999.0)
                    {
                        nRuleType = (uint)CONTRACT_RULE_TYPES.LESS_THAN_RULE;
                        sValue    = Convert.ToString(ArithLimitRule.MaxValue);
                    }
                    else if (ArithLimitRule.MaxValue >= 999999.0)
                    {
                        nRuleType = (uint)CONTRACT_RULE_TYPES.GREATER_THAN_RULE;
                        sValue    = Convert.ToString(ArithLimitRule.MinValue);
                    }
                    else
                    {
                        nRuleType = (uint)CONTRACT_RULE_TYPES.EQUAL_TO_RULE;
                        sValue    = Convert.ToString(ArithLimitRule.MinValue);
                    }

                    sAltRuleName = "Limit(" + sValue + ") for -> [" + 
                        ((TempRule.TargetAttribute.AttrName.Length > 8) ? TempRule.TargetAttribute.AttrName.Substring(0,8) : TempRule.TargetAttribute.AttrName);
                }
                else if (TempRule.RuleType == RULE_TYPE.RT_POPULATED)
                {
                    nRuleType = (uint) CONTRACT_RULE_TYPES.POPULATED_RULE;

                    sAltRuleName = "Populated check for -> [" +
                        ((TempRule.TargetAttribute.AttrName.Length > 8) ? TempRule.TargetAttribute.AttrName.Substring(0, 8) : TempRule.TargetAttribute.AttrName);                        
                }
                else if (TempRule.RuleType == RULE_TYPE.RT_DOMAIN)
                {
                    var DomainRule =
                        (WonkaBre.RuleTree.RuleTypes.DomainRule) TempRule;
                        
                    nRuleType = (uint) CONTRACT_RULE_TYPES.IN_DOMAIN_RULE;

                    foreach (string sTempVal in DomainRule.DomainValueProps.Keys)
                    {
                        if (!String.IsNullOrEmpty(sValue)) sValue += ",";

                        sValue += sTempVal;    
                    }

                    string sDomainAbbr = (sValue.Length > 8) ? sValue.Substring(0, 8) + "..." : sValue;
                    sAltRuleName = "Domain(" + sDomainAbbr + ") for [" +
                        ((TempRule.TargetAttribute.AttrName.Length > 13) ? TempRule.TargetAttribute.AttrName.Substring(0, 13) : TempRule.TargetAttribute.AttrName);                        
                }

                if (!String.IsNullOrEmpty(TempRule.DescRuleId))
                    sRuleName = TempRule.DescRuleId;
                else
                {
                    if (sAltRuleName.Length > CONST_CONTRACT_BYTE32_MAX)
                        sAltRuleName = sAltRuleName.Substring(0, CONST_CONTRACT_BYTE32_MAX - 1);

                    sRuleName = sAltRuleName;
                }

                // if ((nRuleType > 0) && !TempRule.NotOperator)
                if (nRuleType > 0)
                {
                    var result =
                        addRuleTreeFunction.SendTransactionAsync(psSenderAddress, gas, null, psSenderAddress, psRuleSetId, sRuleName, sAttrName, nRuleType, sValue, notFlag, passFlag).Result;
                }
                else 
                {
                    System.Console.WriteLine("ERROR!  This rule doesn't qualify for serialization!");    
                }
            }

            foreach (WonkaBre.RuleTree.WonkaBreRule TempRule in poRuleSet.AssertiveRules)
            {
                var    sRuleName    = "";
                var    sAltRuleName = "Rule" + TempRule.RuleId;
                var    sAttrName    = TempRule.TargetAttribute.AttrName;
                uint   nRuleType    = 0;
                string sValue       = "";
                var    notFlag      = TempRule.NotOperator;

                List<string> CustomOpArgs = new List<string>();

                // This is a legacy issue that will be addressed in the near future
                var passFlag = true; //TempRule.IsPassive;

                if (TempRule.RuleType == RULE_TYPE.RT_ASSIGNMENT)
                {
                    var AssignRule =
                        (WonkaBre.RuleTree.RuleTypes.AssignmentRule) TempRule;

                    nRuleType = (uint) CONTRACT_RULE_TYPES.ASSIGN_RULE;

                    sValue = AssignRule.AssignValue;

                    sAltRuleName = "Assign(" + sValue + ") for -> [" +
                        ((TempRule.TargetAttribute.AttrName.Length > 8) ? TempRule.TargetAttribute.AttrName.Substring(0, 8) : TempRule.TargetAttribute.AttrName);                        
                }
                else if (TempRule.RuleType == RULE_TYPE.RT_ARITHMETIC)
                {
                    var AssignArithmeticRule =
                        (WonkaBre.RuleTree.RuleTypes.ArithmeticRule) TempRule;

                    if (AssignArithmeticRule.OpType == ARITH_OP_TYPE.AOT_SUM)
                        nRuleType = (uint)CONTRACT_RULE_TYPES.ARITH_OP_SUM;
                    else if (AssignArithmeticRule.OpType == ARITH_OP_TYPE.AOT_DIFF)
                        nRuleType = (uint)CONTRACT_RULE_TYPES.ARITH_OP_DIFF;
                    else if (AssignArithmeticRule.OpType == ARITH_OP_TYPE.AOT_PROD)
                        nRuleType = (uint)CONTRACT_RULE_TYPES.ARITH_OP_PROD;                    
                    else if (AssignArithmeticRule.OpType == ARITH_OP_TYPE.AOT_QUOT)
                        nRuleType = (uint)CONTRACT_RULE_TYPES.ARITH_OP_QUOT;                    

                    if (nRuleType > 0)
                    {
                        foreach (string sTempVal in AssignArithmeticRule.DomainValueProps.Keys)
                        {
                            if (!String.IsNullOrEmpty(sValue)) sValue += ",";

                            sValue += sTempVal;
                        }

                        sAltRuleName = "Arithmetic Elements (" + sValue + ") for -> [" +
                            ((TempRule.TargetAttribute.AttrName.Length > 8) ? TempRule.TargetAttribute.AttrName.Substring(0, 8) : TempRule.TargetAttribute.AttrName);
                    }
                }
                else if (TempRule.RuleType == RULE_TYPE.RT_CUSTOM_OP)
                {
                    var CustomOpRule =
                        (WonkaBre.RuleTree.RuleTypes.CustomOperatorRule) TempRule;

                    nRuleType = (uint) CONTRACT_RULE_TYPES.CUSTOM_OP_RULE;

                    sValue = CustomOpRule.CustomOpName;

                    for (int idx = 0; idx < CONST_CUSTOM_OP_ARG_COUNT; ++idx)
                    {
                        if (idx < CustomOpRule.CustomOpPropArgs.Count)
                            CustomOpArgs.Add(CustomOpRule.CustomOpPropArgs[idx]);
                        else
                            CustomOpArgs.Add("dummyValue");
                    }

                    string sParamsAbbr = (sValue.Length > 8) ? sValue.Substring(0, 8) + "..." : sValue;
                    sAltRuleName = "Parameters(" + sParamsAbbr + ") for [" +
                        ((TempRule.TargetAttribute.AttrName.Length > 13) ? TempRule.TargetAttribute.AttrName.Substring(0, 13) : TempRule.TargetAttribute.AttrName);                        
                }

                if (!String.IsNullOrEmpty(TempRule.DescRuleId))
                    sRuleName = TempRule.DescRuleId;
                else
                {
                    if (sAltRuleName.Length > CONST_CONTRACT_BYTE32_MAX)
                        sAltRuleName = sAltRuleName.Substring(0, CONST_CONTRACT_BYTE32_MAX - 1);

                    sRuleName = sAltRuleName;
                }

                if (nRuleType > 0)
                {
                    var result =
                        addRuleTreeFunction.SendTransactionAsync(psSenderAddress, gas, null, psSenderAddress, psRuleSetId, sRuleName, sAttrName, nRuleType, sValue, notFlag, passFlag).Result;

                    if (TempRule.RuleType == RULE_TYPE.RT_CUSTOM_OP)
                    {
                        var result2 = addCustomOpArgsFunction.SendTransactionAsync(psSenderAddress, gas, null, psSenderAddress, psRuleSetId, CustomOpArgs[0], CustomOpArgs[1], CustomOpArgs[2], CustomOpArgs[3]).Result;
                    }
                }
                else 
                {
                    System.Console.WriteLine("ERROR!  This rule doesn't qualify for serialization!");    
                }
            }

            return true;
        }

        /// <summary>
        /// 
        /// This method will use transform the parameter 'poEthInitData' into an instance of OrchestrationInitData.
        /// 
        /// NOTE: This method does not set the metadata needed to enable validation within the .NET side.
        /// 
        /// <param name="poEthInitData">The initialization info that will be repackaged</param>
        /// <returns>The transformation of 'poEthInitData' into an instance of OrchestrationInitData</returns>
        /// </summary>
        public static WonkaEth.Orchestration.Init.OrchestrationInitData TransformIntoOrchestrationInit(this WonkaEth.Init.WonkaEthInitialization poEthInitData, IMetadataRetrievable piMetadataSource = null)
        {
            WonkaEth.Orchestration.Init.OrchestrationInitData OrchInitData = new Orchestration.Init.OrchestrationInitData();

            OrchInitData.AttributesMetadataSource = piMetadataSource;

            OrchInitData.BlockchainEngine =
                new WonkaBreSource(poEthInitData.BlockchainEngine.ContractMarkupId,
                                   poEthInitData.BlockchainEngine.ContractSender,
                                   poEthInitData.BlockchainEngine.ContractPassword,
                                   poEthInitData.BlockchainEngine.ContractAddress,
                                   poEthInitData.BlockchainEngine.ContractABI,
                                   "", "", null);

            OrchInitData.DefaultBlockchainDataSource =
                new WonkaBreSource(poEthInitData.DefaultValueRetrieval.ContractMarkupId,
                                   poEthInitData.DefaultValueRetrieval.ContractSender,
                                   poEthInitData.DefaultValueRetrieval.ContractPassword,
                                   poEthInitData.DefaultValueRetrieval.ContractAddress,
                                   poEthInitData.DefaultValueRetrieval.ContractABI,
                                   poEthInitData.DefaultValueRetrieval.ContractGetterMethod,
                                   poEthInitData.DefaultValueRetrieval.ContractSetterMethod,
                                   null);

            if ((poEthInitData.AttributeSourceList != null) && (poEthInitData.AttributeSourceList.Length > 0))
            {
                foreach (Init.WonkaEthSource TmpSource in poEthInitData.AttributeSourceList)
                {
                    WonkaBreSource TmpBreSource =
                        new WonkaBreSource(TmpSource.ContractMarkupId,
                                           TmpSource.ContractSender,
                                           TmpSource.ContractPassword,
                                           TmpSource.ContractAddress,
                                           TmpSource.ContractABI,
                                           TmpSource.ContractGetterMethod,
                                           TmpSource.ContractSetterMethod,
                                           null);

                    if (OrchInitData.BlockchainDataSources == null)
                        OrchInitData.BlockchainDataSources = new Dictionary<string, WonkaBreSource>();

                    OrchInitData.BlockchainDataSources[TmpSource.TargetAttrName] = TmpBreSource;              
                }
            }

            if ((poEthInitData.CustomOperatorList != null) && (poEthInitData.CustomOperatorList.Length > 0))
            {
                foreach (Init.WonkaEthSource TmpSource in poEthInitData.CustomOperatorList)
                {
                    WonkaBreSource TmpBreSource =
                        new WonkaBreSource(TmpSource.CustomOpMarkupId,
                                           TmpSource.ContractSender,
                                           TmpSource.ContractPassword,
                                           TmpSource.ContractAddress,
                                           TmpSource.ContractABI,
                                           null,
                                           TmpSource.CustomOpContractMethod);

                    if (OrchInitData.BlockchainCustomOpFunctions == null)
                        OrchInitData.BlockchainCustomOpFunctions = new Dictionary<string, WonkaBreSource>();

                    OrchInitData.BlockchainCustomOpFunctions[TmpSource.CustomOpMarkupId] = TmpBreSource;
                }
            }

            return OrchInitData;
        }
    }

}
