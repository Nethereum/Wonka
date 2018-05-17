using System.Collections.Generic;

namespace WonkaBre.Reporting
{
    /// <summary>
    /// 
    /// This class will contain data about the evaluations of applied rules
    /// (i.e., instances of WonkaBreRuleReportNode) within a RuleSet.
    /// 
    /// </summary>
    public class WonkaBreRuleSetReportNode
    {
        public WonkaBreRuleSetReportNode()
        {
            this.RuleSetID = -1;

            this.RuleSetDescription = "";

            this.CustomId  = "";
            this.ErrorCode = ERR_CD.CD_SUCCESS;

            this.ErrorDescription = "";
            this.ErrorSeverity    = RULE_SET_ERR_LVL.ERR_LVL_NONE;

            this.WarningFailureCount = 0;
            this.SevereFailureCount  = 0;

            this.RuleResults = new List<WonkaBreRuleReportNode>();
        }

        public int RuleSetID { get; set; }

        public string RuleSetDescription { get; set; }

        public string CustomId { get; set; }

        public ERR_CD ErrorCode { get; set; }

        public string ErrorDescription { get; set; }

        public RULE_SET_ERR_LVL ErrorSeverity { get; set; }

        public int WarningFailureCount { get; set; }

        public int SevereFailureCount { get; set; }

        public List<WonkaBreRuleReportNode> RuleResults { get; set; }
    }
}


