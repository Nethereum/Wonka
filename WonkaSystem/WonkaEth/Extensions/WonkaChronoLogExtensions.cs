using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Wonka.Eth.Autogen.ChronoLog;
using Wonka.Eth.Contracts;
using Wonka.Eth.Orchestration;

using Wonka.MetaData;
using Wonka.Product;

namespace Wonka.Eth.Extensions
{
    /// <summary>
    /// 
    /// This extensions class provides the functionality to handle all activities that will be invoked on
    /// behalf of a RuleGrove.
    /// 
    /// </summary>
    public static class WonkaChronoLogExtensions
    {
        private static WonkaRefEnvironment moWonkaRevEnv = WonkaRefEnvironment.GetInstance();

        /// <summary>
        ///
		/// NOTE: UNDER CONSTRUCTION
		/// 
        /// This method will log the Wonka Report to an instance of the ChronoLog contract.
        /// 
        /// <param name=""></param>
        /// <returns></returns>
        /// </summary>
        public static async Task<string> WriteToChronoLog(this Wonka.Eth.Extensions.RuleTreeReport poReport,
                                                       Wonka.Eth.Init.WonkaEthEngineInitialization poEngineInitData,
                                                                                            string psChronoLogContractAddr,
                                                                         AddChronoLogEventFunction poChronoLogEventFunction)
        {
            var account = new Nethereum.Web3.Accounts.Account(poEngineInitData.EthPassword);

            Nethereum.Web3.Web3 SenderWeb3;

            if (!String.IsNullOrEmpty(poEngineInitData.Web3HttpUrl))
                SenderWeb3 = new Nethereum.Web3.Web3(account, poEngineInitData.Web3HttpUrl);
            else
                SenderWeb3 = new Nethereum.Web3.Web3(account);

            var addLogHandler = SenderWeb3.Eth.GetContractTransactionHandler<AddChronoLogEventFunction>();

            var receipt = await addLogHandler.SendRequestAndWaitForReceiptAsync(psChronoLogContractAddr, poChronoLogEventFunction);

            return receipt.TransactionHash;
        }

    }
}