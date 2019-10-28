using System.Collections.Generic;
using System.Linq;
using System.Text;

using Wonka.BizRulesEngine.Reporting;
using Wonka.BizRulesEngine.RuleTree;
using Wonka.Product;

namespace Wonka.BizRulesEngine
{
    /// <summary>
    /// 
    /// This class provides the functionality needed to run the rules engine, in particular applying a WonkaBreRuleSet 
    /// (or even a whole RuleTree) against the Product(s).  It will do the actual work of traversing a RuleTree and 
    /// applying the various RuleSets and Rules against the Product(s) in order to evaluate them as valid or not.
    /// 
    /// </summary>
    public class WonkaBizRuleMediator
    {
        private WonkaBizRuleMediator()
        {}

        /// <summary>
        /// 
        /// This method will apply the Rules of the RuleSet to either one or both of the incoming Product record 
        /// and the current Product record.  The collective result of those applied Rules will then determine
        /// whether or not the RuleSet was evaluated as a success.
        /// 
        /// <param name="poTargetRuleSet">The RuleSet whose rules are being applied against the provided records</param>
        /// <param name="poIncomingProduct">The incoming Product record</param>
        /// <param name="poCurrentProduct">The current Product record (i.e., in the database)</param>
        /// <param name="poRuleTreeReport">The report that will contain all evaluations of RuleSets in the RuleTree</param>
        /// <param name="poRuleSetErrorMessage">The buffer that will contain an error message if the 'poTargetRuleSet' fails</param>
        /// <returns>Indicates whether or not the RuleSet evaluated to a success</returns>
        /// </summary>
        private static bool MediateRulesExecution(WonkaBizRuleSet        poTargetRuleSet, 
                                                  WonkaProduct           poIncomingProduct,
                                                  WonkaProduct           poCurrentProduct,
                                                  WonkaBizRuleTreeReport poRuleTreeReport,
                                                  StringBuilder          poRuleSetErrorMessage)
        {
            bool bRuleSetResult = true;

            StringBuilder RuleErrorMsgBuilder = new StringBuilder();
            List<bool>    RuleResultList      = new List<bool>();

            foreach (WonkaBizRule TempRule in poTargetRuleSet.EvaluativeRules)
            {
                bool   bRuleResult      = true;
                string sRuleResult      = string.Empty;
                string sFinalRuleErrMsg = string.Empty;

                RuleErrorMsgBuilder.Clear();
                bRuleResult = TempRule.Execute(poIncomingProduct, poCurrentProduct, RuleErrorMsgBuilder);

                if (TempRule.IsPassive)
                {
                    if (TempRule.NotOperator)
                    {
                        bRuleResult = !bRuleResult;
                    }
                }

                RuleResultList.Add(bRuleResult);

                if (bRuleResult)
                {
                    poRuleSetErrorMessage.Append("SUCCESS");
                }
                else
                {
                    var RuleSetReport =
                        poRuleTreeReport.FindRuleSetReport(poTargetRuleSet.RuleSetId, true);

                    if (RuleSetReport != null)
                    {
                        if (poTargetRuleSet.ErrorSeverity == RULE_SET_ERR_LVL.ERR_LVL_SEVERE)
                        {
                            sRuleResult = "SEVERE";
                            RuleSetReport.SevereFailureCount++;
                        }
                        else
                        {
                            sRuleResult = "WARNING";
                            RuleSetReport.WarningFailureCount++;
                        }
                    }

                    sFinalRuleErrMsg = RuleErrorMsgBuilder.ToString() + " / " + poTargetRuleSet.CustomFailureMsg;
                }

                poRuleTreeReport.ArchiveRuleExecution(TempRule,
                                                      bRuleResult ? ERR_CD.CD_SUCCESS : ERR_CD.CD_FAILURE,
                                                      sRuleResult,
                                                      sFinalRuleErrMsg);
            }

            // Calculate the final outcome of the RuleSet
            if (RuleResultList.Count > 0)
            {
                bRuleSetResult = RuleResultList[0];

                foreach (bool bTempRuleResult in RuleResultList)
                {
                    if (poTargetRuleSet.RulesEvalOperator == RULE_OP.OP_AND)
                    {
                        bRuleSetResult = bRuleSetResult && bTempRuleResult;
                    }
                    else if (poTargetRuleSet.RulesEvalOperator == RULE_OP.OP_OR)
                    {
                        bRuleSetResult = bRuleSetResult || bTempRuleResult;
                    }
                    else
                    {
                        bRuleSetResult = bRuleSetResult && bTempRuleResult;
                    }
                }
            }

            // Only apply the assertive rules if the evaluative rules are applied successfully
            if (bRuleSetResult && (poTargetRuleSet.AssertiveRules.Count() > 0))
            {
                foreach (WonkaBizRule TempRule in poTargetRuleSet.AssertiveRules)
                {
                    bool bRuleResult = true;

                    string sRuleResult      = string.Empty;
                    string sFinalRuleErrMsg = string.Empty;

                    RuleErrorMsgBuilder.Clear();
                    bRuleResult = TempRule.Execute(poIncomingProduct, poCurrentProduct, RuleErrorMsgBuilder);

                    RuleResultList.Add(bRuleResult);

                    poRuleSetErrorMessage.Append("SUCCESS");

                    poRuleTreeReport.ArchiveRuleExecution(TempRule,
                                                          bRuleResult ? ERR_CD.CD_SUCCESS : ERR_CD.CD_FAILURE,
                                                          sRuleResult,
                                                          sFinalRuleErrMsg);
                }
            }

            return bRuleSetResult;
        }

        /// <summary>
        /// 
        /// This method helps to direct the navigation of a branch within the RuleTree.  If the rules of 'poTargetRuleSet'
        /// evaluate to a success when applied to the records, then we will continue to traverse the branch by enumerating
        /// through the child branches that sprout from this node (i.e., by calling this method again recursively).  If not, 
        /// we will stop the traversal here and return back to the parent node of 'poTargetRuleSet'.
        /// 
        /// <param name="poTargetRuleSet">The RuleSet whose rules are being applied against the provided records</param>
        /// <param name="poIncomingProduct">The incoming Product record</param>
        /// <param name="poCurrentProduct">The current Product record (i.e., in the database)</param>
        /// <param name="poRuleTreeReport">The report that will contain all evaluations of RuleSets in the RuleTree</param>
        /// <returns>Indicates whether or not the RuleSet evaluated to a success</returns>
        /// </summary>
        private static bool MediateRuleSetExecution(WonkaBizRuleSet        poTargetRuleSet,
                                                    WonkaProduct           poIncomingProduct,
                                                    WonkaProduct           poCurrentProduct,
                                                    WonkaBizRuleTreeReport poRuleTreeReport)
        {
            bool   bTotalRuleSetResult  = true;
            bool   bTempRulesResult     = true;
            bool   bTempRuleSetResult   = true;
            bool   bTraverseChildren    = true;
            string sGeneralRuleSetError = "ERROR!  One of the rules failed.";

            StringBuilder RuleSetErrorBuilder = new StringBuilder();

            poRuleTreeReport.LastRuleSetExecuted = poTargetRuleSet;

            /*
             * GOOD PLACE FOR TESTING
             * 
            if (!String.IsNullOrEmpty(poTargetRuleSet.Description))
            {
                if (poTargetRuleSet.Description.Contains("Test RuleSet"))
                {
                    int x = 1;
                }
            }
            */

            bTempRulesResult =
                MediateRulesExecution(poTargetRuleSet, 
                                      poIncomingProduct, 
                                      poCurrentProduct, 
                                      poRuleTreeReport, 
                                      RuleSetErrorBuilder);

            if (bTempRulesResult)
            {
                poRuleTreeReport.SetRuleSetStatus(poTargetRuleSet.RuleSetId, 
                                                  poTargetRuleSet.Description,
                                                  poTargetRuleSet.CustomId, 
                                                  ERR_CD.CD_SUCCESS);
            }
            else
            {
                poRuleTreeReport.SetRuleSetStatus(poTargetRuleSet.RuleSetId,
                                                  poTargetRuleSet.Description,
                                                  poTargetRuleSet.CustomId,
                                                  ERR_CD.CD_FAILURE);

                var TargetRuleSetReport = poRuleTreeReport.FindRuleSetReport(poTargetRuleSet.RuleSetId);

                if ((TargetRuleSetReport.SevereFailureCount > 0) || (TargetRuleSetReport.WarningFailureCount > 0))
                {
                    poRuleTreeReport.AddResultSetFailure(poTargetRuleSet, ERR_CD.CD_FAILURE, sGeneralRuleSetError);
                }

                bTraverseChildren = false;
            }

            if (!bTraverseChildren)
            {
                // We should return here, preventing the recursion that will further traverse the branches of the RuleTree
                return bTotalRuleSetResult;
            }

            foreach (WonkaBizRuleSet ChildRuleSet in poTargetRuleSet.ChildRuleSets)
            {
                bTempRuleSetResult =
                    MediateRuleSetExecution(ChildRuleSet, poIncomingProduct, poCurrentProduct, poRuleTreeReport);

                if (!bTempRuleSetResult)
                {
                    // NOTE: Currently, this condition will never happen...but at some point, should it?
                }
            }

            return bTotalRuleSetResult;
        }

        /// <summary>
        /// 
        /// This method will begin the evaluation of a RuleTree when applied to both the incoming and current Product
        /// records.
        /// 
        /// <param name="poRootRuleSet">The root RuleSet of the RuleTree whose rules are being applied against the provided records</param>
        /// <param name="poIncomingProduct">The incoming Product record</param>
        /// <param name="poCurrentProduct">The current Product record (i.e., in the database)</param>
        /// <param name="poRuleTreeReport">The report that will contain all evaluations of RuleSets in the RuleTree</param>
        /// <returns>Indicates whether or not the RuleSet evaluated to a success</returns>
        /// </summary>
        public static bool MediateRuleTreeExecution(WonkaBizRuleSet        poRootRuleSet,
                                                    WonkaProduct           poIncomingProduct,
                                                    WonkaProduct           poCurrentProduct,
                                                    WonkaBizRuleTreeReport poRuleTreeReport)
        {
            bool bRuleTreeResult = true;

            bRuleTreeResult =
                MediateRuleSetExecution(poRootRuleSet, poIncomingProduct, poCurrentProduct, poRuleTreeReport);

            // NOTE: Should we do anything else here?

            return bRuleTreeResult;
        }
    }
}
