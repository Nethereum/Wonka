using System;

namespace Wonka.Eth.Extensions.OpSource.ERC1155
{
	public class WonkaEthERC1150BalanceOfOpSource : WonkaEthERC1155OpSource
	{
		public WonkaEthERC1150BalanceOfOpSource(string psSourceId, string psSenderAddr, string psPwd, string psContractAddr, string psCustomOpMethodName, string psWeb3Url = "") :
			base(psSourceId, psSenderAddr, psPwd, psContractAddr, psCustomOpMethodName, psWeb3Url)
		{
			this.CustomOpDelegate = this.InvokeERC1155BalanceOf;

			// NOTE: Should we be setting this property at all?
			// public delegate CustomOperatorRule BuildCustomOpRuleDelegate(WonkaBizSource poSource, int pnRuleID);
		}
	}
}
