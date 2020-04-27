using System;
using System.Globalization;
using System.Numerics;

using Nethereum.StandardTokenEIP20.ContractDefinition;

using Wonka.BizRulesEngine.RuleTree;

namespace Wonka.Eth.Extensions.OpSource
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

			BigInteger nApprovedAmt = BigInteger.Parse(psApprovedAmt, NumberStyles.AllowHexSpecifier);

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

		public string InvokeERC20GetBalance(string psOwner = "", string psDummyVal1 = "", string psDummyVal2 = "", string psDummyVal3 = "")
		{
			var tokenService = GetERC20TokenService();

			var balance = tokenService.BalanceOfQueryAsync(new BalanceOfFunction() { Owner = psOwner }).Result;

			return balance.ToString();
		}

		public string InvokeERC20GetTotalSupply(string psDummyVal1 = "", string psDummyVal2 = "", string psDummyVal3 = "", string psDummyVal4 = "")
		{
			var tokenService = GetERC20TokenService();

			var totalSupply = tokenService.TotalSupplyQueryAsync().Result;

			return totalSupply.ToString();

		}

		public string InvokeERC20Transfer(string psToAccount, string psTransferAmt, string psDummyVal1 = "", string psDummyVal2 = "")
		{
			var tokenService = GetERC20TokenService();

			BigInteger nAmtToSend = BigInteger.Parse(psTransferAmt, NumberStyles.AllowHexSpecifier);

			var trxReceipt = tokenService.TransferRequestAndWaitForReceiptAsync(new TransferFunction() { To = psToAccount, Value = nAmtToSend }).Result;

			return trxReceipt.TransactionHash;

		}
	}
}