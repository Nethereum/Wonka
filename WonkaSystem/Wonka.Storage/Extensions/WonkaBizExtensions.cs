using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Wonka.BizRulesEngine;
using Wonka.BizRulesEngine.RuleTree;
using Wonka.BizRulesEngine.RuleTree.RuleTypes;
using Wonka.BizRulesEngine.Triggers;
using Wonka.Eth.Extensions;
using Wonka.Eth.Triggers;
using Wonka.MetaData;
using Wonka.Product;

using Wonka.Storage.Rules;

namespace Wonka.Storage.Extensions
{
    public static class WonkaBizExtensions
    {
        /// <summary>
        /// 
        /// This method will build a trigger that will transfer tokens from a holding account to a receiver.
        /// 
        /// NOTE: UNDER CONSTRUCTION
        /// 
        /// </summary>
        public static ISuccessTrigger BuildTokenTransferTrigger(this WonkaBizSource poSource, string psRecvAddress, long pnTransferAmt, string psWeb3Url = "", System.Threading.CancellationTokenSource poTokenSrc = null)
        {
            Nethereum.Web3.Web3 web3 = WonkaExtensions.GetWeb3(poSource.Password, psWeb3Url);

            var TransferTrigger = new WonkaEthEIP20TransferTrigger(web3, poSource.ContractAddress, psRecvAddress, pnTransferAmt, poTokenSrc);

            return TransferTrigger;
        }

        /// <summary>
        /// 
        /// This method will create a custom operator rule that will determine whether an attribute's value falls within a domain defined by the
        /// results of a query on a database.
        /// 
        /// NOTE: UNDER CONSTRUCTION
        /// 
        /// </summary>
        public static CustomOperatorRule BuildSqlQueryRule(this WonkaBizSource poSource, int pnRuleID, bool pbQueryDomainRule = false)
        {
            var QueryRule = new WonkaBizQueryRule(pnRuleID, poSource) { IsDomainQuery = pbQueryDomainRule };

            return QueryRule;
        }

        /// <summary>
        /// 
        /// This method will create a custom operator rule that will determine whether an attribute's value falls within a domain defined by the
        /// results of a query on a database.
        /// 
        /// NOTE: UNDER CONSTRUCTION
        /// 
        /// </summary>
        public static CustomOperatorRule BuildSqlQueryRule(this WonkaBizSource poSource, int pnRuleID, int pnTargetAttrId, string psCustomOpName, bool pbQueryDomainRule = false)
        {
            var QueryRule = new WonkaBizQueryRule(pnRuleID, pnTargetAttrId, psCustomOpName, poSource) { IsDomainQuery = pbQueryDomainRule };

            return QueryRule;
        }

        /// <summary>
        /// 
        /// This method will provide default functionality on the retrieval delegate of each Source 
        /// in an engine's SourceMap .
        /// 
		/// <param name="poEngine">The Wonka.NET instance that represents the instance on the chain</param>
        /// <param name="psWeb3Url">The URL for the Ethereum client to which we want to connect</param>
        /// <returns>N/A</returns>
        /// </summary>
        /// 
        public static void SetDefaultSourceRetrievalDelegates(this WonkaBizRulesEngine poEngine, string psWeb3Url = "")
        {
            if ((poEngine.SourceMap != null) && (poEngine.SourceMap.Count > 0))
            {
                foreach (string sTmpAttrName in poEngine.SourceMap.Keys)
                {
                    var TmpSource = poEngine.SourceMap[sTmpAttrName];

                    if (TmpSource.RetrievalDelegate == null)
                    {
                        if (!String.IsNullOrEmpty(TmpSource.DefaultWeb3Url) && !String.IsNullOrEmpty(psWeb3Url))
                            TmpSource.DefaultWeb3Url = psWeb3Url;

                        if (!String.IsNullOrEmpty(TmpSource.ContractAddress))
                        {
                            TmpSource.RetrievalDelegate = WonkaStorageExtensions.GetAttrValue;
                        }
                        else if (!String.IsNullOrEmpty(TmpSource.APIWebUrl))
                        {
                            TmpSource.RetrievalDelegate = WonkaStorageExtensions.GetAttrValueViaWebMethod;
                        }
                        else if (!String.IsNullOrEmpty(TmpSource.SqlDatabase))
                        {
                            // NOTE: Set delegate that grabs data from SQL Stored procedure
                        }
                    }
                }
            }
        }
    }
}
