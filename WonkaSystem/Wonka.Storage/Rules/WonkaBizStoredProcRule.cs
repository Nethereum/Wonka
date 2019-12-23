using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Wonka.BizRulesEngine;
using Wonka.BizRulesEngine.RuleTree;
using Wonka.BizRulesEngine.RuleTree.RuleTypes;

namespace Wonka.Storage.Rules
{
    public class WonkaBizStoredProcRule : CustomOperatorRule
    {
        public WonkaBizStoredProcRule(int pnRuleID, WonkaBizSource poCustomOpSource) :
            base(pnRuleID, TARGET_RECORD.TRID_NEW_RECORD, 0, null, null, poCustomOpSource)
        {
            this.CustomOpDelegate = ExecuteStoredProcedure;
        }

        public WonkaBizStoredProcRule(int pnRuleID, int pnTargetAttrId, string psCustomOpName, WonkaBizSource poCustomOpSource) :
            base(pnRuleID, TARGET_RECORD.TRID_NEW_RECORD, pnTargetAttrId, psCustomOpName, null, poCustomOpSource)
        {
            this.CustomOpDelegate = ExecuteStoredProcedure;
        }

        /// <summary>
        /// 
        /// This method will do a data lookup of the value by calling a stored procedure in the database.
        ///
        /// NOTE: UNDER CONSTRUCTION
        ///
        /// </summary>
        public string ExecuteStoredProcedure(string psArg1, string psArg2, string psArg3, string psArg4)
        {
            string sResultValue = "";

            string sConnString = "Data Source=" + this.CustomOpContractSource.SqlServer +
                                 ";Initial Catalog=" + this.CustomOpContractSource.SqlDatabase +
                                 ";User ID=" + this.CustomOpContractSource.SqlUsername +
                                 ";Password=" + this.CustomOpContractSource.SqlPassword;

            // NOTE: Use parameters in prepared statement with query here?
            string sSqlQuery = this.CustomOpContractSource.SqlQueryOrProcedure;

            using (SqlConnection DbConn = new SqlConnection(sConnString))
            {
                try
                {
                    DbConn.Open();

                    /*
                     * NOTE: Call stored procedure here
                     */
                }
                catch (Exception ex)
                {
                    // NOTE: Do something here?
                }
            }

            return sResultValue;
        }
    }
}
