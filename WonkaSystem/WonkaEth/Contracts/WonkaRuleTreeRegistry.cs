﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using WonkaEth.Extensions;

namespace WonkaEth.Contracts
{
    /// <summary>
    /// 
    /// This class will represent information a single RuleTree and can be added to
    /// the registry as a reference for interested parties.
    /// 
    /// </summary>
    public class WonkaRegistryItem
    {
        public WonkaRegistryItem()
        {
            RuleTreeId = Description = HostContractAddress = OwnerId = "";

            MinGasCost = MaxGasCost = 0;

            RuleTreeGroveIds = new Dictionary<string, int>();

            AssociateContractAddresses = new HashSet<string>();
            RequiredAttributes         = new HashSet<string>();
            UsedCustomOps              = new HashSet<string>();

            creationTime = 0;
        }

        public string RuleTreeId { get; set; }

        public string Description { get; set; }

        public Dictionary<string, int> RuleTreeGroveIds { get; set; }

        public string HostContractAddress { get; set; }

        public string OwnerId { get; set; }

        public int MinGasCost { get; set; }

        public int MaxGasCost { get; set; }

        public HashSet<string> AssociateContractAddresses { get; set; }

        public HashSet<string> RequiredAttributes { get; set; }

        public HashSet<string> UsedCustomOps { get; set; }

        public uint creationTime;
    }

    /// <summary>
    /// 
    /// This singleton, when initialized, will allow the user to register all RuleTrees
    /// and the Groves to which they belong.  In this way, the user can then allow other
    /// users to discover and reuse an added RuleTree, and in some cases, they might even
    /// discover and reuse a collection of them (i.e., a Grove).
    /// 
    /// </summary>
    public class WonkaRuleTreeRegistry
    {
        private static object mLock = new object();

        private static WonkaRuleTreeRegistry mInstance = null;

        Dictionary<string, WonkaRegistryItem> moRegisteredRuleTrees;

        private WonkaRuleTreeRegistry(string psSenderAddress, string psPassword, string psContractAddress, string psAbi)
        {
            moRegisteredRuleTrees = new Dictionary<string, WonkaRegistryItem>();

            RegistrySender          = psSenderAddress;
            RegistryPassword        = psPassword;
            RegistryContractAddress = psContractAddress;
            RegistryAbi             = psAbi;
        }

        static public WonkaRuleTreeRegistry CreateInstance(string psSenderAddress, string psPassword, string psContractAddress, string psAbi)
        {
            lock (mLock)
            {
                if (mInstance == null)
                    mInstance = new WonkaRuleTreeRegistry(psSenderAddress, psPassword, psContractAddress, psAbi);

                return mInstance;
            }
        }

        static public WonkaRuleTreeRegistry GetInstance()
        {
            lock (mLock)
            {
                if (mInstance == null)
                    throw new Exception("ERROR!  WonkaRuleTreeRegistry has not yet been initialized!");

                return mInstance;
            }
        }

        #region Instance Methods

        public void AddRegistryItem(WonkaRegistryItem poNewRuleTree)
        {
            if ((poNewRuleTree != null) && !String.IsNullOrEmpty(poNewRuleTree.RuleTreeId))
            {
                moRegisteredRuleTrees[poNewRuleTree.RuleTreeId] = poNewRuleTree;

                SerializeRegistryItem(poNewRuleTree.RuleTreeId);
            }
        }

        public void AddRuleGroveMember(string psRuleGroveId, string psRuleTreeName, int pnInGroupOrderNum = 0)
        {
            if (!String.IsNullOrEmpty(psRuleGroveId) && !String.IsNullOrEmpty(psRuleTreeName))
            {
                // We use a 0-based index for the group order
                if (pnInGroupOrderNum > 0)
                {
                    if (moRegisteredRuleTrees.ContainsKey(psRuleTreeName))
                        moRegisteredRuleTrees[psRuleTreeName].RuleTreeGroveIds[psRuleGroveId] = pnInGroupOrderNum;
                }
            }
        }

        public HashSet<WonkaRegistryItem> GetWonkaRegistryGroveMembers(string psRuleGroveId)
        {
            HashSet<WonkaRegistryItem> GroveMembers = new HashSet<WonkaRegistryItem>();

            if (!String.IsNullOrEmpty(psRuleGroveId))
            {
                var OrderedApplicableRuleTrees =
                    moRegisteredRuleTrees.Where(x => x.Value.RuleTreeGroveIds.ContainsKey(psRuleGroveId)).OrderBy(x => x.Value.RuleTreeGroveIds[psRuleGroveId]);

                foreach (var KeyValPair in OrderedApplicableRuleTrees)
                    GroveMembers.Add(KeyValPair.Value);
            }

            return GroveMembers;
        }

        public WonkaRegistryItem GetRegistryItem(string psRuleTreeName)
        {
            WonkaRegistryItem FoundItem = null;

            if (!String.IsNullOrEmpty(psRuleTreeName) && moRegisteredRuleTrees.ContainsKey(psRuleTreeName))
                FoundItem = moRegisteredRuleTrees[psRuleTreeName];

            return FoundItem;
        }

        public void SerializeRegistryItem(string psRuleTreeName)
        {
            WonkaRegistryItem FoundItem = null;

            if (!String.IsNullOrEmpty(psRuleTreeName) && moRegisteredRuleTrees.ContainsKey(psRuleTreeName))
            {
                FoundItem = moRegisteredRuleTrees[psRuleTreeName];

                FoundItem.Serialize();
            }
        }

        #endregion

        #region Properties

        public readonly string RegistrySender;

        public readonly string RegistryPassword;

        public readonly string RegistryContractAddress;

        public readonly string RegistryAbi;

        #endregion
    }
}