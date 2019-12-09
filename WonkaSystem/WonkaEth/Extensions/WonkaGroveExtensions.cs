using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.ABI.Model;
using Nethereum.Web3.Accounts;

using Wonka.BizRulesEngine;
using Wonka.BizRulesEngine.Reporting;
using Wonka.BizRulesEngine.RuleTree;
using Wonka.Eth.Contracts;
using Wonka.Eth.Orchestration;
using Wonka.MetaData;
using Wonka.Product;

namespace Wonka.Eth.Extensions
{
    [FunctionOutput]
    public class RuleGroveRegistryData
    {
        [Parameter("bytes32", "id", 1)]
        public string RuleGroveId { get; set; }

        [Parameter("string", "desc", 2)]
        public string RuleGroveDescription { get; set; }

        [Parameter("bytes32[]", "members", 3)]
        public List<string> RuleTreeMembers { get; set; }

        [Parameter("address", "owner", 4)]
        public string RuleGroveOwner { get; set; }

        [Parameter("uint", "createTime", 5)]
        public uint CreationEpochTime { get; set; }

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
    /// This extensions class provides the functionality to handle all activities that will be invoked on
    /// behalf of a RuleGrove.
    /// 
    /// </summary>
    public static class WonkaGroveExtensions
    {
        private static WonkaRefEnvironment moWonkaRevEnv = WonkaRefEnvironment.GetInstance();

        /// <summary>
        /// 
        /// This method will execute the RuleTree collection of a Grove, invoking each one either on the chain or off the chain.
        /// All of the invocations will be assembled and then returned in a report about the execution of the Grove.
        /// 
        /// NOTE: UNDER CONSTRUCTION
        /// 
        /// <param name="poGrove">The Grove that we are looking to execute, especially its pool of RuleTree members</param>
        /// <returns>The report that will hold the aggregated reports about the execution of each Grove member</returns>
        /// </summary>
        public static async Task<WonkaBizGroveReport> ExecuteAsync(this WonkaBizGrove poGrove, Wonka.Eth.Init.WonkaEthEngineInitialization poEngineInitData)
        {
            if (poGrove.RuleTreeMembers.Count == 0)
                throw new WonkaEthException("ERROR!  Execute() cannot be invoked for Grove(" + poGrove.GroveDesc + ") when it has no members.");

            if (poEngineInitData == null)
                throw new WonkaEthException("ERROR!  Cannot invoke the Grove(" + poGrove.GroveDesc + ") since the EthEngineInit data provided is empty.");

            var GroveReport = new WonkaBizGroveReport(poGrove);

            foreach (WonkaBizRulesEngine RuleTreeMember in poGrove.RuleTreeMembers)
            {
                if (poGrove.ExecuteRuleTreesOnChain.Contains(RuleTreeMember))
                {
                    var OnChainReport = new Wonka.Eth.Extensions.RuleTreeReport();

                    // NOTE: We assume here that that the Wonka contract instance (mentioned within 'poEngineInitData') and certain RuleTrees of 'poGrove'
                    //       exist on the chain - maybe we should check that fact?

                    string sTrxHash = await RuleTreeMember.ExecuteOnChainAsync(poEngineInitData, OnChainReport).ConfigureAwait(false);

                    GroveReport.RuleTreeReports[RuleTreeMember.DetermineRuleTreeChainID()] = OnChainReport;
                }
                else
                {
                    var OffChainReport = new WonkaBizRuleTreeReport();

                    WonkaProduct CurrValues = new WonkaProduct();

                    bool bResult = 
                        await CurrValues.PopulateWithDataFromChainAsync(RuleTreeMember.RefEnvHandle, RuleTreeMember.SourceMap, poEngineInitData.Web3HttpUrl).ConfigureAwait(false);

                    OffChainReport = RuleTreeMember.Validate(CurrValues);

                    GroveReport.RuleTreeReports[RuleTreeMember.DetermineRuleTreeChainID()] = OffChainReport;
                }

                if (poGrove.ExecutionBreakpointBetweenRuleTrees != null)
                    poGrove.ExecutionBreakpointBetweenRuleTrees.Invoke(poGrove, RuleTreeMember);
            }

            GroveReport.EndTime = DateTime.Now;

            return GroveReport;
        }

        /// <summary>
        /// 
        /// This method will use Nethereum to execute all of the RuleTrees within a particular Grove.
        /// 
        /// <param name="poGrove">The Grove that we are interested in</param>
        /// <param name="poCommand">The command (and data) that is to be processed by invoking the Grove</param>
        /// <param name="poOrchestrators">The collection that knows how to serialize/deserialize/orchestrate in reference to the data of 'poCommand'</param>
        /// <returns>None</returns>
        /// </summary>
        public static void Orchestrate(this WonkaRuleGrove poGrove, ICommand poCommand, Dictionary<string, IOrchestrate> poOrchestrators)
        {
            foreach (WonkaRegistryItem TmpRuleTree in poGrove.OrderedRuleTrees)
            {
                IOrchestrate TmpOrchestrator = poOrchestrators[TmpRuleTree.RuleTreeId];

                TmpOrchestrator.SerializeRecordToBlockchain(poCommand);

                bool bValid = TmpOrchestrator.Orchestrate(poCommand, false);

                TmpOrchestrator.DeserializeRecordFromBlockchain(poCommand);
            }
        }

        /// <summary>
        /// 
        /// This method will use Nethereum to call upon the Registry and retrieve all member data affiliated with 
        /// a particular Grove.
        /// 
        /// <param name="poGrove">The Grove that we are interested in</param>
        /// <param name="psDefaultWonkaABI">The default ABI for the Wonka contract (since we might have different versions in the future)</param>
        /// <returns>None</returns>
        /// </summary>
        public static void PopulateFromRegistry(this WonkaRuleGrove poGrove, string psDefaultWonkaABI)
        {
            if (String.IsNullOrEmpty(poGrove.GroveId))
                throw new Exception("ERROR!  No Grove ID provided.");

            var contract             = WonkaExtensions.GetRegistryContract();
            var getGroveInfoFunction = contract.GetFunction("getRuleGrove");

            var groveRegistryInfo = getGroveInfoFunction.CallDeserializingToObjectAsync<RuleGroveRegistryData>(poGrove.GroveId).Result;

            poGrove.Ingest(groveRegistryInfo);

            foreach (string sTmpGroveId in groveRegistryInfo.RuleTreeMembers)
                poGrove.OrderedRuleTrees.Add(new WonkaRegistryItem(WonkaExtensions.GetRuleTreeIndex(sTmpGroveId), psDefaultWonkaABI));

            string sCreateDateTime = poGrove.CreationTime.ToString();
        }

        /// <summary>
        /// 
        /// This method will break apart a current RuleTree encapsulated within an instance of WonkaBizRulesEngine, taking each top level branch
        /// and using it to create a new RuleTree.  These multiple instances of RuleTree will then form a new Grove.
        /// 
        /// NOTE: UNDER CONSTRUCTION
        /// 
        /// <param name="poRuleTree">The RuleTree that we are looking to turn into a Grove</param>
        /// <returns>The Grove that will hold the RuleTrees created from its parent</returns>
        /// </summary>
        public static WonkaBizGrove Splinter(this WonkaBizRulesEngine poRuleTree, int pnGroveId, string psGroveDesc)
        {
            WonkaBizGrove SpawnedGrove = new WonkaBizGrove(pnGroveId, psGroveDesc);

            if (poRuleTree.RuleTreeRoot == null)
                throw new WonkaEthException("ERROR!  Splinter() cannot be invoked when there is no root of the supplied RuleTree.");

            if (poRuleTree.RuleTreeRoot.ChildRuleSets.Count <= 0)
                throw new WonkaEthException("ERROR!  Splinter() cannot be invoked when the root of the supplied RuleTree has no children.");

            foreach (WonkaBizRuleSet TopChildRuleset in poRuleTree.RuleTreeRoot.ChildRuleSets)
            {
                SpawnedGrove.AddRuleTree(new WonkaBizRulesEngine(TopChildRuleset, poRuleTree));
            }

            return SpawnedGrove;
        }

    }
}