using System;
using System.Globalization;
using System.Numerics;

using Nethereum.StandardTokenEIP20.ContractDefinition;

using Wonka.BizRulesEngine.RuleTree;

namespace Wonka.Eth.Extensions.OpSource.ERC1155
{
	public class WonkaEthERC1155OpSource : WonkaBizSource
	{
		#region PROPERTIES

		public readonly Nethereum.Web3.Web3 SenderWeb3;

		#endregion

		public WonkaEthERC1155OpSource(string psSourceId, string psSenderAddr, string psPwd, string psContractAddr, string psCustomOpMethodName, string psWeb3Url = "") :
			base(psSourceId, psSenderAddr, psPwd, psContractAddr, null, null, psCustomOpMethodName)
		{
			var account = new Nethereum.Web3.Accounts.Account(psPwd);

			if (!String.IsNullOrEmpty(psWeb3Url))
				SenderWeb3 = new Nethereum.Web3.Web3(account, psWeb3Url);
			else
				SenderWeb3 = new Nethereum.Web3.Web3(account);
		}
	}
}