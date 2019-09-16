using System;
using System.Threading;
using System.Numerics;

using Nethereum.Contracts.ContractHandlers;
using Nethereum.StandardTokenEIP20;
using Nethereum.StandardTokenEIP20.ContractDefinition;
using Nethereum.Web3;

using WonkaBre;
using WonkaBre.Triggers;

namespace WonkaEth.Triggers
{
	public class WonkaEthEIP20TransferTrigger : WonkaEthContractTriggerBase, ISuccessTrigger
	{
        protected long       mnTotalSupply;
        protected BigInteger mnTransferAmt;

        protected string msTokenName;
        protected string msTokenSymbol;
        protected string msReceiverAddress;

		private StandardTokenService moEIP20Service;

        public WonkaEthEIP20TransferTrigger(Web3 poWeb3, long pnTotalSupply, string psTokenName, string psTokenSymbol, string psReceiverAddress, long pnTransferAmount, CancellationTokenSource poCancelToken = null) 
            : base(poWeb3, poCancelToken)
		{
            mnTotalSupply = pnTotalSupply;
			mnTransferAmt = pnTransferAmount;

			msTokenName       = psTokenName;
			msTokenSymbol     = psTokenSymbol;
			msReceiverAddress = psReceiverAddress;

            var deploymentContract = new EIP20Deployment()
            {
                InitialAmount = new System.Numerics.BigInteger(pnTotalSupply),
                TokenName     = psTokenName,
                TokenSymbol   = psTokenSymbol
            };

            moEIP20Service = StandardTokenService.DeployContractAndGetServiceAsync(poWeb3, deploymentContract).Result;
            moContract     = moEIP20Service.ContractHandler;
        }

        public WonkaEthEIP20TransferTrigger(Web3 poWeb3, EIP20Deployment moDeployData, string psContractAddress, string psReceiverAddress, long pnTransferAmount, CancellationTokenSource poCancelToken = null)
            : base(poWeb3, psContractAddress, poCancelToken)
        {
            mnTotalSupply = Convert.ToInt64(moDeployData.InitialAmount);
			mnTransferAmt = pnTransferAmount;

			msTokenName       = moDeployData.TokenName;
			msTokenSymbol     = moDeployData.TokenSymbol;
			msReceiverAddress = psReceiverAddress;

            moEIP20Service = new StandardTokenService(poWeb3, psContractAddress);
            moContract     = moEIP20Service.ContractHandler;
        }

		public void Execute()
		{
            var transferReceipt =
                moEIP20Service.TransferRequestAndWaitForReceiptAsync(msReceiverAddress, mnTransferAmt, moCancelToken).Result;
		}
	}
}
