using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.ABI.Model;

using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;

using WonkaBre;
using WonkaEth.Orchestration.BlockchainEvents;

namespace WonkaEth.Extensions
{
    [FunctionOutput]
    public class ExportRuleTreeProps
    {
        [Parameter("bytes32", "rtid", 1)]
        public string RuleTreeId { get; set; }

        [Parameter("string", "rtdesc", 2)]
        public string RuleTreeDesc { get; set; }

        [Parameter("bytes32", "rootruleset", 3)]
        public string RootRuleSetName { get; set; }
    }

    [FunctionOutput]
    public class ExportRuleSetProps
    {
        [Parameter("string", "rsdesc", 1)]
        public string RuleSetDesc { get; set; }

        [Parameter("bool", "severefail", 2)]
        public bool SevereFailureFlag { get; set; }

        [Parameter("bool", "andop", 3)]
        public bool AndOperatorFlag { get; set; }

        [Parameter("uint", "evalrulenum", 4)]
        public uint EvalRuleCount { get; set; }

        [Parameter("uint", "assertrulenum", 5)]
        public uint AssertiveRuleCount { get; set; }

        [Parameter("uint", "childrulesetnum", 6)]
        public uint ChildRuleSetCount { get; set; }
    }

    [FunctionOutput]
    public class ExportRuleProps
    {
        [Parameter("bytes32", "rulename", 1)]
        public string RuleName { get; set; }

        [Parameter("uint", "ruletype", 2)]
        public uint RuleType { get; set; }

        [Parameter("bytes32", "attrname", 3)]
        public string AttrName { get; set; }

        [Parameter("string", "ruleval", 4)]
        public string RuleValue { get; set; }

        [Parameter("bool", "notopflag", 5)]
        public bool NotOpFlag { get; set; }

        [Parameter("bytes32[]", "custopargs", 6)]
        public List<string> CustomOpArgs { get; set; }
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

    public class WonkaInvocationEvents
    {
        public WonkaInvocationEvents(Contract poWonkaEngineContract)
        {
            RuleTreeEvents      = poWonkaEngineContract.GetEvent(WonkaExtensions.CONST_EVENT_CALL_RULE_TREE);
            RuleTreeEventFilter = RuleTreeEvents.CreateFilterAsync().Result;

            RuleSetEvents      = poWonkaEngineContract.GetEvent(WonkaExtensions.CONST_EVENT_CALL_RULE_SET);
            RuleSetEventFilter = RuleSetEvents.CreateFilterAsync().Result;

            RuleEvents      = poWonkaEngineContract.GetEvent(WonkaExtensions.CONST_EVENT_CALL_RULE);
            RuleEventFilter = RuleEvents.CreateFilterAsync().Result;

            RuleSetErrorEvents      = poWonkaEngineContract.GetEvent(WonkaExtensions.CONST_EVENT_RULE_SET_ERROR);
            RuleSetErrorEventFilter = RuleSetErrorEvents.CreateFilterAsync().Result;
        }

        public Event RuleTreeEvents { get; set; }

        public Event RuleSetEvents { get; set; }

        public Event RuleEvents { get; set; }

        public Event RuleSetErrorEvents { get; set; }

        public HexBigInteger RuleTreeEventFilter { get; set; }

        public HexBigInteger RuleSetEventFilter { get; set; }

        public HexBigInteger RuleEventFilter { get; set; }

        public HexBigInteger RuleSetErrorEventFilter { get; set; }

        public void HandleEvents(WonkaBreRulesEngine poRulesEngine, RuleTreeReport poRuleTreeReport)
        {
            var ruleTreeLog   = RuleTreeEvents.GetFilterChanges<CallRuleTreeEvent>(RuleTreeEventFilter).Result;
            var ruleSetLog    = RuleSetEvents.GetFilterChanges<CallRuleSetEvent>(RuleSetEventFilter).Result;
            var ruleLog       = RuleEvents.GetFilterChanges<CallRuleEvent>(RuleEventFilter).Result;
            var ruleSetErrLog = RuleSetErrorEvents.GetFilterChanges<RuleSetErrorEvent>(RuleSetErrorEventFilter).Result;

            /**
            if (ruleTreeLog.Count > 0)
                System.Console.WriteLine("RuleTree Called that Belongs to : (" + ruleTreeLog[0].Event.TreeOwner + ")");
            **/

            if (ruleSetLog.Count > 0)
            {
                foreach (EventLog<CallRuleSetEvent> TmpRuleSetEvent in ruleSetLog)
                {
                    if (TmpRuleSetEvent.Event != null)
                        poRuleTreeReport.RuleSetIds.Add(TmpRuleSetEvent.Event.RuleSetId);
                }
            }

            if (ruleLog.Count > 0)
            {
                foreach (EventLog<CallRuleEvent> TmpRuleEvent in ruleLog)
                {
                    if (TmpRuleEvent.Event != null)
                        poRuleTreeReport.RuleIds.Add(TmpRuleEvent.Event.RuleId); // TmpRuleEvent.Event.RuleType
                }
            }

            if (ruleSetErrLog.Count > 0)
            {
                foreach (EventLog<RuleSetErrorEvent> TmpRuleSetError in ruleSetErrLog)
                {
                    if (TmpRuleSetError.Event != null)
                    {
                        if (TmpRuleSetError.Event.SevereFailure)
                            poRuleTreeReport.RuleSetFailures.Add(TmpRuleSetError.Event.RuleSetId);
                        else
                            poRuleTreeReport.RuleSetWarnings.Add(TmpRuleSetError.Event.RuleSetId);
                    }
                }
            }
        }

    }

    /// <summary>
    /// This class represents the aggregated output of the rules engine on the blockchain.
    /// </summary>
    [FunctionOutput]
    public class RuleTreeReport
    {
        public RuleTreeReport()
        {
            NumberOfRuleFailures = 0;
            TransactionHash      = "";
            InvokeTrxBlockNumber = null;

            RuleSetIds = new List<string>();
            RuleIds    = new List<string>();

            RuleSetWarnings = new List<string>();
            RuleSetFailures = new List<string>();

            RuleSetFailMessages = new Dictionary<string, string>();
        }

        public RuleTreeReport(RuleTreeReport poOriginal)
        {
            Copy(poOriginal);
        }

        public void Copy(RuleTreeReport poOriginal)
        {
            NumberOfRuleFailures = poOriginal.NumberOfRuleFailures;
            TransactionHash      = poOriginal.TransactionHash;
            InvokeTrxBlockNumber = poOriginal.InvokeTrxBlockNumber;

            RuleSetIds = poOriginal.RuleSetIds;
            RuleIds    = poOriginal.RuleIds;

            RuleSetWarnings = poOriginal.RuleSetWarnings;
            RuleSetFailures = poOriginal.RuleSetFailures;

            RuleSetFailMessages = poOriginal.RuleSetFailMessages;
        }

        [Parameter("uint", "fails", 1)]
        public uint NumberOfRuleFailures { get; set; }

        [Parameter("bytes32[]", "rsets", 2)]
        public List<string> RuleSetIds { get; set; }

        [Parameter("bytes32[]", "rules", 3)]
        public List<string> RuleIds { get; set; }

        [Parameter("bytes32[]", "rset_warnings", 4)]
        public List<string> RuleSetWarnings { get; set; }

        [Parameter("bytes32[]", "rset_failures", 5)]
        public List<string> RuleSetFailures { get; set; }

        public Dictionary<string, string> RuleSetFailMessages { get; set; }

        public string TransactionHash { get; set; }

        public HexBigInteger InvokeTrxBlockNumber { get; set; }
    }
}
