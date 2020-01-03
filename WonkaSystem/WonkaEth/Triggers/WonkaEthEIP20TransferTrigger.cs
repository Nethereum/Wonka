using System;
using System.Numerics;
using System.Threading;

using Nethereum.Contracts.ContractHandlers;
using Nethereum.StandardTokenEIP20;
using Nethereum.StandardTokenEIP20.ContractDefinition;
using Nethereum.Web3;

using Wonka.BizRulesEngine;
using Wonka.BizRulesEngine.Triggers;

namespace Wonka.Eth.Triggers
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

        public WonkaEthEIP20TransferTrigger(Web3 poWeb3, string psContractAddress, string psReceiverAddress, long pnTransferAmount, CancellationTokenSource poCancelToken = null, EIP20Deployment poDeployData = null)
            : base(poWeb3, psContractAddress, poCancelToken)
        {
			mnTransferAmt     = pnTransferAmount;
            msReceiverAddress = psReceiverAddress;

            if (poDeployData != null)
            {
                msTokenName   = poDeployData.TokenName;
                msTokenSymbol = poDeployData.TokenSymbol;
                mnTotalSupply = Convert.ToInt64(poDeployData.InitialAmount);
            }
            else
            {
                msTokenName   = msTokenSymbol = "";
                mnTotalSupply = 0;
            }

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
