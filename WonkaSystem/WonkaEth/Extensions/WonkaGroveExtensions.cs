using System;
using System.Collections.Generic;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.ABI.Model;
using Nethereum.Web3.Accounts;

using Wonka.Eth.Contracts;
using Wonka.Eth.Orchestration;
using Wonka.MetaData;

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
    }
}