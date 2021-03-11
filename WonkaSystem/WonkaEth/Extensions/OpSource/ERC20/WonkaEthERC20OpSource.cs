using System;
using System.Globalization;
using System.Numerics;

using Nethereum.StandardTokenEIP20.ContractDefinition;

using Wonka.BizRulesEngine.RuleTree;

namespace Wonka.Eth.Extensions.OpSource.ERC20
{
	public class WonkaEthERC20OpSource : WonkaBizSource
	{
		#region PROPERTIES

		public readonly Nethereum.Web3.Web3 SenderWeb3;

		#endregion

		public WonkaEthERC20OpSource(string psSourceId, string psSenderAddr, string psPwd, string psContractAddr, string psCustomOpMethodName, string psWeb3Url = "") :
			base(psSourceId, psSenderAddr, psPwd, psContractAddr, null, null, psCustomOpMethodName)
		{
			var account = new Nethereum.Web3.Accounts.Account(psPwd);

			if (!String.IsNullOrEmpty(psWeb3Url))
				SenderWeb3 = new Nethereum.Web3.Web3(account, psWeb3Url);
			else
				SenderWeb3 = new Nethereum.Web3.Web3(account);
		}

		public string InvokeERC20Approve(string psSpender, string psApprovedAmt, string psDummyVal1 = "", string psDummyVal2 = "")
		{
			var tokenService = GetERC20TokenService();

			psApprovedAmt = psApprovedAmt.StartsWith("0x") ? psApprovedAmt.Replace("0x", "0") : psApprovedAmt;

			var nApprovedAmt =
            	System.Numerics.BigInteger.Parse(psApprovedAmt, System.Globalization.NumberStyles.HexNumber);

			var trxReceipt = tokenService.ApproveRequestAndWaitForReceiptAsync(new ApproveFunction() { Spender = psSpender, Value = nApprovedAmt }).Result;

			return trxReceipt.TransactionHash;
		}

		public Nethereum.StandardTokenEIP20.StandardTokenService GetERC20TokenService()
		{
			return new Nethereum.StandardTokenEIP20.StandardTokenService(SenderWeb3, this.ContractAddress);
		}

		public string InvokeERC20GetAllowance(string psOwner, string psSpender, string psDummyVal1 = "", string psDummyVal2 = "")
		{
			var tokenService = GetERC20TokenService();

			var nAmtRemaining = tokenService.AllowanceQueryAsync(new AllowanceFunction() { Owner = psOwner, Spender = psSpender }).Result;

			return nAmtRemaining.ToString();
		}

		public string InvokeERC20GetBalance(string psOwner, string psDummyVal1 = "", string psDummyVal2 = "", string psDummyVal3 = "")
		{
			var tokenService = GetERC20TokenService();

			var acctOwner = psOwner;

			var function = new BalanceOfFunction() { Owner = acctOwner };

			var balance = tokenService.BalanceOfQueryAsync(function).Result;

			return balance.ToString();
		}

		public string InvokeERC20GetGasEstimate(string psToAccount, string psTransferAmt, string psDummyVal1 = "", string psDummyVal2 = "")
		{
			var tokenService = GetERC20TokenService();

			psTransferAmt = psTransferAmt.StartsWith("0x") ? psTransferAmt.Replace("0x", "0") : psTransferAmt;

			var nAmtToSend =
				System.Numerics.BigInteger.Parse(psTransferAmt, System.Globalization.NumberStyles.HexNumber);

			var transferHandler = this.SenderWeb3.Eth.GetContractTransactionHandler<TransferFunction>();
			var transfer = new TransferFunction()
			{
				To = psToAccount,
				Value = nAmtToSend
			};

			var estimate = transferHandler.EstimateGasAsync(this.ContractAddress, transfer).Result;

			return estimate.Value.ToString();
		}

		public string InvokeERC20GetTotalSupply(string psDummyVal1 = "", string psDummyVal2 = "", string psDummyVal3 = "", string psDummyVal4 = "")
		{
			var tokenService = GetERC20TokenService();

			var totalSupply = tokenService.TotalSupplyQueryAsync().Result;

			return totalSupply.ToString();

		}

		public string InvokeERC20Transfer(string psToAccount, string psTransferAmt, string psGasToSend = "", string psDummyVal1 = "")
		{
			var tokenService = GetERC20TokenService();

			psTransferAmt = psTransferAmt.StartsWith("0x") ? psTransferAmt.Replace("0x", "0") : psTransferAmt;

			var nAmtToSend =
	            System.Numerics.BigInteger.Parse(psTransferAmt, System.Globalization.NumberStyles.HexNumber);

			var ERC20TransferFunction =
				new TransferFunction() { To = psToAccount, Value = nAmtToSend };

			if (!String.IsNullOrEmpty(psGasToSend))
			{
				var nGasToSend =
					System.Numerics.BigInteger.Parse(psGasToSend, System.Globalization.NumberStyles.HexNumber);

				ERC20TransferFunction.Gas = nGasToSend;
			}

			var trxReceipt = tokenService.TransferRequestAndWaitForReceiptAsync(ERC20TransferFunction).Result;

			return trxReceipt.TransactionHash;

		}
	}
}