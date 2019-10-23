using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Wonka.BizRulesEngine.RuleTree;
using WonkaPrd;
using WonkaRef;

namespace Wonka.BizRulesEngine.Reporting
{
    /// <summary>
    /// 
    /// This class will contain data about the evaluations of applied rulesets
    /// (i.e., instances of WonkaBreRuleSetReportNode) within a RuleTree.
    /// 
    /// </summary>
    public class WonkaBizRuleTreeReport
    {
        #region CONSTANTS
        private const string CONST_ERROR_MSG_PREFACE = "BUSINESS RULE: ";
        #endregion

        public WonkaBizRuleTreeReport()
        {
            this.OverallRuleTreeResult = ERR_CD.CD_SUCCESS;            

            this.RuleSetResults  = new List<WonkaBizRuleSetReportNode>();
            this.RuleSetFailures = new List<WonkaBizRuleSetReportNode>();
        }

        #region Simple Accessors

        public int GetRuleSetFailureCount()
        {
            return RuleSetFailures.Count;
        }

        public WonkaBizRuleSetReportNode GetRuleSetFailure(int pnIndex = 0)
        {
            WonkaBizRuleSetReportNode FailureReportNode = null;

            if (pnIndex < RuleSetFailures.Count)
            {
                FailureReportNode = RuleSetFailures[pnIndex];
            }

            return FailureReportNode;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// This method will flag the failure of a particular RuleSet by
        /// 
        /// 1.) Setting the error properties on the corresponding RuleSetReport
        /// 2.) Adding the RuleSetReport to the Failures list
        /// 3.) Marking the RuleTree as a failure (since the failure of only one RuleSet flags the whole tree)
        /// 
        /// <param name="poTargetRuleSet">The target RuleSet which will be marked as a failure</param>
        /// <param name="peRuleSetErrCd">The type of failure for the RuleSet</param>
        /// <param name="psRuleSetErrMsg">The error message that will provide details about the RuleSet's failure</param>
        /// <returns>The list of RuleReports that describes the execution of a specific RuleSet</returns>
        /// </summary>
        public bool AddResultSetFailure(WonkaBizRuleSet poTargetRuleSet, ERR_CD peRuleSetErrCd, string psRuleSetErrMsg)
        {
            bool bResult = true;

            WonkaBizRuleSetReportNode RuleSetReportNode = FindRuleSetReport(poTargetRuleSet.RuleSetId, false);
            if (RuleSetReportNode != null)
            {
                RuleSetReportNode.ErrorSeverity    = poTargetRuleSet.ErrorSeverity;
                RuleSetReportNode.ErrorCode        = peRuleSetErrCd;
                RuleSetReportNode.ErrorDescription = psRuleSetErrMsg;

                if ( (poTargetRuleSet.ErrorSeverity == RULE_SET_ERR_LVL.ERR_LVL_WARNING) ||
                     (poTargetRuleSet.ErrorSeverity == RULE_SET_ERR_LVL.ERR_LVL_SEVERE) )
                {
                    RuleSetFailures.Add(RuleSetReportNode);

                    OverallRuleTreeResult = ERR_CD.CD_FAILURE;
                }
            }            

            return bResult;
        }

        /// <summary>
        /// 
        /// This method will archive the results of a particular rule's execution by storing them as a 
        /// a RuleReportNode and by inserting it into a RuleSetReportNode.
        /// 
        /// <param name="poRule">The business rule that we have executed</param>
        /// <param name="peRuleErrCd">The error (i.e., result) code of that rule's execution</param>
        /// <param name="psRuleErrorDesc">The general description of the rule's error (if there is one)</param>
        /// <param name="psVerboseError">The verbose description of the rule's error (if there is one)</param>
        /// <returns>The indicator for whether or not the rule's execution results were successfully archived</returns>
        /// </summary>
        public bool ArchiveRuleExecution(WonkaBizRule poRule, ERR_CD peRuleErrCd, string psRuleErrorDesc, string psVerboseError)
        {
            bool bResult = true;

            WonkaBizRuleSetReportNode RuleSetReportNode = FindRuleSetReport(poRule.ParentRuleSetId, true);
            if (RuleSetReportNode != null)
            {
                WonkaBizRuleReportNode RuleReportNode = new WonkaBizRuleReportNode(poRule);

                RuleReportNode.ErrorCode        = peRuleErrCd;
                RuleReportNode.ErrorDescription = psRuleErrorDesc;
                RuleReportNode.VerboseError     = psVerboseError;

                RuleSetReportNode.RuleResults.Add(RuleReportNode);
            }

            return bResult;
        }

        /// <summary>
        /// 
        /// Under Construction
        /// 
        /// </summary>
        public void Dump(string psTargetFilepath)
        {
            // NOTE: Will we use this function?
        }

        /// <summary>
        /// 
        /// This method will create an error string that mentions all of the validation rules that indicate 
        /// a failure, whether a warning or severe.
        /// 
        /// <returns>The string that mentions all of the failed valiation rules</returns>
        /// </summary>
        public string GetErrorString()
        {
            StringBuilder AllErrorsBody = new StringBuilder();

            foreach (WonkaBizRuleSetReportNode WarningRuleSetReport in GetRuleSetWarningFailures())
            {
                foreach (WonkaBizRuleReportNode WarningRuleReport in WarningRuleSetReport.RuleResults)
                {
                    if (WarningRuleReport.ErrorCode == ERR_CD.CD_FAILURE)
                    {
                        AllErrorsBody.Append("WARNING!  (" + WarningRuleReport.VerboseError + ").");
                    }
                }
            }

            foreach (WonkaBizRuleSetReportNode SevereRuleSetReport in GetRuleSetWarningFailures())
            {
                foreach (WonkaBizRuleReportNode SevereRuleReport in SevereRuleSetReport.RuleResults)
                {
                    if (SevereRuleReport.ErrorCode == ERR_CD.CD_FAILURE)
                    {
                        AllErrorsBody.Append("ERROR!  (" + SevereRuleReport.VerboseError + ").");
                    }
                }
            }

            return AllErrorsBody.ToString();
        }

        /// <summary>
        /// 
        /// This method will return all RuleSetReports that are flagged as failures and have their 
        /// severity rating set to SEVERE.
        /// 
        /// <returns>The list of failed RuleSetReports whose respective RuleSets are set as SEVERE</returns>
        /// </summary>
        public List<WonkaBizRuleSetReportNode> GetRuleSetSevereFailures()
        {
            return RuleSetResults.Where(x => x.ErrorSeverity == RULE_SET_ERR_LVL.ERR_LVL_SEVERE).ToList();
        }

        /// <summary>
        /// 
        /// This method will return all RuleSetReports that are flagged as failures and have their 
        /// severity rating set to WARNING.
        /// 
        /// <returns>The list of failed RuleSetReports whose respective RuleSets are set as WARNING</returns>
        /// </summary>
        public List<WonkaBizRuleSetReportNode> GetRuleSetWarningFailures()
        {
            return RuleSetResults.Where(x => x.ErrorSeverity == RULE_SET_ERR_LVL.ERR_LVL_WARNING).ToList();
        }

        /// <summary>
        /// 
        /// This method provides a convenient way of taking RuleReports with failures (from certain failed RuleSets)
        /// and converting them into ProductError records (that can then be packaged into a XML message).
        /// 
        /// <param name="psProductId">The Product ID of the current product that has just been evaluated in this RuleTreeReport</param>
        /// <param name="peRuleSetErrLvl">The sought error level of failed RuleSets</param>
        /// <returns>The RuleReport list repackaged as WonkaPrdProductErrors</returns>
        /// </summary>
        public List<WonkaProductError> GetProductErrors(string psProductId, RULE_SET_ERR_LVL peRuleSetErrLvl = RULE_SET_ERR_LVL.ERR_LVL_SEVERE)
        {
            List<WonkaBizRuleSetReportNode> RuleSetErrors    = new List<WonkaBizRuleSetReportNode>();
            List<WonkaProductError>         ProductErrorList = new List<WonkaProductError>();

            WonkaRefEnvironment WonkaRefEnv = WonkaRefEnvironment.GetInstance();
            if (peRuleSetErrLvl == RULE_SET_ERR_LVL.ERR_LVL_WARNING)
            {
                RuleSetErrors = GetRuleSetWarningFailures();
            }
            else if (peRuleSetErrLvl == RULE_SET_ERR_LVL.ERR_LVL_SEVERE)
            {
                RuleSetErrors = GetRuleSetSevereFailures();
            }

            foreach (WonkaBizRuleSetReportNode RuleSetReport in RuleSetErrors)
            {
                foreach (WonkaBizRuleReportNode RuleReport in RuleSetReport.RuleResults)
                {
                    if (RuleReport.ErrorCode == ERR_CD.CD_FAILURE)
                    {
                        WonkaProductError ProductError = new WonkaProductError();

                        ProductError.ProductId = psProductId;
                        ProductError.AttrName  = WonkaRefEnv.GetAttributeByAttrId(RuleReport.TriggerAttrId).AttrName;

                        ProductError.ErrorMessage =
                            CONST_ERROR_MSG_PREFACE + RuleReport.VerboseError;

                        if (!string.IsNullOrEmpty(RuleSetReport.CustomId))
                        {
                            ProductError.ErrorMessage = "[" + RuleSetReport.CustomId + "] " + ProductError.ErrorMessage;
                        }

                        ProductErrorList.Add(ProductError);
                    }
                }
            }

            return ProductErrorList;
        }

        /// <summary>
        /// 
        /// This method will find the RuleReport for a specific Rule.
        /// 
        /// <param name="pnRuleSetId">The ID of the RuleSet who contains the Rule of interest</param>
        /// <param name="pnRuleId">The ID of the Rule of interest</param>
        /// <returns>The RuleReport that describes the execution of a specific Rule</returns>
        /// </summary>
        public WonkaBizRuleReportNode FindRuleReport(int pnRuleSetId, int pnRuleId)
        {
            WonkaBizRuleReportNode RuleReportNode = null;

            WonkaBizRuleSetReportNode RuleSetReportNode = FindRuleSetReport(pnRuleSetId, false);
            if (RuleSetReportNode != null)
            {
                RuleReportNode = RuleSetReportNode.RuleResults.Where(x => x.RuleID == pnRuleId).FirstOrDefault();
            }

            return RuleReportNode;
        }

        /// <summary>
        /// 
        /// This method will find the RuleReports found within a sought RuleSetReport.  If the RuleSetReport
        /// is not found and if the caller has flagged it, the function will initialize a new one
        /// (with the provided ID) and return its empty list.
        /// 
        /// <param name="pnRuleSetId">The ID of the RuleSet whose report we are interested in</param>
        /// <param name="pbCreateNew">Indicator for whether or not we should create a new RuleSetReport if one is not found</param>
        /// <returns>The list of RuleReports that describes the execution of a specific RuleSet</returns>
        /// </summary>
        public List<WonkaBizRuleReportNode> FindRuleReports(int pnRuleSetId, bool pbCreateNew)
        {
            List<WonkaBizRuleReportNode> RuleReportNodes = null;

            WonkaBizRuleSetReportNode SoughtRuleSetReport = FindRuleSetReport(pnRuleSetId, pbCreateNew);
            if (SoughtRuleSetReport != null)
            {
                RuleReportNodes = SoughtRuleSetReport.RuleResults;
            }

            return RuleReportNodes;
        }

        /// <summary>
        /// 
        /// This method will find the RuleSetReport indicated by the RuleSet ID.  If it's not
        /// found and if the caller has flagged it, the function will initialize a new one
        /// (with the provided ID) and add it to our list of RuleSetReports.
        /// 
        /// <param name="pnRuleSetId">The ID of the RuleSet whose report we are interested in</param>
        /// <param name="pbCreateNew">Indicator for whether or not we should create a new RuleSetReport if one is not found</param>
        /// <returns>The RuleSetReport that describes the execution of a specific RuleSet</returns>
        /// </summary>
        public WonkaBizRuleSetReportNode FindRuleSetReport(int pnRuleSetId, bool pbCreateNew = false)
        {
            WonkaBizRuleSetReportNode RuleSetReportNode = null;

            if (RuleSetResults.Where(x => x.RuleSetID == pnRuleSetId).Count() > 0)
            {
                RuleSetReportNode = RuleSetResults.Where(x => x.RuleSetID == pnRuleSetId).FirstOrDefault();
            }
            else if (pbCreateNew)
            {
                RuleSetReportNode = new WonkaBizRuleSetReportNode() { RuleSetID = pnRuleSetId };
                RuleSetResults.Add(RuleSetReportNode);
            }
            else
            {
                throw new WonkaBizRuleException(pnRuleSetId, -1, "ERROR!  This RuleSetReport does not exist!");
            }

            return RuleSetReportNode;
        }

        /// <summary>
        /// 
        /// This method will score the execution of a RuleSet, setting its status (success, failure, etc.)
        /// on the corresponding RuleSetReport.
        /// 
        /// <param name="pnRuleSetId">The ID of the RuleSet whose report we are interested in</param>
        /// <param name="psRuleSetDesc">The description of the RuleSet (that we are passing onto the report)</param>
        /// <param name="psCustomId">The customId of the RuleSet (that we are passing onto the report)</param>
        /// <param name="peRuleSetErrCd">The status code of the target RuleSet's execution</param>
        /// <returns>Indicates whether or not we successfully set the RuleSetReport</returns>
        /// </summary>
        public bool SetRuleSetStatus(int pnRuleSetId, string psRuleSetDesc, string psCustomId, ERR_CD peRuleSetErrCd)
        {
            bool bResult = true;

            WonkaBizRuleSetReportNode RuleSetReport = FindRuleSetReport(pnRuleSetId, true);
            if (RuleSetReport != null)
            {
                RuleSetReport.RuleSetDescription = psRuleSetDesc;
                RuleSetReport.ErrorCode          = peRuleSetErrCd;
                RuleSetReport.CustomId           = psCustomId;
            }

            return bResult;
        }

        #endregion

        #region Properties
        public WonkaBizRuleSet LastRuleSetExecuted { get; set; }

        public ERR_CD OverallRuleTreeResult { get; set; }        

        private Dictionary<long, ERR_CD> OverallRulesetResults { get; set; }

        private List<WonkaBizRuleSetReportNode> RuleSetResults { get; set; }

        private List<WonkaBizRuleSetReportNode> RuleSetFailures { get; set; }
        #endregion
    }
}
