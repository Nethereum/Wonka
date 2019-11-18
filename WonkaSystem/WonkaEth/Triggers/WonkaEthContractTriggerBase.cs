using System;
using System.Threading;

using Nethereum.Contracts.ContractHandlers;
using Nethereum.Web3;

namespace Wonka.Eth.Triggers
{
    public class WonkaEthContractTriggerBase
    {
        protected Web3                    moWeb3;
        protected ContractHandler         moContract;
        protected CancellationTokenSource moCancelToken;

        public WonkaEthContractTriggerBase(Web3 poWeb3, CancellationTokenSource poCancelToken = null)
		{
			moWeb3 = poWeb3;

            moContract    = null;
            moCancelToken = poCancelToken;
        }

		public WonkaEthContractTriggerBase(Web3 poWeb3, string psContractAddress, CancellationTokenSource poCancelToken = null)
		{
			moWeb3 = poWeb3;

            moContract    = moWeb3.Eth.GetContractHandler(psContractAddress);
            moCancelToken = poCancelToken;
        }

    }
}
