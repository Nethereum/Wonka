using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.ABI.Model;
using Nethereum.Web3.Accounts;

using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;

using Wonka.BizRulesEngine;
using Wonka.BizRulesEngine.Readers;
using Wonka.BizRulesEngine.RuleTree;
using Wonka.Eth.Contracts;
using Wonka.MetaData;

namespace Wonka.Eth.Extensions
{
    /// <summary>
    /// 
    /// This extensions class provides the functionality to "serialize" a Wonka RuleTree into an
    /// into an Ethereum blockchain by using Nethereum to contact and call an already created instance 
    /// of the Ethgine contract.
    /// 
    /// </summary>
    public static class WonkaExtensionsAsync
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
        public const string CONST_CONTRACT_FUNCTION_HAS_RT       = "hasRuleTree";

        private const int CONST_CONTRACT_ATTR_NUM_ON_START = 3;
        private const int CONST_CONTRACT_BYTE32_MAX        = 32;
        private const int CONST_CUSTOM_OP_ARG_COUNT        = 4;
        private const int CONST_MIN_GAS_COST_DEFAULT       = 100000;
        private const int CONST_MID_GAS_COST_DEFAULT       = 1000000;
        private const int CONST_MAX_GAS_COST_DEFAULT       = 2000000;
        private const int CONST_MAX_RULE_TREE_ID_LEN       = 16;

        private const string CONST_BLOCK_NUM_OP_IND = "00000";

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
        public static async Task<bool> CompareRuleTreesAsync(this WonkaBizRulesEngine poEngine, string psSenderAddress)
        {
			bool bResult = true;

            var sRuleTreeID = poEngine.DetermineRuleTreeChainID();

            var RuleTreeInfo = await GetRuleTreeIndexAsync(sRuleTreeID).ConfigureAwait(false);

            if (RuleTreeInfo.RuleTreeOwner != psSenderAddress)
                throw new Exception("ERROR!  You are attempting to save a RuleTree with the ID(" + sRuleTreeID + "), which is already registered by a different owner.");

			return bResult;
        }

        /// <summary>
        /// 
        /// This method will determine whether or not a tree exists in the instance of the Wonka engine on the chain.  Currently,
        /// a RuleTree can only exist for one account, and the owner's account serves as the ID for the RuleTree.
        /// 
        /// <param name="poEngine">The instance of an engine which wraps around a RuleTree</param>
        /// <param name="poWonkaContract">The instance of a Wonka contract on the blockchain</param>
        /// <param name="psTreeOwnerAddress">Address of the owner of the RuleTree in this engine instance on the chain</param>
        /// <returns>Indicates whether or not the RuleTree exists</returns>
        /// </summary>
        public static async Task<bool> DoesTreeExistOnChainAsync(this WonkaBizRulesEngine poEngine, Contract poWonkaContract, string psTreeOwnerAddress)
        {
            var hasRuleTreeFunction = poWonkaContract.GetFunction(CONST_CONTRACT_FUNCTION_HAS_RT);

            var gas = hasRuleTreeFunction.EstimateGasAsync(psTreeOwnerAddress).Result;

            return await hasRuleTreeFunction.CallAsync<bool>(psTreeOwnerAddress, gas, null, psTreeOwnerAddress).ConfigureAwait(false);
        }


        /// <summary>
        /// 
        /// This method will execute a RuleTree that exists within an instance of the Wonka engine on the chain.  Currently,
        /// a RuleTree can only exist for one account, and the owner's account serves as the ID for the RuleTree.
        /// 
        /// <param name="poEngine">The instance of an engine which wraps around a RuleTree</param>
        /// <param name="poEngineInitProps">The properties of the Wonka instance on the blockchain</param>
        /// <param name="poReport">If not null, we will fill the report with the results of the RuleTree's invocation on the blockchain</param>
        /// <returns>Receipt hash of transaction</returns>
        /// </summary>
        public static async Task<string> ExecuteOnChainAsync(this WonkaBizRulesEngine poEngine, Wonka.Eth.Init.WonkaEthEngineInitialization poEngineInitProps, Wonka.Eth.Extensions.RuleTreeReport poReport = null)
        {
            var account = new Account(poEngineInitProps.EthPassword);

			Nethereum.Web3.Web3 web3 = null;

			if (!String.IsNullOrEmpty(poEngineInitProps.Web3HttpUrl))
			    web3 = new Nethereum.Web3.Web3(account, poEngineInitProps.Web3HttpUrl);
			else
				web3 = new Nethereum.Web3.Web3(account);

			var contractAddress = poEngineInitProps.RulesEngineContractAddress;

            var wonkaContract = web3.Eth.GetContract(poEngineInitProps.RulesEngineABI, contractAddress);

            var executeWithReportFunction = wonkaContract.GetFunction(CONST_CONTRACT_FUNCTION_EXEC_RPT);

            uint nMaxGas = poEngineInitProps.Engine.CalculateMaxGasEstimate();

            string trxHashRuleTreeInvocation = null;

            if (poEngineInitProps.EthRuleTreeOwnerAddress == poEngineInitProps.EthSenderAddress)
            {
                var RuleTreeReport =
                    await poEngine.InvokeOnChainAsync(wonkaContract, poEngineInitProps.EthRuleTreeOwnerAddress, nMaxGas).ConfigureAwait(false);

				trxHashRuleTreeInvocation = RuleTreeReport.TransactionHash;

				if (poReport != null)
                    poReport.Copy(RuleTreeReport);

				if ( (poReport.RuleSetFailures.Count > 0) && (poEngine.OnFailureTriggers.Count > 0) )
					poEngine.OnFailureTriggers.Where(x => (x != null)).ToList().ForEach(x => x.Execute());
				else if ( (poReport.RuleSetFailures.Count == 0) && (poEngine.OnSuccessTriggers.Count > 0) )
					poEngine.OnSuccessTriggers.Where(x => (x != null)).ToList().ForEach(x => x.Execute());
			}
            else
            {
                var gas = new Nethereum.Hex.HexTypes.HexBigInteger(nMaxGas);

                trxHashRuleTreeInvocation =
                    await executeWithReportFunction.SendTransactionAsync(poEngineInitProps.EthSenderAddress, gas, null, poEngineInitProps.EthRuleTreeOwnerAddress).ConfigureAwait(false);
            }

            return trxHashRuleTreeInvocation;
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
        public static async Task<string> ExportXmlStringAsync(this WonkaRegistryItem poRegistryItem, string psWeb3HttpUrl = "")
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

            ExportRuleTreeProps TreeProps =
                await getRuleTreePropsFunction.CallDeserializingToObjectAsync<ExportRuleTreeProps>(poRegistryItem.OwnerId).ConfigureAwait(false);

            string sMainXmlBody =
                await ExportXmlStringAsync(contract, poRegistryItem.OwnerId, TreeProps.RootRuleSetName, 0).ConfigureAwait(false);

            sbExportXmlString.Append(sMainXmlBody);

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
        private static async Task<string> ExportXmlStringAsync(Contract poEngineContract, string psOwnerId, string psRuleSetName, uint pnStepLevel)
        {
            var RSNodeTag   = WonkaBizRulesXmlReader.CONST_RS_FLOW_TAG;
            var RSNodeDesc  = WonkaBizRulesXmlReader.CONST_RS_FLOW_DESC_ATTR;
            var RSLeafTag   = WonkaBizRulesXmlReader.CONST_RS_VALID_TAG;
            var RSLeafMode  = WonkaBizRulesXmlReader.CONST_RS_VALID_ERR_ATTR;
            var RuleCollTag = WonkaBizRulesXmlReader.CONST_RULES_TAG;
            var LogicOp     = WonkaBizRulesXmlReader.CONST_RULES_OP_ATTR;

            StringBuilder sbExportXmlString = new StringBuilder();
            StringBuilder sbTabSpaces       = new StringBuilder();
            StringBuilder sbCritSpaces      = new StringBuilder();
            StringBuilder sbRuleSpaces      = new StringBuilder();

            var getRuleSetPropsFunction   = poEngineContract.GetFunction("getRuleSetProps");
            var getRuleSetChildIdFunction = poEngineContract.GetFunction("getRuleSetChildId");
            var getRulePropsFunction      = poEngineContract.GetFunction("getRuleProps");

            ExportRuleSetProps SetProps =
                await getRuleSetPropsFunction.CallDeserializingToObjectAsync<ExportRuleSetProps>(psOwnerId, psRuleSetName).ConfigureAwait(false);

            for (uint x = 0; x < pnStepLevel; x++)
                sbTabSpaces.Append("    ");

            sbCritSpaces.Append(sbTabSpaces.ToString()).Append("    ");
            sbRuleSpaces.Append(sbCritSpaces.ToString()).Append("    ");

            if (!psRuleSetName.StartsWith("Root", StringComparison.CurrentCultureIgnoreCase))
            {
                if (SetProps.ChildRuleSetCount > 0)
                {
                    sbExportXmlString.Append(sbTabSpaces.ToString()).Append("<" + RSNodeTag + " " + RSNodeDesc + "=\"" + SetProps.RuleSetDesc + "\" >\n");
                }
                else
                {
                    string sMode =
                        SetProps.SevereFailureFlag ? WonkaBizRulesXmlReader.CONST_RS_VALID_ERR_SEVERE : WonkaBizRulesXmlReader.CONST_RS_VALID_ERR_WARNING;

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
                            await getRulePropsFunction.CallDeserializingToObjectAsync<ExportRuleProps>(psOwnerId, psRuleSetName, true, idx).ConfigureAwait(false);

                        sbExportXmlString.Append(WonkaExtensions.ExportXmlString(poEngineContract, RuleProps, sbRuleSpaces));
                    }
                }

                if (SetProps.AssertiveRuleCount > 0)
                {
                    StringBuilder sbRulesBody = new StringBuilder();
                    sbRulesBody.Append(sbTabSpaces.ToString()).Append(sbTabSpaces.ToString()).Append(sbTabSpaces.ToString());

                    for (uint idx = 0; idx < SetProps.AssertiveRuleCount; idx++)
                    {
                        ExportRuleProps RuleProps =
                            await getRulePropsFunction.CallDeserializingToObjectAsync<ExportRuleProps>(psOwnerId, psRuleSetName, false, idx).ConfigureAwait(false);

                        sbExportXmlString.Append(WonkaExtensions.ExportXmlString(poEngineContract, RuleProps, sbRuleSpaces));
                    }
                }

                sbExportXmlString.Append(sbCritSpaces.ToString());
                sbExportXmlString.Append("</" + RuleCollTag + ">\n");
            }

            // Now invoke the rulesets
            for (uint childIdx = 0; childIdx < SetProps.ChildRuleSetCount; childIdx++)
            {
                string nChildRuleSetId =
                    await getRuleSetChildIdFunction.CallAsync<string>(psOwnerId, psRuleSetName, childIdx).ConfigureAwait(false);

                string sChildRuleSetXml =
                    await ExportXmlStringAsync(poEngineContract, psOwnerId, nChildRuleSetId, pnStepLevel + 1).ConfigureAwait(false);

                sbExportXmlString.Append(sChildRuleSetXml);
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

        /// <summary>
        /// 
        /// This method will return the metadata about a RuleTree that is registered within the blockchain.
        /// 
        /// <param name="psRuleTreeId">The ID of the RuleTree of interest</param>
        /// <returns>Provides the metadata (about the RuleTree) held within the registry</returns>
        /// </summary>
        public static async Task<RuleTreeRegistryIndex> GetRuleTreeIndexAsync(string psRuleTreeId)
        {
            var contract = WonkaExtensions.GetRegistryContract();

            var getRuleTreeIndexFunction = contract.GetFunction("getRuleTreeIndex");

            return await getRuleTreeIndexFunction.CallDeserializingToObjectAsync<RuleTreeRegistryIndex>(psRuleTreeId).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// This method will use Nethereum to execute the sibling RuleTree of a Wonka.NET instance.  This sibling should already exist on the chain.
        /// 
        /// <param name="poRulesEngine">The instance of the Wonka.NET whose sibling should exist already on an Ethereum client</param>
        /// <param name="poWonkaContract">The contract instance on the chain that should contain the sibling of 'poRulesEngine'</param>
        /// <param name="psRuleTreeOwnerAddress">The owner of the RuleTree on an Ethereum client</param>
        /// <param name="nSendTrxGas">The gas amount to use when invoking the RuleTree sibling (on the chain) of 'poRulesEngine'</param>
        /// <returns>Contains the detailed report of the RuleTree's execution on the chain</returns>
        /// </summary>
        public static async Task<RuleTreeReport> InvokeOnChainAsync(this WonkaBizRulesEngine poRulesEngine, Contract poWonkaContract, string psRuleTreeOwnerAddress, uint nSendTrxGas = 0)
        {
            var InvocationReport = new RuleTreeReport();
            var WonkaEvents      = new WonkaInvocationEvents(poWonkaContract);

            var executeFunction = poWonkaContract.GetFunction(CONST_CONTRACT_FUNCTION_EXEC);

            var gas = new Nethereum.Hex.HexTypes.HexBigInteger(CONST_MAX_GAS_COST_DEFAULT);
            if (nSendTrxGas > 0)
                gas = new Nethereum.Hex.HexTypes.HexBigInteger(nSendTrxGas);

            var receipt =
                await executeFunction.SendTransactionAndWaitForReceiptAsync(psRuleTreeOwnerAddress, gas, null, null, psRuleTreeOwnerAddress).ConfigureAwait(false);

            // Finally, we handle any events that have been issued during the execution of the rules engine
            if (InvocationReport != null)
            {			
				InvocationReport.TransactionHash      = receipt.TransactionHash;
                InvocationReport.InvokeTrxBlockNumber = receipt.BlockNumber;

                WonkaEvents.HandleEvents(poRulesEngine, InvocationReport);

                if (poRulesEngine.AllRuleSets != null)
                {
                    foreach (string sTmpCustomId in InvocationReport.RuleSetFailures)
                    {
                        WonkaBizRuleSet FoundRuleSet =
                            poRulesEngine.AllRuleSets.Where(x => x.CustomId == sTmpCustomId).FirstOrDefault();

                        if (!String.IsNullOrEmpty(FoundRuleSet.CustomId))
                            InvocationReport.RuleSetFailMessages[FoundRuleSet.CustomId] = FoundRuleSet.CustomFailureMsg;
                    }
                }
            }

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
        public static async Task<bool> IsRuleTreeRegisteredAsync(this WonkaBizRulesEngine poEngine)
        {
            var contract   = WonkaExtensions.GetRegistryContract();
            var ruleTreeID = poEngine.DetermineRuleTreeChainID();

            var isRegisteredFunction = contract.GetFunction("isRuleTreeRegistered");

            return await isRegisteredFunction.CallAsync<bool>(poEngine.DetermineRuleTreeChainID()).ConfigureAwait(false);
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
        public static async Task<bool> SerializeAsync(this WonkaBizRulesEngine poEngine, 
                                                                        string psRuleMasterAddress,
                                                                        string psPassword, 
                                                                        string psSenderAddress,
                                                                        string psContractAddress, 
                                                                        string psAbi, 
                                                                        string psTransStateContractAddress = null,
                                                                        string psWeb3HttpUrl = null)
        {
            bool bResult = true;

            WonkaBizRuleSet treeRoot = poEngine.RuleTreeRoot;
            
            var account = new Account(psPassword);

            Nethereum.Web3.Web3 web3 = null;
            if (!String.IsNullOrEmpty(psWeb3HttpUrl))
                web3 = new Nethereum.Web3.Web3(account, psWeb3HttpUrl);
            else
                web3 = new Nethereum.Web3.Web3(account);

            var contractAddress = psContractAddress;
            var contract        = web3.Eth.GetContract(psAbi, contractAddress);

            bool bTreeExists = await poEngine.DoesTreeExistOnChainAsync(contract, psRuleMasterAddress).ConfigureAwait(false);

            if (!bTreeExists)
			{
                /* 
                 * NOTE: We should probably make some alterations here, especially in the case of handling errors thrown by CompareRuleTreesAsync()
                 * 
				if (poEngine.AddToRegistry)
				{
                    if (!poEngine.IsRuleTreeRegistered())
                        await poEngine.SerializeRegistryInfoAsync(psRuleMasterAddress, psContractAddress).ConfigureAwait(false);
                    else
                        poEngine.CompareRuleTreesAsync(psRuleMasterAddress);
				}
                */

                await treeRoot.SerializeTreeRootAsync(contract, psRuleMasterAddress, psSenderAddress, poEngine.RegistrationId).ConfigureAwait(false);

                if (poEngine.UsingOrchestrationMode)
                    await poEngine.SerializeOrchestrationInfoAsync(contract, psRuleMasterAddress, psSenderAddress).ConfigureAwait(false);

				if (!String.IsNullOrEmpty(psTransStateContractAddress))
				{
                    if (poEngine.TransactionState != null)
                        await poEngine.TransactionState.SerializeAsync(contract, psRuleMasterAddress, psPassword, psSenderAddress, psTransStateContractAddress, psWeb3HttpUrl).ConfigureAwait(false);
				}
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
        public static async Task<bool> SerializeAsync(this WonkaRefEnvironment poInstance, 
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

            nAttrNum = await getAttrNumFunction.CallAsync<uint>().ConfigureAwait(false);

            if (nAttrNum <= CONST_CONTRACT_ATTR_NUM_ON_START)
            {
                foreach (WonkaRefAttr TempAttr in poInstance.AttrCache)
                {
                    var sAttrName = string.Empty;

                    if (TempAttr.AttrName.Length > CONST_CONTRACT_BYTE32_MAX)
                        sAttrName = TempAttr.AttrName.Trim().Replace(" ", string.Empty).Substring(0, 31);
                    else
                        sAttrName = TempAttr.AttrName.Trim().Replace(" ", string.Empty);

                    uint   MaxLen    = (uint)TempAttr.MaxLength;
                    uint   MaxNumVal = 999999; // TempAttr.MaxValue;
                    string DefVal    = !String.IsNullOrEmpty(TempAttr.DefaultValue) ? TempAttr.DefaultValue : string.Empty;
                    bool   IsString  = !TempAttr.IsNumeric;
                    bool   IsNumeric = TempAttr.IsNumeric;

                    // For now, this is a kludge in order to identify an Attribute that is a date
                    if (TempAttr.IsDate)
                        IsString = IsNumeric = true;

                    var gas = new Nethereum.Hex.HexTypes.HexBigInteger(1000000);

                    var receiptAddAttribute =
                        await addAttrFunction.SendTransactionAsync(psRuleMasterAddress, gas, null, sAttrName, MaxLen, MaxNumVal, DefVal, IsString, IsNumeric).ConfigureAwait(false);
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
        public static async Task<bool> SerializeAsync(this WonkaRegistryItem poRegistryItem)
        {
            var WonkaRegistry = WonkaRuleTreeRegistry.GetInstance();
            
            HashSet<string> SourcesAdded   = new HashSet<string>();
            HashSet<string> CustomOpsAdded = new HashSet<string>();

            string sGroveId  = string.Empty;
            int    nGroveIdx = 0;

            foreach (string TmpGroveId in poRegistryItem.RuleTreeGroveIds.Keys)
            {
                sGroveId  = TmpGroveId;
                nGroveIdx = poRegistryItem.RuleTreeGroveIds[sGroveId];

                break;
            }

            var contract = WonkaExtensions.GetRegistryContract();

            uint nSecondsSinceEpoch = 0;

            var addRegistryItemFunction = contract.GetFunction("addRuleTreeIndex");

            var gas = new Nethereum.Hex.HexTypes.HexBigInteger(1000000);

            if (poRegistryItem.creationTime > 0)
            {
                nSecondsSinceEpoch = poRegistryItem.creationTime;
            }
            else
            {
                TimeSpan span = DateTime.UtcNow - new DateTime(1970, 1, 1);

                nSecondsSinceEpoch = Convert.ToUInt32(span.TotalSeconds);
            }

            var result =
                await addRegistryItemFunction.SendTransactionAsync(WonkaRegistry.RegistrySender,
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
                                                                   nSecondsSinceEpoch).ConfigureAwait(false);
            
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
        public static async Task<bool> SerializeAsync(this Wonka.BizRulesEngine.Permissions.ITransactionState poTransState,
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
                await setTrxStateFunction.SendTransactionAsync(psRuleMasterAddress, gas, null, psSenderAddress, psTransStateContractAddress).ConfigureAwait(false);

            #endregion

            #region Now set all properties for this TrxState on the contract

            var sTrxStateContractAddr = psTransStateContractAddress;

            var TmpAssembly  = System.Reflection.Assembly.GetExecutingAssembly();
            var TmpResStream = TmpAssembly.GetManifestResourceStream("WonkaEth.Contracts.Ethereum.TransactionStateInterface.abi");

            string sTrxStateABI = string.Empty;
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
                    await setMinScoreFunction.SendTransactionAsync(psRuleMasterAddress, gas, null, nMinScore).ConfigureAwait(false);
            }

            HashSet<string> TrxStateExecutors = poTransState.GetExecutors();
            foreach (string sTmpExecutor in TrxStateExecutors)
            {
                var setExecutorRetVal =
                    await setExecutorFunction.SendTransactionAsync(psRuleMasterAddress, gas, null, sTmpExecutor).ConfigureAwait(false);
            }

            var revokeAllConfirmsRetVal =
                await revokeAllConfirmsFunction.SendTransactionAsync(psRuleMasterAddress, gas, null).ConfigureAwait(false);

            HashSet<string> TrxStateConfirmedList = poTransState.GetOwnersConfirmed();
            foreach (string sTmpConfirmed in TrxStateConfirmedList)
            {
                uint nOwnerWeight = poTransState.GetOwnerWeight(sTmpConfirmed);

                var setOwnerRetVal =
                    await setOwnerFunction.SendTransactionAsync(psRuleMasterAddress, gas, null, sTmpConfirmed, nOwnerWeight).ConfigureAwait(false);

                var confirmRetVal =
                    await addConfirmFunction.SendTransactionAsync(psRuleMasterAddress, gas, null, sTmpConfirmed).ConfigureAwait(false);
            }

            HashSet<string> TrxStateUnconfirmedList = poTransState.GetOwnersUnconfirmed();
            foreach (string sTmpUnconfirmed in TrxStateUnconfirmedList)
            {
                uint nOwnerWeight = poTransState.GetOwnerWeight(sTmpUnconfirmed);

                var setOwnerRetVal =
                    await setOwnerFunction.SendTransactionAsync(psRuleMasterAddress, gas, null, sTmpUnconfirmed, nOwnerWeight).ConfigureAwait(false);

                var revokeRetVal =
                    await revokeConfirmFunction.SendTransactionAsync(psRuleMasterAddress, gas, null, sTmpUnconfirmed).ConfigureAwait(false);
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
        private static async Task<bool> SerializeOrchestrationInfoAsync(this WonkaBizRulesEngine poEngine, Nethereum.Contracts.Contract poContract, string psRuleMasterAddress, string psSenderAddress)
        {
            var addSourceFunction   = poContract.GetFunction("addSource");
            var addCustomOpFunction = poContract.GetFunction("addCustomOp");
            var setOrchModeFunction = poContract.GetFunction("setOrchestrationMode");

            HashSet<string> SourcesAdded   = new HashSet<string>();
            HashSet<string> CustomOpsAdded = new HashSet<string>();

            if (poEngine.UsingOrchestrationMode)
            {
                string result = string.Empty;
                string defSrc = (!String.IsNullOrEmpty(poEngine.DefaultSource)) ? poEngine.DefaultSource : string.Empty;

                var gas = new Nethereum.Hex.HexTypes.HexBigInteger(1000000);

                result =
                    await setOrchModeFunction.SendTransactionAsync(psRuleMasterAddress, gas, null, true, defSrc).ConfigureAwait(false);

                foreach (string sTmpAttrId in poEngine.SourceMap.Keys)
                {
                    WonkaBizSource TmpSource = poEngine.SourceMap[sTmpAttrId];

                    // NOTE: Causes "out of gas" exception to be thrown?
                    // var gas = addSourceFunction.EstimateGasAsync("Something", "Something", "Something", "Something", "Something").Result;
                    var addSrcGas = new Nethereum.Hex.HexTypes.HexBigInteger(1000000);

                    if (!SourcesAdded.Contains(TmpSource.SourceId))
                    {
                        result =
                            await addSourceFunction.SendTransactionAsync(psRuleMasterAddress,
                                                                         addSrcGas,
                                                                         null,
                                                                         TmpSource.SourceId,
                                                                         "ACT",
                                                                         TmpSource.ContractAddress,
                                                                         TmpSource.MethodName,
                                                                         TmpSource.SetterMethodName).ConfigureAwait(false);

                        SourcesAdded.Add(TmpSource.SourceId);
                    }
                }

                foreach (string sCustomOpName in poEngine.CustomOpMap.Keys)
                {
                    WonkaBizSource TmpSource = poEngine.CustomOpMap[sCustomOpName];

                    var addSrcGas = new Nethereum.Hex.HexTypes.HexBigInteger(1500000);

                    if (!CustomOpsAdded.Contains(TmpSource.SourceId))
                    {
                        result =
                            await addCustomOpFunction.SendTransactionAsync(psRuleMasterAddress,
                                                                           addSrcGas,
                                                                           null,
                                                                           TmpSource.SourceId,
                                                                           "ACT",
                                                                           TmpSource.ContractAddress,
                                                                           TmpSource.CustomOpMethodName).ConfigureAwait(false);

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
        private static async Task<bool> SerializeRegistryInfoAsync(this WonkaBizRulesEngine poEngine, string psSenderAddress, string psContractAddress)
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

            await newRegistryItem.SerializeAsync().ConfigureAwait(false);

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
        private static async Task<bool> SerializeTreeRootAsync(this WonkaBizRuleSet poRuleSet, 
                                                       Nethereum.Contracts.Contract poContract, 
                                                                             string psRuleMasterAddress,
                                                                             string psSenderAddress, 
                                                                             string psRegistrationId)
        {
            var addRuleTreeFunction = poContract.GetFunction("addRuleTree");

            // NOTE: EstimateGasAsync() throws an exception
            // var gas = addRuleTreeFunction.EstimateGasAsync(psSenderAddress, "SomeName", "SomeDesc", true, true, true).Result;
            var gas = new Nethereum.Hex.HexTypes.HexBigInteger(CONST_MID_GAS_COST_DEFAULT);

            var sRootName      = string.Empty;
            var sDesc          = "Root Node of the Tree";
            var severeFailFlag = (poRuleSet.ErrorSeverity == RULE_SET_ERR_LVL.ERR_LVL_SEVERE);
            var andOpFlag      = (poRuleSet.RulesEvalOperator == RULE_OP.OP_AND);

            sRootName = poRuleSet.DetermineRuleSetID(psRegistrationId);

            var result =
                await addRuleTreeFunction.SendTransactionAsync(psRuleMasterAddress, gas, null, psSenderAddress, sRootName, sDesc, severeFailFlag, andOpFlag, false).ConfigureAwait(false);

            await poRuleSet.SerializeRulesAsync(poContract, psRuleMasterAddress, psSenderAddress, sRootName).ConfigureAwait(false);

            foreach (WonkaBizRuleSet TempChildRuleSet in poRuleSet.ChildRuleSets)
            {
                await TempChildRuleSet.SerializeRuleSetAsync(poContract, psRuleMasterAddress, psSenderAddress, sRootName).ConfigureAwait(false);
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
        private static async Task<bool> SerializeRuleSetAsync(this WonkaBizRuleSet poRuleSet,
                                                      Nethereum.Contracts.Contract poContract,
                                                                            string psRuleMasterAddress,
                                                                            string psSenderAddress, 
                                                                            string psRSParentName)
        {
            var addRuleSetFunction = poContract.GetFunction("addRuleSet");

            // NOTE: Causes exception to be thrown?
            // var gas = addRuleSetFunction.EstimateGasAsync(psSenderAddress, "SomeName", "SomeDesc", "SomeParentName", true, true, true).Result;
            var gas = new Nethereum.Hex.HexTypes.HexBigInteger(1000000);

            var sResultSetID   = string.Empty;
            var sDescription   = string.Empty;
            var severeFailFlag = (poRuleSet.ErrorSeverity == RULE_SET_ERR_LVL.ERR_LVL_SEVERE);
            var andOpFlag      = (poRuleSet.RulesEvalOperator == RULE_OP.OP_AND);

            if (!String.IsNullOrEmpty(poRuleSet.CustomId))
            {
                if (poRuleSet.CustomId.Length >= 32)
                    sResultSetID = psRSParentName.Substring(0, 32);
                else
                    sResultSetID = poRuleSet.CustomId;
            }
            else if (String.IsNullOrEmpty(poRuleSet.Description) && (poRuleSet.ChildRuleSets.Count == 0))
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
                sResultSetID = poRuleSet.Description.Replace(" ", string.Empty).Trim().Substring(0, 31);
                sDescription = poRuleSet.Description;
            }
            else
            {
                sResultSetID = poRuleSet.Description.Replace(" ", string.Empty).Trim();
                sDescription = poRuleSet.Description;
            }

            if (String.IsNullOrEmpty(poRuleSet.CustomId))
                poRuleSet.CustomId = sResultSetID;

            var result =
                await addRuleSetFunction.SendTransactionAsync(psRuleMasterAddress, gas, null, psSenderAddress, sResultSetID, sDescription, psRSParentName, severeFailFlag, andOpFlag, false).ConfigureAwait(false);

            await poRuleSet.SerializeRulesAsync(poContract, psRuleMasterAddress, psSenderAddress, sResultSetID).ConfigureAwait(false);

            foreach (WonkaBizRuleSet TempChildRuleSet in poRuleSet.ChildRuleSets)
            {
                await TempChildRuleSet.SerializeRuleSetAsync(poContract, psRuleMasterAddress, psSenderAddress, sResultSetID).ConfigureAwait(false);
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
        private static async Task<bool> SerializeRulesAsync(this WonkaBizRuleSet poRuleSet, 
                                                                      Nethereum.Contracts.Contract poContract, 
                                                                                            string psRuleMasterAddress, 
                                                                                            string psSenderAddress, 
                                                                                            string psRuleSetId)
        {
            var addRuleFunction         = poContract.GetFunction("addRule");
            var addCustomOpArgsFunction = poContract.GetFunction("addRuleCustomOpArgs");

            // NOTE: Caused exception to be thrown
            // var gas = addRuleFunction.EstimateGasAsync(psSenderAddress, "SomeRSID", "SomeRuleName", "SomeAttrName", 0, "SomeVal", false, false).Result;
            var gas = new Nethereum.Hex.HexTypes.HexBigInteger(1500000);

            foreach (WonkaBizRule TempRule in poRuleSet.EvaluativeRules)
            {
                var    sRuleName    = string.Empty;
                var    sAltRuleName = "Rule" + TempRule.RuleId;
                var    sAttrName    = TempRule.TargetAttribute.AttrName;
                uint   nRuleType    = (uint) CONTRACT_RULE_TYPES.MODE_MAX;
                string sValue       = string.Empty;
                var    passFlag     = TempRule.IsPassive;
                var    notFlag      = TempRule.NotOperator;

                if (TempRule.RuleType == RULE_TYPE.RT_ARITH_LIMIT)
                {
                    var ArithLimitRule = 
                            (Wonka.BizRulesEngine.RuleTree.RuleTypes.ArithmeticLimitRule) TempRule;

                    if (ArithLimitRule.MinValue <= Double.MinValue)
                    {
                        nRuleType = (uint)CONTRACT_RULE_TYPES.LESS_THAN_RULE;
                        sValue    = Convert.ToString(ArithLimitRule.MaxValue);
                    }
                    else if (ArithLimitRule.MaxValue >= Double.MaxValue)
                    {
                        nRuleType = (uint)CONTRACT_RULE_TYPES.GREATER_THAN_RULE;
                        sValue    = Convert.ToString(ArithLimitRule.MinValue);
                    }
                    else
                    {
                        nRuleType = (uint)CONTRACT_RULE_TYPES.EQUAL_TO_RULE;
                        sValue    = Convert.ToString(ArithLimitRule.MinValue);
                    }

                    if (ArithLimitRule.BlockNumOperator)
                        sValue = CONST_BLOCK_NUM_OP_IND;

                    sAltRuleName = "Limit(" + sValue + ") for -> [" + 
                        ((TempRule.TargetAttribute.AttrName.Length > 8) ? TempRule.TargetAttribute.AttrName.Substring(0,8) : TempRule.TargetAttribute.AttrName);
                }
                else if (TempRule.RuleType == RULE_TYPE.RT_DATE_LIMIT)
                {
                    var DateLimitRule =
                            (Wonka.BizRulesEngine.RuleTree.RuleTypes.DateLimitRule) TempRule;

                    if (DateLimitRule.MinValue <= DateTime.MinValue)
                    {
                        nRuleType = (uint) CONTRACT_RULE_TYPES.LESS_THAN_RULE;
                        sValue    = Convert.ToString(DateLimitRule.MaxValue.ToEpochTime());
                    }
                    else if (DateLimitRule.MaxValue >= DateTime.MaxValue)
                    {
                        nRuleType = (uint) CONTRACT_RULE_TYPES.GREATER_THAN_RULE;
                        sValue    = Convert.ToString(DateLimitRule.MinValue.ToEpochTime());
                    }
                    else
                    {
                        nRuleType = (uint) CONTRACT_RULE_TYPES.EQUAL_TO_RULE;
                        sValue    = Convert.ToString(DateLimitRule.MinValue.ToEpochTime());
                    }

                    if (DateLimitRule.TodayIndicator)
                    {
                        sValue = "0";

						if (DateLimitRule.AlmostOperator)
							sValue = "1";
                    }

                    /*
                     * NOTE: How do we serialize this operator?  Until we figure that out, prevent this rule from serialization
                    if (DateLimitRule.AroundOperator)
                    {}
                     */

                    sAltRuleName = "Date Limit(" + sValue + ") for -> [" +
                        ((TempRule.TargetAttribute.AttrName.Length > 8) ? TempRule.TargetAttribute.AttrName.Substring(0, 8) : TempRule.TargetAttribute.AttrName);
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
                        (Wonka.BizRulesEngine.RuleTree.RuleTypes.DomainRule) TempRule;
                        
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
				{
					sRuleName = TempRule.DescRuleId;
				}
				else
				{
					if (sAltRuleName.Length > CONST_CONTRACT_BYTE32_MAX)
						sAltRuleName = sAltRuleName.Substring(0, CONST_CONTRACT_BYTE32_MAX - 1);

					sRuleName = sAltRuleName;
				}

                // if ((nRuleType > 0) && !TempRule.NotOperator)
                if (nRuleType < (uint) CONTRACT_RULE_TYPES.MODE_MAX)
                {
                    var result =
                        await addRuleFunction.SendTransactionAsync(psRuleMasterAddress, gas, null, psSenderAddress, psRuleSetId, sRuleName, sAttrName, nRuleType, sValue, notFlag, passFlag).ConfigureAwait(false);
                }
                else 
                {
                    System.Console.WriteLine("ERROR!  This rule doesn't qualify for serialization!");    
                }
            }

            foreach (WonkaBizRule TempRule in poRuleSet.AssertiveRules)
            {
                var    sRuleName    = string.Empty;
                var    sAltRuleName = "Rule" + TempRule.RuleId;
                var    sAttrName    = TempRule.TargetAttribute.AttrName;
                uint   nRuleType    = (uint) CONTRACT_RULE_TYPES.MODE_MAX;
                string sValue       = string.Empty;
                var    notFlag      = TempRule.NotOperator;

                List<string> CustomOpArgs = new List<string>();

                // This is a legacy issue that will be addressed in the near future
                var passFlag = true; //TempRule.IsPassive;

                if (TempRule.RuleType == RULE_TYPE.RT_ASSIGNMENT)
                {
                    var AssignRule =
                        (Wonka.BizRulesEngine.RuleTree.RuleTypes.AssignmentRule) TempRule;

                    nRuleType = (uint) CONTRACT_RULE_TYPES.ASSIGN_RULE;

                    sValue = AssignRule.AssignValue;

                    sAltRuleName = "Assign(" + sValue + ") for -> [" +
                        ((TempRule.TargetAttribute.AttrName.Length > 8) ? TempRule.TargetAttribute.AttrName.Substring(0, 8) : TempRule.TargetAttribute.AttrName);                        
                }
                else if (TempRule.RuleType == RULE_TYPE.RT_ARITHMETIC)
                {
                    var AssignArithmeticRule =
                        (Wonka.BizRulesEngine.RuleTree.RuleTypes.ArithmeticRule) TempRule;

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
                            if (!string.IsNullOrEmpty(sValue)) sValue += ",";

                            sValue += sTempVal;
                        }

                        sAltRuleName = "Arithmetic Elements (" + sValue + ") for -> [" +
                            ((TempRule.TargetAttribute.AttrName.Length > 8) ? TempRule.TargetAttribute.AttrName.Substring(0, 8) : TempRule.TargetAttribute.AttrName);
                    }
                }
                else if (TempRule.RuleType == RULE_TYPE.RT_CUSTOM_OP)
                {
                    var CustomOpRule =
                        (Wonka.BizRulesEngine.RuleTree.RuleTypes.CustomOperatorRule) TempRule;

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
                {
                    sRuleName = TempRule.DescRuleId;
                }
                else
                {
                    if (sAltRuleName.Length > CONST_CONTRACT_BYTE32_MAX)
                        sAltRuleName = sAltRuleName.Substring(0, CONST_CONTRACT_BYTE32_MAX - 1);

                    sRuleName = sAltRuleName;
                }

				if (nRuleType < (uint) CONTRACT_RULE_TYPES.MODE_MAX)
				{
                    var result =
                        addRuleFunction.SendTransactionAsync(psRuleMasterAddress, gas, null, psSenderAddress, psRuleSetId, sRuleName, sAttrName, nRuleType, sValue, notFlag, passFlag).Result;

                    if (TempRule.RuleType == RULE_TYPE.RT_CUSTOM_OP)
                    {
                        var result2 =
                            await addCustomOpArgsFunction.SendTransactionAsync(psRuleMasterAddress, gas, null, psSenderAddress, psRuleSetId, CustomOpArgs[0], CustomOpArgs[1], CustomOpArgs[2], CustomOpArgs[3]).ConfigureAwait(false);
                    }
                }
                else 
                {
                    System.Console.WriteLine("ERROR!  This rule doesn't qualify for serialization!");    
                }
            }

            return true;
        }

    }

}
