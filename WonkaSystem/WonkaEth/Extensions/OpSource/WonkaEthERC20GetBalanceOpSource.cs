using System;

namespace Wonka.Eth.Extensions.OpSource
{
	public class WonkaEthERC20GetBalanceOpSource : WonkaEthERC20OpSource
	{
		public WonkaEthERC20GetBalanceOpSource(string psSourceId, string psSenderAddr, string psPwd, string psContractAddr, string psCustomOpMethodName, string psWeb3Url = "") :
			base(psSourceId, psSenderAddr, psPwd, psContractAddr, psCustomOpMethodName, psWeb3Url)
		{
			this.CustomOpDelegate = this.InvokeERC20Transfer;

			// NOTE: Should we be setting this property at all?
			// public delegate CustomOperatorRule BuildCustomOpRuleDelegate(WonkaBizSource poSource, int pnRuleID);
		}
	}
}
