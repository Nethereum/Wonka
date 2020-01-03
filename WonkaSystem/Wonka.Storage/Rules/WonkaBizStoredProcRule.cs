using System;
using System.Collections.Generic;
using System.Data;
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
        public const string CONST_OUT_PARAM_RET_VALUE = "@RETURN_VALUE";

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

            string sConnString = "Data Source=" + this.CustomOpSource.SqlServer +
                                 ";Initial Catalog=" + this.CustomOpSource.SqlDatabase +
                                 ";User ID=" + this.CustomOpSource.SqlUsername +
                                 ";Password=" + this.CustomOpSource.SqlPassword;

            // NOTE: Use parameters in prepared statement with query here?
            string sSqlQuery = this.CustomOpSource.SqlQueryOrProcedure;

            using (SqlConnection DbConn = new SqlConnection(sConnString))
            {
                try
                {
                    DbConn.Open();

                    using (SqlCommand StoredProcCmd = new SqlCommand())
                    {
                        StoredProcCmd.CommandType = CommandType.StoredProcedure;
                        StoredProcCmd.CommandText = sSqlQuery;
                        StoredProcCmd.Parameters.Clear();

                        string[] Args1 = psArg1.Split('=');
                        string[] Args2 = psArg2.Split('=');
                        string[] Args3 = psArg3.Split('=');
                        string[] Args4 = psArg4.Split('=');

                        AddParameter(StoredProcCmd, Args1);
                        AddParameter(StoredProcCmd, Args2);
                        AddParameter(StoredProcCmd, Args3);
                        AddParameter(StoredProcCmd, Args4);

                        StoredProcCmd.Parameters.Add(CONST_OUT_PARAM_RET_VALUE, SqlDbType.VarChar, 128);
                        StoredProcCmd.Parameters[CONST_OUT_PARAM_RET_VALUE].Direction = ParameterDirection.ReturnValue;

                        using (SqlDataReader ProcReader = StoredProcCmd.ExecuteReader())
                        {
                            if (ProcReader.Read())
                            {
                                if (StoredProcCmd.Parameters[CONST_OUT_PARAM_RET_VALUE].Value != DBNull.Value)
                                {
                                    sResultValue = 
                                        StoredProcCmd.Parameters[CONST_OUT_PARAM_RET_VALUE].Value.ToString();
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

            return sResultValue;
        }

        #region Support Methods

        private void AddParameter(SqlCommand poStoredProcedure, string[] pArgs)
        {
            if (pArgs.Length >= 2)
            {
                poStoredProcedure.Parameters.Add(pArgs[0], SqlDbType.VarChar, 128);
                poStoredProcedure.Parameters[pArgs[0]].Value = pArgs[1];
            }
        }

        #endregion
    }
}
