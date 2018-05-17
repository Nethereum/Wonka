using WonkaBre.RuleTree;

namespace WonkaBre.Reporting
{
    /// <summary>
    /// 
    /// This class will contain data about the success or failure
    /// with a particular rule.
    /// 
    /// </summary>
    public class WonkaBreRuleReportNode
    {
        public WonkaBreRuleReportNode()
        {
            RuleID        = -1;
            TriggerAttrId = -1;

            ErrorCode = ERR_CD.CD_SUCCESS;

            ErrorDescription = VerboseError = "";
        }

        public WonkaBreRuleReportNode(WonkaBreRule poRule) : this()
        {
            RuleID        = poRule.RuleId;
            TriggerAttrId = poRule.TargetAttribute.AttrId;
        }

        public int RuleID { get; set; }

        public int TriggerAttrId { get; set; }

        public ERR_CD ErrorCode { get; set; }

        public string ErrorDescription { get; set; }

        public string VerboseError { get; set; }
    }
}
