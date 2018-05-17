using System;

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
using WonkaRef;

namespace WonkaEth.Extensions
{
    public class CallAddRuleTreeEvent
    {
        [Parameter("address", "ruler", 1, true)]
        public string TreeOwner { get; set; }
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

        private static WonkaRefEnvironment moWonkaRevEnv = WonkaRefEnvironment.GetInstance();

        public enum CONTRACT_RULE_TYPES
        {
            EQUAL_TO_RULE = 0,
            LESS_THAN_RULE,
            GREATER_THAN_RULE = 2,
            POPULATED_RULE = 3,
            IN_DOMAIN_RULE = 4,
            ASSIGN_RULE = 5,
            MODE_MAX
        }

        /// <summary>
        /// 
        /// This method will use Nethereum to call upon an instance of the Ethgine contract and 
        /// to create a RuleTree that will be owned by the Sender.
        /// 
        /// <param name="poEngine">The instance of an engine which contains the root node of the RuleTree/param>
        /// <param name="psSenderAddress">The Ethereum address of the sender account/param>
        /// <param name="psPassword">The password for the sender/param>
        /// <param name="psContractAddress">The address of the instance of the Ethgine contract/param>
        /// <param name="psAbi">The ABI interface for the Ethgine contract/param>
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

            return bResult;
        }

        /// <summary>
        /// 
        /// This method will use Nethereum to call upon an instance of the Ethgine contract and 
        /// to establish the Attributes (i.e., data points) that our intended RuleTree will examine.
        /// 
        /// <param name="poInstance">The instance of an Environment which contains the Attributes that we will want to share with the Ethgine contract/param>
        /// <param name="psSenderAddress">The Ethereum address of the sender account/param>
        /// <param name="psPassword">The password for the sender/param>
        /// <param name="psContractAddress">The address of the instance of the Ethgine contract/param>
        /// <param name="psAbi">The ABI interface for the Ethgine contract/param>
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

        /// <summary>
        /// 
        /// This method will use Nethereum to call upon an instance of the Ethgine contract and 
        /// to add the root node for a RuleTree (as well as the rest of the RuleTree).
        /// 
        /// <param name="poRuleSet">The root node of the RuleTree that we are creating in the Ethgine contract/param>
        /// <param name="psSenderAddress">The Ethereum address of the sender account who owns the RuleTree/param>
        /// <param name="poContract">The Ethgine contract in which we are adding the RuleTree/param>
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
        /// <param name="poRuleSet">The current node of the RuleTree that we are creating in the Ethgine contract/param>
        /// <param name="psSenderAddress">The Ethereum address of the sender account who owns the RuleTree/param>
        /// <param name="poContract">The Ethgine contract in which we are adding the RuleTree/param>
        /// <param name="psRSParentName">The parent node of the current node that we are adding to the RuleTree/param>
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
        /// to add all of the rules that belong to a RuelSet node of the RuleTree.
        /// 
        /// <param name="poRuleSet">The current node of the RuleTree whose rules we are creating in the Ethgine contract/param>
        /// <param name="psSenderAddress">The Ethereum address of the sender account who owns the RuleTree/param>
        /// <param name="poContract">The Ethgine contract in which we are adding the RuleTree/param>
        /// <param name="psRuleSetId">The name of the current node in the blockchain whose rules we are adding to the RuleTree/param>
        /// <returns>Indicates whether or not the rules of the current node were submitted to the blockchain</returns>
        /// </summary>
        private static bool SerializeRules(this WonkaBre.RuleTree.WonkaBreRuleSet poRuleSet, string psSenderAddress, Nethereum.Contracts.Contract poContract, string psRuleSetId)
        {
            var addRuleTreeFunction = poContract.GetFunction("addRule");

            // NOTE: Caused exception to be thrown
            // var gas = addRuleTreeFunction.EstimateGasAsync(psSenderAddress, "SomeRSID", "SomeRuleName", "SomeAttrName", 0, "SomeVal", false, false).Result;
            var gas = new Nethereum.Hex.HexTypes.HexBigInteger(1000000);

            if (poRuleSet.Description == "Validating Account Type")
            {
                int x = 1;
            }

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

                    foreach (string sTempVal in DomainRule.DomainCache)
                    {
                        if (!String.IsNullOrEmpty(sValue)) sValue += ",";

                        sValue += sTempVal;    
                    }

                    string sDomainAbbr = (sValue.Length > 8) ? sValue.Substring(0, 8) + "..." : sValue;
                    sAltRuleName = "Domain(" + sDomainAbbr + ") for [" +
                        ((TempRule.TargetAttribute.AttrName.Length > 13) ? TempRule.TargetAttribute.AttrName.Substring(0, 13) : TempRule.TargetAttribute.AttrName);                        
                }
                else if (TempRule.RuleType == RULE_TYPE.RT_ASSIGNMENT)
                {
                    var AssignRule =
                        (WonkaBre.RuleTree.RuleTypes.AssignmentRule) TempRule;

                    nRuleType = (uint)CONTRACT_RULE_TYPES.ASSIGN_RULE;

                    sValue = AssignRule.AssignValue;

                    sAltRuleName = "Assign(" + sValue + ") for -> [" +
                        ((TempRule.TargetAttribute.AttrName.Length > 8) ? TempRule.TargetAttribute.AttrName.Substring(0, 8) : TempRule.TargetAttribute.AttrName);                        
                }

                if (!String.IsNullOrEmpty(TempRule.DescRuleId))
                    sRuleName = TempRule.DescRuleId;
                else
                {
                    if (sAltRuleName.Length > CONST_CONTRACT_BYTE32_MAX)
                        sAltRuleName = sAltRuleName.Substring(0, CONST_CONTRACT_BYTE32_MAX - 1);

                    sRuleName = sAltRuleName;
                }

                if ((nRuleType > 0) && !TempRule.NotOperator)
                {
                    var result =
                        addRuleTreeFunction.SendTransactionAsync(psSenderAddress, gas, null, psSenderAddress, psRuleSetId, sRuleName, sAttrName, nRuleType, sValue, notFlag, passFlag).Result;
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
