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
    public class WonkaBizQueryRule: CustomOperatorRule
    {
        public WonkaBizQueryRule(int pnRuleID, WonkaBizSource poCustomOpSource) :
            base(pnRuleID, TARGET_RECORD.TRID_NEW_RECORD, 0, null, null, poCustomOpSource)
        {
            this.CustomOpDelegate = ExecuteQuery;
        }

        public WonkaBizQueryRule(int pnRuleID, int pnTargetAttrId, string psCustomOpName, WonkaBizSource poCustomOpSource) : 
            base(pnRuleID, TARGET_RECORD.TRID_NEW_RECORD, pnTargetAttrId, psCustomOpName, null, poCustomOpSource)
        {
            this.CustomOpDelegate = ExecuteQuery;
        }

        /// <summary>
        /// 
        /// This method will do a data lookup of the value by calling a query on a table in the database.
        ///
        /// NOTE: UNDER CONSTRUCTION (AND, YES, HORRIBLY INEFFICIENT FOR NOW)
        ///
        /// </summary>
        public string ExecuteQuery(string psArg1, string psArg2, string psArg3, string psArg4)
        {
            string sResultValue = "";

            string sConnString = "Data Source=" + this.CustomOpContractSource.SqlServer +
                                 ";Initial Catalog=" + this.CustomOpContractSource.SqlDatabase +
                                 ";User ID=" + this.CustomOpContractSource.SqlUsername +
                                 ";Password=" + this.CustomOpContractSource.SqlPassword;

            // NOTE: Use parameters in prepared statement with query here?
            string sSqlQuery = 
                String.Format(this.CustomOpContractSource.SqlQueryOrProcedure, psArg1, psArg2, psArg3, psArg4);

            using (SqlConnection DbConn = new SqlConnection(sConnString))
            {
                try
                {
                    DbConn.Open();

                    using (SqlCommand QueryCmd = new SqlCommand(sSqlQuery, DbConn))
                    {
                        using (SqlDataReader dataReader = QueryCmd.ExecuteReader())
                        {
                            if (dataReader.Read())
                            {
                                // NOTE: For the time being, it will return the first value of the first column
                                if (!dataReader.IsDBNull(0))
                                    sResultValue = dataReader[0].ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // NOTE: Do something here?
                }
            }            

            return sResultValue;
        }

        /// <summary>
        /// 
        /// This method will assist with serializing the data to the chain
        ///
        /// NOTE: UNDER CONSTRUCTION
        ///
        /// </summary>
        public HashSet<string> RetrieveDomainSet()
        {
            var DomainSet = new HashSet<string>();

            if (IsDomainQuery)
            {
                // NOTE: Populate the set here
            }

            return DomainSet;
        }

        #region PROPERTIES

        public bool IsDomainQuery { get; set; }

        #endregion
    }
}
