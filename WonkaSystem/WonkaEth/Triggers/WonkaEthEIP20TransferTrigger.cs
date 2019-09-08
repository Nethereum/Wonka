using System;
using System.Numerics;

using Nethereum.Web3;
using Nethereum.StandardTokenEIP20;
using Nethereum.StandardTokenEIP20.ContractDefinition;

using WonkaBre;
using WonkaBre.Triggers;

namespace WonkaEth.Triggers
{
	public class WonkaEthEIP20TransferTrigger : ISuccessTrigger
	{
		private long       mnTotalSupply;
		private BigInteger mnTransferAmt;

		private string msTokenName;
		private string msTokenSymbol;
		private string msReceiverAddress;

		private Web3                 moWeb3;
		private EIP20Deployment      moEIP20Contract;
		private StandardTokenService moEIP20Service;

		public WonkaEthEIP20TransferTrigger(Web3 poWeb3, long pnTotalSupply, string psTokenName, string psTokenSymbol, string psReceiverAddress, long pnTransferAmount)
		{
			mnTotalSupply = pnTotalSupply;
			mnTransferAmt = pnTransferAmount;

			msTokenName       = psTokenName;
			msTokenSymbol     = psTokenSymbol;
			msReceiverAddress = psReceiverAddress;

			moWeb3 = poWeb3;

			var deploymentContract = new EIP20Deployment()
			{
				InitialAmount = new System.Numerics.BigInteger(pnTotalSupply),
				TokenName     = psTokenName,
				TokenSymbol   = psTokenSymbol
			};

			moEIP20Service = StandardTokenService.DeployContractAndGetServiceAsync(poWeb3, deploymentContract).Result;
		}

		public void Execute()
		{
			var transferReceipt =
				moEIP20Service.TransferRequestAndWaitForReceiptAsync(msReceiverAddress, mnTransferAmt).Result;
		}
	}
}
