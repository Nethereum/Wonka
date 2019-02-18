using System;
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
            Init();
        }

        public WonkaRegistryItem(WonkaEth.Extensions.RuleTreeRegistryIndex poIndex, string psHostContractABI)
        {
            Init();


            RuleTreeId  = poIndex.RuleTreeId;
            Description = poIndex.RuleTreeDesc;

            HostContractAddress = poIndex.RuleTreeHostEngineAddr;
            HostContractABI     = psHostContractABI;

            OwnerId      = poIndex.RuleTreeOwner;
            MaxGasCost   = poIndex.MaxGasCost;
            creationTime = poIndex.CreationEpochTime;

            RequiredAttributes.UnionWith(poIndex.RequiredAttributes);
        }

        public void Init()
        {
            RuleTreeId = Description = HostContractAddress = OwnerId = HostContractABI = "";

            MinGasCost = MaxGasCost = 0;

            RuleTreeGroveIds = new Dictionary<string, int>();

            AssociateContractAddresses = new HashSet<string>();
            RequiredAttributes         = new HashSet<string>();
            UsedCustomOps              = new HashSet<string>();

            creationTime = 0;            
        }


        #region Properties

        public string RuleTreeId { get; set; }

        public string Description { get; set; }

        public Dictionary<string, int> RuleTreeGroveIds { get; set; }

        public string HostContractAddress { get; set; }

        public string HostContractABI { get; set; }

        public string OwnerId { get; set; }

        public uint MinGasCost { get; set; }

        public uint MaxGasCost { get; set; }

        public HashSet<string> AssociateContractAddresses { get; set; }

        public HashSet<string> RequiredAttributes { get; set; }

        public HashSet<string> UsedCustomOps { get; set; }

        public uint creationTime;

        #endregion
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

        private WonkaRuleTreeRegistry(string psSenderAddress, string psPassword, string psContractAddress, string psAbi, string psWeb3HttpUrl)
        {
            moRegisteredRuleTrees = new Dictionary<string, WonkaRegistryItem>();

            RegistrySender          = psSenderAddress;
            RegistryPassword        = psPassword;
            RegistryContractAddress = psContractAddress;
            RegistryAbi             = psAbi;
            RegistryWeb3HttpUrl     = psWeb3HttpUrl;
        }

        static public WonkaRuleTreeRegistry CreateInstance(string psSenderAddress, string psPassword, string psContractAddress, string psAbi, string psWeb3HttpUrl = "")
        {
            lock (mLock)
            {
                if (mInstance == null)
                    mInstance = new WonkaRuleTreeRegistry(psSenderAddress, psPassword, psContractAddress, psAbi, psWeb3HttpUrl);

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

        public readonly string RegistryWeb3HttpUrl;

        public readonly string RegistrySender;

        public readonly string RegistryPassword;

        public readonly string RegistryContractAddress;

        public readonly string RegistryAbi;

        #endregion
    }
}
