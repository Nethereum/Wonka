﻿using System;
using System.Collections.Generic;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.ABI.Model;
using Nethereum.Web3.Accounts;

using WonkaEth.Contracts;
using WonkaEth.Orchestration;
using WonkaRef;

namespace WonkaEth.Extensions
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
        /// <returns>None</returns>
        /// </summary>
        public static void Orchestrate(this WonkaRuleGrove poGrove, ICommand poCommand)
        {
            // NOTE: TBD
        }

        /// <summary>
        /// 
        /// This method will use Nethereum to call upon the Registry and retrieve all member data affiliated with 
        /// a particular Grove.
        /// 
        /// <param name="poGrove">The Grove that we are interested in</param>
        /// <returns>None</returns>
        /// </summary>
        public static void PopulateFromRegistry(this WonkaRuleGrove poGrove)
        {
            var WonkaRegistry = WonkaRuleTreeRegistry.GetInstance();

            if (String.IsNullOrEmpty(poGrove.GroveId))
                throw new Exception("ERROR!  No Grove ID provided.");

            var sPassword = WonkaRegistry.RegistryPassword;
            var sABI = WonkaRegistry.RegistryAbi;
            var sContractAddr = WonkaRegistry.RegistryContractAddress;

            var account = new Account(sPassword);
            var web3 = new Nethereum.Web3.Web3(account);
            var contract = web3.Eth.GetContract(sABI, sContractAddr);

            var getGroveInfoFunction = contract.GetFunction("getRuleGrove");

            var groveRegistryInfo = getGroveInfoFunction.CallDeserializingToObjectAsync<RuleGroveRegistryData>(poGrove.GroveId).Result;

            poGrove.Ingest(groveRegistryInfo);

            foreach (string sTmpGroveId in groveRegistryInfo.RuleTreeMembers)
                poGrove.OrderedRuleTrees.Add(new WonkaRegistryItem(WonkaExtensions.GetRuleTreeIndex(sTmpGroveId)));

            string sCreateDateTime = poGrove.CreationTime.ToString();
        }
    }
}