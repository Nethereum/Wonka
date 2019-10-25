using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3.Accounts;

using Wonka.BizRulesEngine;
using Wonka.BizRulesEngine.RuleTree;

namespace WonkaEth.Extensions
{
    /// <summary>
    /// 
    /// This extensions class provides the functionality for Nethereum-related operations, so that it can be invoked
    /// from within the Wonka rules engine
    /// 
    /// </summary>
    public static class WonkaOpExtensions
    {
        private static Dictionary<WonkaBizRulesEngine, Nethereum.Web3.Web3> EngineWeb3Accounts = new Dictionary<WonkaBizRulesEngine, Nethereum.Web3.Web3>();

        /// <summary>
        /// 
        /// This method will register the default set of standard operations (especially Nethereum-related ones) that can be 
        /// invoked from within the Wonka rules engine
        /// 
        /// <param name="poEngine">The target instance of an engine</param>
        /// <returns>None</returns>
        /// </summary>
        public static void SetDefaultStdOps(this WonkaBizRulesEngine poEngine, string psPassword, string psWeb3HttpUrl = null)
        {
            var account = new Account(psPassword);

            Nethereum.Web3.Web3 web3 = null;
            if (!String.IsNullOrEmpty(psWeb3HttpUrl))
                web3 = new Nethereum.Web3.Web3(account, psWeb3HttpUrl);
            else
                web3 = new Nethereum.Web3.Web3(account);

            EngineWeb3Accounts[poEngine] = web3;

			Dictionary<STD_OP_TYPE, WonkaBizRulesEngine.RetrieveStdOpValDelegate> DefaultStdOpMap =
				new Dictionary<STD_OP_TYPE, WonkaBizRulesEngine.RetrieveStdOpValDelegate>();

			DefaultStdOpMap[STD_OP_TYPE.STD_OP_BLOCK_NUM] = GetCurrentBlockNum;

			poEngine.StdOpMap = DefaultStdOpMap;
		}

        #region Default Op Functions

        private static string GetCurrentBlockNum(WonkaBizRulesEngine poEngine, string psUnusedVal)
        {
            string sCurrBlockNum = string.Empty;

            if (EngineWeb3Accounts.ContainsKey(poEngine))
            {
                Nethereum.Web3.Web3 EngineWeb3 = EngineWeb3Accounts[poEngine];

                sCurrBlockNum = EngineWeb3.Eth.Blocks.GetBlockNumber.SendRequestAsync().Result.HexValue.ToString();
            }

			if (sCurrBlockNum.HasHexPrefix())
				sCurrBlockNum = sCurrBlockNum.RemoveHexPrefix();

            return sCurrBlockNum;
        }

        #endregion

    }
}
