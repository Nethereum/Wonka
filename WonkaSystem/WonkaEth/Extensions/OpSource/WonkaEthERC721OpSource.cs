using System;
using System.Globalization;
using System.Numerics;
using System.Text;

using Nethereum.StandardNonFungibleTokenERC721.ContractDefinition;

using Wonka.BizRulesEngine.RuleTree;

namespace Wonka.Eth.Extensions.OpSource
{
	public class WonkaEthERC721OpSource : WonkaBizSource
	{
		#region PROPERTIES

		public readonly Nethereum.Web3.Web3 SenderWeb3;

		#endregion

		public WonkaEthERC721OpSource(string psSourceId, string psSenderAddr, string psPwd, string psContractAddr, string psCustomOpMethodName, string psWeb3Url = "") :
			base(psSourceId, psSenderAddr, psPwd, psContractAddr, null, null, psCustomOpMethodName)
		{
			var account = new Nethereum.Web3.Accounts.Account(psPwd);

			if (!String.IsNullOrEmpty(psWeb3Url))
				SenderWeb3 = new Nethereum.Web3.Web3(account, psWeb3Url);
			else
				SenderWeb3 = new Nethereum.Web3.Web3(account);
		}

		public string InvokeERC721Approve(string psApprovedAcct, string psTokenId, string psDummyVal1 = "", string psDummyVal2 = "")
		{
			var tokenService = GetERC721TokenService();

			BigInteger nTokenId = BigInteger.Parse(psTokenId, NumberStyles.AllowHexSpecifier);

			var trxReceipt = tokenService.ApproveRequestAndWaitForReceiptAsync(new ApproveFunction() { To = psApprovedAcct, TokenId = nTokenId }).Result;

			return trxReceipt.TransactionHash;
		}

		public Nethereum.StandardNonFungibleTokenERC721.ERC721Service GetERC721TokenService()
		{
			return new Nethereum.StandardNonFungibleTokenERC721.ERC721Service(SenderWeb3, this.ContractAddress);
		}

		public string InvokeERC20GetBalance(string psOwner = "", string psDummyVal1 = "", string psDummyVal2 = "", string psDummyVal3 = "")
		{
			var tokenService = GetERC721TokenService();

			var balance = tokenService.BalanceOfQueryAsync(new BalanceOfFunction() { Owner = psOwner }).Result;

			return balance.ToString();
		}

		public string InvokeERC20OwnerOf(string psTokenId = "", string psDummyVal1 = "", string psDummyVal2 = "", string psDummyVal3 = "")
		{
			var tokenService = GetERC721TokenService();

			BigInteger nTokenId = BigInteger.Parse(psTokenId, NumberStyles.AllowHexSpecifier);

			var owner = tokenService.OwnerOfQueryAsync(new OwnerOfFunction() { TokenId = nTokenId }).Result;

			return owner.ToString();
		}

		public string InvokeERC721SafeTransferFrom(string psFromAccount, string psToAccount, string psTokenId, string psTokenData)
		{
			var tokenService = GetERC721TokenService();

			byte[] TokenData = Encoding.ASCII.GetBytes(psTokenData);

			BigInteger nTokenId = BigInteger.Parse(psTokenId, NumberStyles.AllowHexSpecifier);

			var trxReceipt =
				tokenService.SafeTransferFromRequestAndWaitForReceiptAsync(new SafeTransferFromWithDataFunction() { From = psFromAccount, To = psToAccount, TokenId = nTokenId, Data = TokenData }).Result;

			return trxReceipt.TransactionHash;

		}

		public string InvokeERC721TransferFrom(string psFromAccount, string psToAccount, string psTokenId, string psDummyVal1 = "")
		{
			var tokenService = GetERC721TokenService();

			BigInteger nTokenId = BigInteger.Parse(psTokenId, NumberStyles.AllowHexSpecifier);

			var trxReceipt = tokenService.TransferFromRequestAndWaitForReceiptAsync(new TransferFromFunction() { From = psFromAccount, To = psToAccount, TokenId = nTokenId }).Result;

			return trxReceipt.TransactionHash;

		}
	}
}