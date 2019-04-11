using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.ABI.Model;
using Nethereum.Web3.Accounts;

using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;

using WonkaBre;
using WonkaBre.Readers;
using WonkaBre.RuleTree;
using WonkaEth.Contracts;
using WonkaRef;

namespace WonkaEth.Extensions
{
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

        public const string CONST_EVENT_CALL_RULE_TREE = "CallRuleTree";
        public const string CONST_EVENT_CALL_RULE_SET  = "CallRuleSet";
        public const string CONST_EVENT_CALL_RULE      = "CallRule";
        public const string CONST_EVENT_RULE_SET_ERROR = "RuleSetError";

        public const string CONST_CONTRACT_FUNCTION_EXEC         = "execute"; 
        public const string CONST_CONTRACT_FUNCTION_EXEC_RPT     = "executeWithReport"; 
        public const string CONST_CONTRACT_FUNCTION_GET_LAST_RPT = "getLastRuleReport";

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
            var sRuleTreeID = poEngine.DetermineRuleTreeChainID();

            var RuleTreeInfo = GetRuleTreeIndex(sRuleTreeID);

            if (RuleTreeInfo.RuleTreeOwner != psSenderAddress)
                throw new Exception("ERROR!  You are attempting to save a RuleTree with the ID(" + sRuleTreeID + "), which is already registered by a different owner.");

            string sCreateDateTime = RuleTreeInfo.CreationTime.ToString();
        }

        /// <summary>
        /// 
        /// This method will determine the RuleTree ID of the RuleTree held by the instance of the engine.
        /// 
        /// <param name="poEngine">The instance of an engine which contains the root node of the RuleTree</param>
        /// <returns>Provides the ID of the RuleTree contained by the engine</returns>
        /// </summary>
        public static string DetermineRuleTreeChainID(this WonkaBreRulesEngine poEngine)
        {
            return poEngine.RuleTreeRoot.DetermineRuleSetID(poEngine.RegistrationId);
        }

        /// <summary>
        /// 
        /// This method will determine the ID of the RuleSet meant for the blockchain.
        /// 
        /// <param name="poEngine">The instance of an engine which contains a node of the RuleTree</param>
        /// <param name="psRegistrationId">The Registration ID for the Registry on the blockchain</param>
        /// <returns>Provides the ID of the RuleTree for the blockchain</returns>
        /// </summary>
        public static string DetermineRuleSetID(this WonkaBreRuleSet poRootRuleSet, string psRegistrationId)
        {
            string sRuleTreeChainId = "";

            if (!String.IsNullOrEmpty(psRegistrationId))
                sRuleTreeChainId = psRegistrationId;
            else
                sRuleTreeChainId = poRootRuleSet.Description;

            if (sRuleTreeChainId.Length >= (CONST_CONTRACT_BYTE32_MAX - 4))
                sRuleTreeChainId = "Root" + sRuleTreeChainId.Replace(" ", "").Trim().Substring(0, 27);
            else
                sRuleTreeChainId = "Root" + sRuleTreeChainId.Replace(" ", "").Trim();

            return sRuleTreeChainId;
        }

        ///
        /// <summary>
        /// 
        /// This method will use Nethereum to obtain the XML (i.e., Wonka rules markup) of a RuleTree within the blockchain.
        /// 
        /// NOTE: Currently, we use a StringBuilder class to build the XML Document.  In the future, we should transition to
        /// using a XmlDocument and a XmlWriter.
        /// 
        /// <param name="psWeb3HttpUrl">The URL for the Ethereum node/client from which we will export the RuleTree</param>
        /// <returns>Returns the XML payload that represents a RuleTree within the blockchain</returns>
        /// </summary>
        public static string ExportXmlString(this WonkaRegistryItem poRegistryItem, string psWeb3HttpUrl = "")
        {
            var WonkaRegistry = WonkaRuleTreeRegistry.GetInstance();

            var sPassword = WonkaRegistry.RegistryPassword;
            var sABI      = poRegistryItem.HostContractABI;
            var account   = new Account(sPassword);

            Nethereum.Web3.Web3 web3 = null;
            if (!String.IsNullOrEmpty(psWeb3HttpUrl))
                web3 = new Nethereum.Web3.Web3(account, psWeb3HttpUrl);
            else   
                web3 = new Nethereum.Web3.Web3(account);
            
            var contract = web3.Eth.GetContract(sABI, poRegistryItem.HostContractAddress);

            StringBuilder sbExportXmlString = new StringBuilder("<?xml version=\"1.0\"?>\n<RuleTree>\n");

            var getRuleTreePropsFunction = contract.GetFunction("getRuleTreeProps");

            ExportRuleTreeProps TreeProps = getRuleTreePropsFunction.CallDeserializingToObjectAsync<ExportRuleTreeProps>(poRegistryItem.OwnerId).Result;

            sbExportXmlString.Append(ExportXmlString(contract, poRegistryItem.OwnerId, TreeProps.RootRuleSetName, 0));

            sbExportXmlString.Append("</RuleTree>\n");
            
            return sbExportXmlString.ToString();
        }

        ///
        /// <summary>
        /// 
        /// This method will use Nethereum to obtain the XML (i.e., Wonka rules markup) of a RuleSet within the blockchain.
        /// 
        /// NOTE: Currently, we use a StringBuilder class to build the XML Document.  In the future, we should transition to
        /// using a XmlDocument and a XmlWriter.
        /// 
        /// <returns>Returns the XML payload that represents a RuleSet within the blockchain</returns>
        /// </summary>
        private static string ExportXmlString(Contract poEngineContract, string psOwnerId, string psRuleSetName, uint pnStepLevel)
        {
            var RSNodeTag   = WonkaBreXmlReader.CONST_RS_FLOW_TAG;
            var RSNodeDesc  = WonkaBreXmlReader.CONST_RS_FLOW_DESC_ATTR;
            var RSLeafTag   = WonkaBreXmlReader.CONST_RS_VALID_TAG;
            var RSLeafMode  = WonkaBreXmlReader.CONST_RS_VALID_ERR_ATTR;
            var RuleCollTag = WonkaBreXmlReader.CONST_RULES_TAG;
            var LogicOp     = WonkaBreXmlReader.CONST_RULES_OP_ATTR;

            StringBuilder sbExportXmlString = new StringBuilder();
            StringBuilder sbTabSpaces       = new StringBuilder();
            StringBuilder sbCritSpaces      = new StringBuilder();
            StringBuilder sbRuleSpaces      = new StringBuilder();

            var getRuleSetPropsFunction   = poEngineContract.GetFunction("getRuleSetProps");
            var getRuleSetChildIdFunction = poEngineContract.GetFunction("getRuleSetChildId");
            var getRulePropsFunction      = poEngineContract.GetFunction("getRuleProps");

            ExportRuleSetProps SetProps = 
                getRuleSetPropsFunction.CallDeserializingToObjectAsync<ExportRuleSetProps>(psOwnerId, psRuleSetName).Result;

            for (uint x = 0; x < pnStepLevel; x++)
                sbTabSpaces.Append("    ");

            sbCritSpaces.Append(sbTabSpaces.ToString()).Append("    ");
            sbRuleSpaces.Append(sbCritSpaces.ToString()).Append("    ");

            if (!psRuleSetName.StartsWith("Root", StringComparison.CurrentCultureIgnoreCase))
            {
                if (SetProps.ChildRuleSetCount > 0)
                    sbExportXmlString.Append(sbTabSpaces.ToString()).Append("<" + RSNodeTag + " " + RSNodeDesc + "=\"" + SetProps.RuleSetDesc + "\" >\n");
                else
                {
                    string sMode = 
                        SetProps.SevereFailureFlag ? WonkaBreXmlReader.CONST_RS_VALID_ERR_SEVERE : WonkaBreXmlReader.CONST_RS_VALID_ERR_WARNING;
                    
                    sbExportXmlString.Append(sbTabSpaces.ToString()).Append("<" + RSLeafTag + " " + RSLeafMode + "=\"" + sMode + "\" >\n");
                }

                sbExportXmlString.Append(sbCritSpaces.ToString());
                sbExportXmlString.Append("<" + RuleCollTag + " " + LogicOp + "=\"" + (SetProps.AndOperatorFlag ? "AND" : "OR") + "\" >\n");

                if (SetProps.EvalRuleCount > 0)
                {
                    StringBuilder sbRulesBody = new StringBuilder();
                    sbRulesBody.Append(sbTabSpaces.ToString()).Append(sbTabSpaces.ToString()).Append(sbTabSpaces.ToString());

                    for (uint idx = 0; idx < SetProps.EvalRuleCount; idx++)
                    {
                        ExportRuleProps RuleProps =
                            getRulePropsFunction.CallDeserializingToObjectAsync<ExportRuleProps>(psOwnerId, psRuleSetName, true, idx).Result;

                        sbExportXmlString.Append(ExportXmlString(poEngineContract, RuleProps, sbRuleSpaces));
                    }
                }

                if (SetProps.AssertiveRuleCount > 0)
                {
                    StringBuilder sbRulesBody = new StringBuilder();
                    sbRulesBody.Append(sbTabSpaces.ToString()).Append(sbTabSpaces.ToString()).Append(sbTabSpaces.ToString());

                    for (uint idx = 0; idx < SetProps.AssertiveRuleCount; idx++)
                    {
                        ExportRuleProps RuleProps =
                            getRulePropsFunction.CallDeserializingToObjectAsync<ExportRuleProps>(psOwnerId, psRuleSetName, false, idx).Result;

                        sbExportXmlString.Append(ExportXmlString(poEngineContract, RuleProps, sbRuleSpaces));
                    }
                }

                sbExportXmlString.Append(sbCritSpaces.ToString());
                sbExportXmlString.Append("</" + RuleCollTag + ">\n");
            }

            // Now invoke the rulesets
            for (uint childIdx = 0; childIdx < SetProps.ChildRuleSetCount; childIdx++)
            {
                string nChildRuleSetId =
                    getRuleSetChildIdFunction.CallAsync<string>(psOwnerId, psRuleSetName, childIdx).Result;

                sbExportXmlString.Append(ExportXmlString(poEngineContract, psOwnerId, nChildRuleSetId, pnStepLevel + 1));
            }

            if (!psRuleSetName.StartsWith("Root", StringComparison.CurrentCultureIgnoreCase))
            {
                if (SetProps.ChildRuleSetCount > 0)
                    sbExportXmlString.Append(sbTabSpaces.ToString()).Append("</" + RSNodeTag + ">\n");
                else
                    sbExportXmlString.Append(sbTabSpaces.ToString()).Append("</" + RSLeafTag + ">\n");
            }

            return sbExportXmlString.ToString();
        }

        ///
        /// <summary>
        /// 
        /// This method will use Nethereum to obtain the XML (i.e., Wonka rules markup) of a Rule within the blockchain.
        /// 
        /// NOTE: Currently, we use a StringBuilder class to build the XML Document.  In the future, we should transition to
        /// using a XmlDocument and a XmlWriter.
        /// 
        /// <returns>Returns the XML payload that represents a Rule within the blockchain</returns>
        /// </summary>
        public static string ExportXmlString(Contract poEngineContract, ExportRuleProps poRuleProps, StringBuilder poSpaces)
        {
            bool   bEvalRule      = true;
            string sOpName        = "";
            string sRuleValue     = poRuleProps.RuleValue;
            string sDelim         = WonkaBreXmlReader.CONST_RULE_TOKEN_VAL_DELIM;
            string sSingleQuote   = "'";

            string sRuleTagFormat = 
                "{0}<" + WonkaBreXmlReader.CONST_RULE_TAG + " " + WonkaBreXmlReader.CONST_RULE_ID_ATTR + "=\"{1}\">(N.{2}) {3} {4}</eval>\n";

            if (poRuleProps.RuleType == (uint) CONTRACT_RULE_TYPES.LESS_THAN_RULE)
                sOpName = WonkaBreXmlReader.CONST_AL_LT;
            else if (poRuleProps.RuleType == (uint) CONTRACT_RULE_TYPES.EQUAL_TO_RULE)
                sOpName = WonkaBreXmlReader.CONST_AL_EQ;
            else if (poRuleProps.RuleType == (uint) CONTRACT_RULE_TYPES.GREATER_THAN_RULE)
                sOpName = WonkaBreXmlReader.CONST_AL_GT;
            else if (poRuleProps.RuleType == (uint) CONTRACT_RULE_TYPES.POPULATED_RULE)
                sOpName = WonkaBreXmlReader.CONST_BASIC_OP_POP;
            else if (poRuleProps.RuleType == (uint) CONTRACT_RULE_TYPES.IN_DOMAIN_RULE)
                sOpName = WonkaBreXmlReader.CONST_BASIC_OP_IN;
            else
            {
                bEvalRule = false;

                if (poRuleProps.RuleType == (uint) CONTRACT_RULE_TYPES.ASSIGN_RULE)
                    sOpName = WonkaBreXmlReader.CONST_BASIC_OP_ASSIGN;
                else if (poRuleProps.RuleType == (uint) CONTRACT_RULE_TYPES.ARITH_OP_SUM)
                    sOpName = WonkaBreXmlReader.CONST_BASIC_OP_ASSIGN_SUM;
                else if (poRuleProps.RuleType == (uint) CONTRACT_RULE_TYPES.ARITH_OP_DIFF)
                    sOpName = WonkaBreXmlReader.CONST_BASIC_OP_ASSIGN_DIFF;
                else if (poRuleProps.RuleType == (uint) CONTRACT_RULE_TYPES.ARITH_OP_PROD)
                    sOpName = WonkaBreXmlReader.CONST_BASIC_OP_ASSIGN_PROD;
                else if (poRuleProps.RuleType == (uint) CONTRACT_RULE_TYPES.ARITH_OP_QUOT)
                    sOpName = WonkaBreXmlReader.CONST_BASIC_OP_ASSIGN_QUOT;
                else if (poRuleProps.RuleType == (uint) CONTRACT_RULE_TYPES.CUSTOM_OP_RULE)
                {
                    List<string> CustomOpArgs = new List<string>(poRuleProps.CustomOpArgs);
                    CustomOpArgs.RemoveAll(x => x == "dummyValue");

                    sOpName    = poRuleProps.RuleValue;
                    sRuleValue = string.Join(sDelim, CustomOpArgs);
                }
            }

            if (bEvalRule && poRuleProps.NotOpFlag)
                sOpName = "NOT " + sOpName;

            if (poRuleProps.RuleType == (uint) CONTRACT_RULE_TYPES.IN_DOMAIN_RULE)
            {
                if (sRuleValue.Contains(sDelim))
                {
                    StringBuilder sbNewValue = new StringBuilder();

                    string[] asValueList = sRuleValue.Split(new char[1] { ',' });
                    foreach (string sTmpValue in asValueList)
                    {
                        if (sbNewValue.Length > 0)
                            sbNewValue.Append(sDelim);

                        sbNewValue.Append(sSingleQuote).Append(sTmpValue).Append(sSingleQuote);
                    }

                    sRuleValue = sbNewValue.ToString();
                }
                else
                    sRuleValue = "'" + poRuleProps.RuleValue + "'";
            }

            if (!String.IsNullOrEmpty(sRuleValue))
                sRuleValue = "(" + sRuleValue + ")";

            return String.Format(sRuleTagFormat, poSpaces.ToString(), poRuleProps.RuleName, poRuleProps.AttrName, sOpName, sRuleValue);
        }

        /// <summary>
        /// 
        /// This method will return a proxy to the Registry contract.
        /// 
        /// <returns>Provides the proxy to the Registry contract</returns>
        /// </summary>
        public static Nethereum.Contracts.Contract GetRegistryContract()
        {
            var WonkaRegistry = WonkaRuleTreeRegistry.GetInstance();
            var sPassword     = WonkaRegistry.RegistryPassword;
            var sABI          = WonkaRegistry.RegistryAbi;
            var sContractAddr = WonkaRegistry.RegistryContractAddress;

            var account = new Account(sPassword);

            Nethereum.Web3.Web3 web3 = null;
            if (!String.IsNullOrEmpty(WonkaRegistry.RegistryWeb3HttpUrl))
                web3 = new Nethereum.Web3.Web3(account, WonkaRegistry.RegistryWeb3HttpUrl);
            else
                web3 = new Nethereum.Web3.Web3(account);

            var contract = web3.Eth.GetContract(sABI, sContractAddr);

            return contract;
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
            var contract = GetRegistryContract();

            var getRuleTreeIndexFunction = contract.GetFunction("getRuleTreeIndex"); 

            return getRuleTreeIndexFunction.CallDeserializingToObjectAsync<RuleTreeRegistryIndex>(psRuleTreeId).Result;
        }

        public static RuleTreeReport InvokeOnChain(this WonkaBreRulesEngine poRulesEngine, Contract poWonkaContract, string psRuleTreeOwnerAddress)
        {
            var InvocationReport = new RuleTreeReport();
            var WonkaEvents      = new WonkaInvocationEvents(poWonkaContract);

            var executeFunction = poWonkaContract.GetFunction(CONST_CONTRACT_FUNCTION_EXEC);

            var gas = new Nethereum.Hex.HexTypes.HexBigInteger(CONST_MAX_GAS_COST_DEFAULT);

            var receiptInvocation = 
                executeFunction.SendTransactionAsync(psRuleTreeOwnerAddress, gas, null, psRuleTreeOwnerAddress).Result;

            // ruleTreeReport = executeGetLastReportFunction.CallDeserializingToObjectAsync<RuleTreeReport>().Result;

            // Finally, we handle any events that have been issued during the execution of the rules engine
            if (InvocationReport != null)
                WonkaEvents.HandleEvents(poRulesEngine, InvocationReport);
            
            return InvocationReport;
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
            var contract   = GetRegistryContract();
            var ruleTreeID = poEngine.DetermineRuleTreeChainID();

            var isRegisteredFunction = contract.GetFunction("isRuleTreeRegistered");

            return isRegisteredFunction.CallAsync<bool>(poEngine.DetermineRuleTreeChainID()).Result;
        }

        /// <summary>
        /// 
        /// This method will use Nethereum to call upon an instance of the Ethgine contract and 
        /// to create a RuleTree that will be owned by the Sender.
        /// 
        /// <param name="poEngine">The instance of an engine which contains the root node of the RuleTree</param>
        /// <param name="psRuleMasterAddress">The Ethereum address of the RulesMaster account (i.e., the one who owns this instance of the Wonka contract)</param>
        /// <param name="psPassword">The password for the psRuleMasterAddress</param>
        /// <param name="psSenderAddress">The Ethereum address of the sender (i.e., owner) account</param>
        /// <param name="psContractAddress">The address of the instance of the Ethgine contract</param>
        /// <param name="psAbi">The ABI interface for the Ethgine contract</param>
        /// <param name="psTransStateContractAddress">The address of the instance of the transaction state</param>
        /// <param name="psWeb3HttpUrl">The URL of the Ethereum node/client to which we will serialize the RuleTree</param>
        /// <returns>Indicates whether or not the RuleTree was created to the blockchain</returns>
        /// </summary>
        public static bool Serialize(this WonkaBreRulesEngine poEngine, 
                                                       string psRuleMasterAddress,
                                                       string psPassword, 
                                                       string psSenderAddress,
                                                       string psContractAddress, 
                                                       string psAbi, 
                                                       string psTransStateContractAddress = null,
                                                       string psWeb3HttpUrl = null)
        {
            bool bResult = true;

            WonkaBre.RuleTree.WonkaBreRuleSet treeRoot = poEngine.RuleTreeRoot;
            
            var account = new Account(psPassword);

            Nethereum.Web3.Web3 web3 = null;
            if (!String.IsNullOrEmpty(psWeb3HttpUrl))
                web3 = new Nethereum.Web3.Web3(account, psWeb3HttpUrl);
            else
                web3 = new Nethereum.Web3.Web3(account);

            var contractAddress = psContractAddress;
            var contract        = web3.Eth.GetContract(psAbi, contractAddress);

            if (poEngine.AddToRegistry)
            {
                if (!poEngine.IsRuleTreeRegistered())
                    poEngine.SerializeRegistryInfo(psSenderAddress, psContractAddress);
                else
                    poEngine.CompareRuleTrees(psSenderAddress);
            }

            treeRoot.SerializeTreeRoot(contract, psRuleMasterAddress, psSenderAddress, poEngine.RegistrationId);

            if (poEngine.UsingOrchestrationMode)
                poEngine.SerializeOrchestrationInfo(contract, psRuleMasterAddress, psSenderAddress);

            if (!String.IsNullOrEmpty(psTransStateContractAddress)) 
            {
                if (poEngine.TransactionState != null)
                    poEngine.TransactionState.Serialize(contract, psRuleMasterAddress, psPassword, psSenderAddress, psTransStateContractAddress, psWeb3HttpUrl);
            }

            return bResult;
        }

        /// <summary>
        /// 
        /// This method will use Nethereum to call upon an instance of the Ethgine contract and 
        /// to establish the Attributes (i.e., data points) that our intended RuleTree will examine.
        /// 
        /// <param name="poInstance">The instance of an Environment which contains the Attributes that we will want to share with the Ethgine contract</param>
        /// <param name="psRuleMasterAddress">The Ethereum address of the RulesMaster account (i.e., the one who owns this instance of the Wonka contract)</param>
        /// <param name="psPassword">The password for the psRuleMasterAddress/param>
        /// <param name="psSenderAddress">The Ethereum address of the sender account</param>
        /// <param name="psContractAddress">The address of the instance of the Ethgine contract</param>
        /// <param name="psAbi">The ABI interface for the Ethgine contract</param>
        /// <param name="psWeb3HttpUrl">The URL of the Ethereum node/client to which we will serialize the RefEnvironment instance</param>
        /// <returns>Indicates whether or not the Attributes were submitted to the blockchain</returns>
        /// </summary>
        public static bool Serialize(this WonkaRefEnvironment poInstance, 
                                                       string psRuleMasterAddress, 
                                                       string psPassword,
                                                       string psSenderAddress,
                                                       string psContractAddress, 
                                                       string psAbi,
                                                       string psWeb3HttpUrl = null)
        {
            uint nAttrNum = 3;

            var account = new Account(psPassword);

            Nethereum.Web3.Web3 web3 = null;
            if (!String.IsNullOrEmpty(psWeb3HttpUrl))
                web3 = new Nethereum.Web3.Web3(account, psWeb3HttpUrl);
            else
                web3 = new Nethereum.Web3.Web3(account);

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
                        addAttrFunction.SendTransactionAsync(psRuleMasterAddress, gas, null, sAttrName, MaxLen, MaxNumVal, DefVal, IsString, IsNumeric).Result;
                }
            }

            return true;
        }

        /// <summary>
        /// 
        /// This method will use Nethereum to register a RuleTree in the blockchain's registry.
        /// 
        /// <returns>Indicates whether or not the registry info was submitted to the blockchain</returns>
        /// </summary>
        public static bool Serialize(this WonkaRegistryItem poRegistryItem)
        {
            var WonkaRegistry = WonkaRuleTreeRegistry.GetInstance();
            
            HashSet<string> SourcesAdded   = new HashSet<string>();
            HashSet<string> CustomOpsAdded = new HashSet<string>();

            string sGroveId  = "";
            int    nGroveIdx = 0;

            foreach (string TmpGroveId in poRegistryItem.RuleTreeGroveIds.Keys)
            {
                sGroveId  = TmpGroveId;
                nGroveIdx = poRegistryItem.RuleTreeGroveIds[sGroveId];

                break;
            }

            var contract = GetRegistryContract();

            uint nSecondsSinceEpoch = 0;

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
        /// This method will use Nethereum to serialize the transaction state into the blockchain's registry.
        /// 
        /// <param name="poTransState">The instance of the transaction state</param>
        /// <param name="poContract">The Ethgine contract in which we are adding the RuleTree</param>
        /// <param name="psRuleMasterAddress">The Ethereum address of the RulesMaster account (i.e., the one who owns this instance of the Wonka contract)</param>
        /// <param name="psSenderAddress">The Ethereum address of the sender account who owns the RuleTree</param>
        /// <param name="psTransStateContractAddress">The address of the instance of the blockchain contract that serves as the transaction state</param>
        /// <param name="psWeb3HttpUrl">The URL of the Ethereum node/client to which we will serialize the TransactionState</param>
        /// <returns>Indicates whether or not the transaction state was submitted to the blockchain</returns>
        /// </summary>
        public static bool Serialize(this WonkaBre.Permissions.ITransactionState poTransState,
                                                    Nethereum.Contracts.Contract poWonkaContract,
                                                                          string psRuleMasterAddress,
                                                                          string psPassword,
                                                                          string psSenderAddress,
                                                                          string psTransStateContractAddress,
                                                                          string psWeb3HttpUrl)
        {
            #region Set Trx State on Tree in the Chain

            var setTrxStateFunction = poWonkaContract.GetFunction("setTransactionState");

            var gas = setTrxStateFunction.EstimateGasAsync(psSenderAddress, psTransStateContractAddress).Result;

            var receiptSetTrxState =
                setTrxStateFunction.SendTransactionAsync(psRuleMasterAddress, gas, null, psSenderAddress, psTransStateContractAddress).Result;

            #endregion

            #region Now set all properties for this TrxState on the contract

            var sTrxStateContractAddr = psTransStateContractAddress;

            var TmpAssembly  = System.Reflection.Assembly.GetExecutingAssembly();
            var TmpResStream = TmpAssembly.GetManifestResourceStream("WonkaEth.Contracts.Ethereum.TransactionStateInterface.abi");

            string sTrxStateABI = "";
            using (var AbiReader = new System.IO.StreamReader(TmpResStream))
            {
                sTrxStateABI = AbiReader.ReadToEnd();
            }

            var account = new Account(psPassword);

            Nethereum.Web3.Web3 web3 = null;
            if (!String.IsNullOrEmpty(psWeb3HttpUrl))
                web3 = new Nethereum.Web3.Web3(account, psWeb3HttpUrl);
            else
                web3 = new Nethereum.Web3.Web3(account);

            var contract = web3.Eth.GetContract(sTrxStateABI, psTransStateContractAddress);

            var addConfirmFunction        = contract.GetFunction("addConfirmation");
            var getMinScoreFunction       = contract.GetFunction("getMinScoreRequirement");
            var hasConfirmedFunction      = contract.GetFunction("hasConfirmed");
            var revokeConfirmFunction     = contract.GetFunction("revokeConfirmation");
            var revokeAllConfirmsFunction = contract.GetFunction("revokeAllConfirmations");
            var setExecutorFunction       = contract.GetFunction("setExecutor");
            var setMinScoreFunction       = contract.GetFunction("setMinScoreRequirement");
            var setOwnerFunction          = contract.GetFunction("setOwner");

            // NOTE: Causes "out of gas" exception to be thrown?
            // gas = addRegistryItemFunction.EstimateGasAsync(etc,etc,etc).Result;
            gas = new Nethereum.Hex.HexTypes.HexBigInteger(100000);

            var nMinScore = poTransState.GetMinScoreRequirement();
            if (nMinScore > 0)
            {
                var setMinScoreRetVal =
                    setMinScoreFunction.SendTransactionAsync(psRuleMasterAddress, gas, null, nMinScore).Result;
            }

            HashSet<string> TrxStateExecutors = poTransState.GetExecutors();
            foreach (string sTmpExecutor in TrxStateExecutors)
            {
                var setExecutorRetVal =
                    setExecutorFunction.SendTransactionAsync(psRuleMasterAddress, gas, null, sTmpExecutor).Result;
            }

            var revokeAllConfirmsRetVal =
                revokeAllConfirmsFunction.SendTransactionAsync(psRuleMasterAddress, gas, null).Result;

            HashSet<string> TrxStateConfirmedList = poTransState.GetOwnersConfirmed();
            foreach (string sTmpConfirmed in TrxStateConfirmedList)
            {
                uint nOwnerWeight = poTransState.GetOwnerWeight(sTmpConfirmed);

                var setOwnerRetVal =
                    setOwnerFunction.SendTransactionAsync(psRuleMasterAddress, gas, null, sTmpConfirmed, nOwnerWeight).Result;

                var confirmRetVal =
                    addConfirmFunction.SendTransactionAsync(psRuleMasterAddress, gas, null, sTmpConfirmed).Result;
            }

            HashSet<string> TrxStateUnconfirmedList = poTransState.GetOwnersUnconfirmed();
            foreach (string sTmpUnconfirmed in TrxStateUnconfirmedList)
            {
                uint nOwnerWeight = poTransState.GetOwnerWeight(sTmpUnconfirmed);

                var setOwnerRetVal =
                    setOwnerFunction.SendTransactionAsync(psRuleMasterAddress, gas, null, sTmpUnconfirmed, nOwnerWeight).Result;

                var revokeRetVal =
                    revokeConfirmFunction.SendTransactionAsync(psRuleMasterAddress, gas, null, sTmpUnconfirmed).Result;
            }

            #endregion

            return true;
        }

        /// <summary>
        /// 
        /// This method will use Nethereum to call upon an instance of the Ethgine contract and 
        /// to set the Orchestration mode information.
        /// 
        /// <param name="poEngine">The instance of an engine which contains the Orchestration info</param>
        /// <param name="poContract">The Ethgine contract in which we are adding the RuleTree</param>
        /// <param name="psRuleMasterAddress">The Ethereum address of the RulesMaster account (i.e., the one who owns this instance of the Wonka contract)</param>
        /// <param name="psSenderAddress">The Ethereum address of the sender account</param>
        /// <returns>Indicates whether or not the Orchestration info was submitted to the blockchain</returns>
        /// </summary>
        private static bool SerializeOrchestrationInfo(this WonkaBreRulesEngine poEngine, Nethereum.Contracts.Contract poContract, string psRuleMasterAddress, string psSenderAddress)
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
                    setOrchModeFunction.SendTransactionAsync(psRuleMasterAddress, gas, null, true, defSrc).Result;

                foreach (string sTmpAttrId in poEngine.SourceMap.Keys)
                {
                    WonkaBreSource TmpSource = poEngine.SourceMap[sTmpAttrId];

                    // NOTE: Causes "out of gas" exception to be thrown?
                    // var gas = addSourceFunction.EstimateGasAsync("Something", "Something", "Something", "Something", "Something").Result;
                    var addSrcGas = new Nethereum.Hex.HexTypes.HexBigInteger(1000000);

                    if (!SourcesAdded.Contains(TmpSource.SourceId))
                    {
                        result =
                            addSourceFunction.SendTransactionAsync(psRuleMasterAddress, 
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
                            addCustomOpFunction.SendTransactionAsync(psRuleMasterAddress,
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
        /// <param name="psPassword">The password for the psRuleMasterAddress</param>
        /// <param name="psContractAddress">The address of the instance of the Ethgine contract</param>
        /// <returns>Indicates whether or not the Attributes were submitted to the blockchain</returns>
        /// </summary>
        private static bool SerializeRegistryInfo(this WonkaBreRulesEngine poEngine, string psSenderAddress, string psContractAddress)
        {
            string sRuleTreeId = poEngine.DetermineRuleTreeChainID();

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
            newRegistryItem.MinGasCost          = CONST_MIN_GAS_COST_DEFAULT;
            newRegistryItem.MaxGasCost          = CONST_MAX_GAS_COST_DEFAULT;
            newRegistryItem.RequiredAttributes  = RequiredAttributes;
            newRegistryItem.RuleTreeGroveIds    = new Dictionary<string, int>();

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
        /// <param name="poContract">The Ethgine contract in which we are adding the RuleTree</param>
        /// <param name="psRuleMasterAddress">The Ethereum address of the RulesMaster account (i.e., the one who owns this instance of the Wonka contract)</param>
        /// <param name="psSenderAddress">The Ethereum address of the sender account who owns the RuleTree</param>
        /// <param name="psRegistrationId">The alternate ID for the RuleTree to use when registering it within the Registry</param>
        /// <returns>Indicates whether or not all of the RuleTree's nodes were submitted to the blockchain</returns>
        /// </summary>
        private static bool SerializeTreeRoot(this WonkaBre.RuleTree.WonkaBreRuleSet poRuleSet, 
                                                        Nethereum.Contracts.Contract poContract, 
                                                                              string psRuleMasterAddress,
                                                                              string psSenderAddress, 
                                                                              string psRegistrationId)
        {
            var addRuleTreeFunction = poContract.GetFunction("addRuleTree");

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

            sRootName = poRuleSet.DetermineRuleSetID(psRegistrationId);

            var result =
                addRuleTreeFunction.SendTransactionAsync(psRuleMasterAddress, gas, null, psSenderAddress, sRootName, sDesc, severeFailFlag, andOpFlag, false).Result;

            poRuleSet.SerializeRules(poContract, psRuleMasterAddress, psSenderAddress, sRootName);

            foreach (WonkaBre.RuleTree.WonkaBreRuleSet TempChildRuleSet in poRuleSet.ChildRuleSets)
            {
                TempChildRuleSet.SerializeRuleSet(poContract, psRuleMasterAddress, psSenderAddress, sRootName);
            }

            return true;
        }

        /// <summary>
        /// 
        /// This method will use Nethereum to call upon an instance of the Ethgine contract and 
        /// to add another node for a RuleTree.
        /// 
        /// <param name="poRuleSet">The current node of the RuleTree that we are creating in the Ethgine contract</param>
        /// <param name="poContract">The Ethgine contract in which we are adding the RuleTree</param>
        /// <param name="psRuleMasterAddress">The Ethereum address of the RulesMaster account (i.e., the one who owns this instance of the Wonka contract)</param>
        /// <param name="psSenderAddress">The Ethereum address of the sender account who owns the RuleTree</param>
        /// <param name="psRSParentName">The parent node of the current node that we are adding to the RuleTree</param>
        /// <returns>Indicates whether or not the current node was submitted to the blockchain</returns>
        /// </summary>
        private static bool SerializeRuleSet(this WonkaBre.RuleTree.WonkaBreRuleSet poRuleSet,
                                                       Nethereum.Contracts.Contract poContract,
                                                                             string psRuleMasterAddress,
                                                                             string psSenderAddress, 
                                                                             string psRSParentName)
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
                addRuleSetFunction.SendTransactionAsync(psRuleMasterAddress, gas, null, psSenderAddress, sResultSetID, sDescription, psRSParentName, severeFailFlag, andOpFlag, false).Result;

            poRuleSet.SerializeRules(poContract, psRuleMasterAddress, psSenderAddress,  sResultSetID);

            foreach (WonkaBre.RuleTree.WonkaBreRuleSet TempChildRuleSet in poRuleSet.ChildRuleSets)
            {
                TempChildRuleSet.SerializeRuleSet(poContract, psRuleMasterAddress, psSenderAddress, sResultSetID);
            }

            return true;
        }

        /// <summary>
        /// 
        /// This method will use Nethereum to call upon an instance of the Ethgine contract and 
        /// to add all of the rules that belong to a RuleSet node of the RuleTree.
        /// 
        /// <param name="poRuleSet">The current node of the RuleTree whose rules we are creating in the Ethgine contract</param>
        /// <param name="poContract">The Ethgine contract in which we are adding the RuleTree</param>
        /// <param name="psRuleMasterAddress">The Ethereum address of the RulesMaster account (i.e., the one who owns this instance of the Wonka contract)</param>
        /// <param name="psSenderAddress">The Ethereum address of the sender account who owns the RuleTree</param>
        /// <param name="psRuleSetId">The name of the current node in the blockchain whose rules we are adding to the RuleTree</param>
        /// <returns>Indicates whether or not the rules of the current node were submitted to the blockchain</returns>
        /// </summary>
        private static bool SerializeRules(this WonkaBre.RuleTree.WonkaBreRuleSet poRuleSet, 
                                                     Nethereum.Contracts.Contract poContract, 
                                                                           string psRuleMasterAddress, 
                                                                           string psSenderAddress, 
                                                                           string psRuleSetId)
        {
            var addRuleTreeFunction     = poContract.GetFunction("addRule");
            var addCustomOpArgsFunction = poContract.GetFunction("addRuleCustomOpArgs");

            // NOTE: Caused exception to be thrown
            // var gas = addRuleTreeFunction.EstimateGasAsync(psSenderAddress, "SomeRSID", "SomeRuleName", "SomeAttrName", 0, "SomeVal", false, false).Result;
            var gas = new Nethereum.Hex.HexTypes.HexBigInteger(1500000);

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
                        addRuleTreeFunction.SendTransactionAsync(psRuleMasterAddress, gas, null, psSenderAddress, psRuleSetId, sRuleName, sAttrName, nRuleType, sValue, notFlag, passFlag).Result;
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
                        addRuleTreeFunction.SendTransactionAsync(psRuleMasterAddress, gas, null, psSenderAddress, psRuleSetId, sRuleName, sAttrName, nRuleType, sValue, notFlag, passFlag).Result;

                    if (TempRule.RuleType == RULE_TYPE.RT_CUSTOM_OP)
                    {
                        var result2 = addCustomOpArgsFunction.SendTransactionAsync(psRuleMasterAddress, gas, null, psSenderAddress, psRuleSetId, CustomOpArgs[0], CustomOpArgs[1], CustomOpArgs[2], CustomOpArgs[3]).Result;
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
        public static WonkaEth.Orchestration.Init.OrchestrationInitData 
            TransformIntoOrchestrationInit(this WonkaEth.Init.WonkaEthInitialization poEthInitData, 
                                                                IMetadataRetrievable piMetadataSource = null,
                         Dictionary<string, WonkaBreXmlReader.ExecuteCustomOperator> poDelegateMap = null)

        {
            WonkaEth.Orchestration.Init.OrchestrationInitData OrchInitData = new WonkaEth.Orchestration.Init.OrchestrationInitData();

            OrchInitData.Web3HttpUrl              = poEthInitData.Web3HttpUrl;
            OrchInitData.AttributesMetadataSource = piMetadataSource;
            OrchInitData.BlockchainEngineOwner    = poEthInitData.BlockchainEngine.ContractOwner;

            OrchInitData.BlockchainEngine =
                new WonkaBreSource(poEthInitData.BlockchainEngine.ContractMarkupId,
                                   poEthInitData.BlockchainEngine.ContractSender,
                                   poEthInitData.BlockchainEngine.ContractPassword,
                                   poEthInitData.BlockchainEngine.ContractAddress,
                                   poEthInitData.BlockchainEngine.ContractABI,
                                   "", "", null);

            OrchInitData.TrxStateContractAddress = poEthInitData.BlockchainEngine.TrxStateContractAddress;

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
                    WonkaBreXmlReader.ExecuteCustomOperator poCustomOpDelegate = null;

                    if ((poDelegateMap != null) && poDelegateMap.ContainsKey(TmpSource.CustomOpContractMethod))
                        poCustomOpDelegate = poDelegateMap[TmpSource.CustomOpContractMethod];

                    WonkaBreSource TmpBreSource =
                        new WonkaBreSource(TmpSource.CustomOpMarkupId,
                                           TmpSource.ContractSender,
                                           TmpSource.ContractPassword,
                                           TmpSource.ContractAddress,
                                           TmpSource.ContractABI,
                                           poCustomOpDelegate,
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
