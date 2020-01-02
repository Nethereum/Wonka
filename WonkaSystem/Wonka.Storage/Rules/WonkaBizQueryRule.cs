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
        public WonkaBizQueryRule(int pnRuleID, WonkaBizSource poCustomOpSource, bool pbCacheDomain = false) :
            base(pnRuleID, TARGET_RECORD.TRID_NEW_RECORD, 0, null, null, poCustomOpSource)
        {
            Init(pbCacheDomain);
        }

        public WonkaBizQueryRule(int pnRuleID, int pnTargetAttrId, string psCustomOpName, WonkaBizSource poCustomOpSource, bool pbCacheDomain = false) : 
            base(pnRuleID, TARGET_RECORD.TRID_NEW_RECORD, pnTargetAttrId, psCustomOpName, null, poCustomOpSource)
        {
            Init(pbCacheDomain);
        }

        /// <summary>
        /// 
        /// This method will do a data lookup of the value by looking for the value in the cached domain.
        ///
        /// </summary>
        public string ExecuteCacheCheck(string psArg1, string psArg2, string psArg3, string psArg4)
        {
            string sResultValue = "";

            if (IsDomainQuery && !String.IsNullOrEmpty(psArg1) && CachedDomain.Contains(psArg1))
            {
                sResultValue = psArg1;
            }

            return sResultValue;
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

            try
            {
                using (SqlConnection DbConn = GetConnection())
                {
                    DbConn.Open();

                    // NOTE: Use parameters in prepared statement with query here?
                    string sSqlQuery =
                        String.Format(this.CustomOpContractSource.SqlQueryOrProcedure, psArg1, psArg2, psArg3, psArg4);

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
            }
            catch (Exception ex)
            {
                // NOTE: Do something here?
            }

            return sResultValue;
        }

        /// <summary>
        /// 
        /// This method will assist with retrieving the entire domain
        ///
        /// NOTE: UNDER CONSTRUCTION
        ///
        /// </summary>
        public HashSet<string> RetrieveDomainSet()
        {
            var DomainSet = new HashSet<string>();

            if (IsDomainQuery)
            {
                try
                {
                    using (SqlConnection DbConn = GetConnection())
                    {
                        DbConn.Open();

                        // NOTE: Use parameters in prepared statement with query here?
                        string sSqlQuery =
                            String.Format(this.CustomOpContractSource.SqlQueryOrProcedure);

                        using (SqlCommand QueryCmd = new SqlCommand(sSqlQuery, DbConn))
                        {
                            using (SqlDataReader dataReader = QueryCmd.ExecuteReader())
                            {
                                if (dataReader.Read())
                                {
                                    // NOTE: For the time being, it will return the first value of the first column
                                    if (!dataReader.IsDBNull(0))
                                    {
                                        string sResultValue = dataReader[0].ToString();
                                        CachedDomain.Add(sResultValue);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // NOTE: Do something here?
                }
            }

            return DomainSet;
        }

        #region SUPPORT METHODS

        private SqlConnection GetConnection()
        {
            string sConnString = "Data Source=" + this.CustomOpContractSource.SqlServer +
                     ";Initial Catalog=" + this.CustomOpContractSource.SqlDatabase +
                     ";User ID=" + this.CustomOpContractSource.SqlUsername +
                     ";Password=" + this.CustomOpContractSource.SqlPassword;

            return new SqlConnection(sConnString);
        }

        protected void Init(bool pbCacheDomain)
        {
            this.CachedDomain = new HashSet<string>();

            if (pbCacheDomain)
            {
                RetrieveDomainSet();
                this.CustomOpDelegate = ExecuteCacheCheck;
            }
            else
            {
                this.CustomOpDelegate = ExecuteQuery;
            }
        }

        #endregion

        #region PROPERTIES

        public bool IsDomainQuery { get; set; }

        private HashSet<string> CachedDomain { get; set; }

        #endregion
    }
}
