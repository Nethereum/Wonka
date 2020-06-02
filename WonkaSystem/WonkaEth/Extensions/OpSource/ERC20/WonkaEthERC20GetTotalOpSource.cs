using System;

namespace Wonka.Eth.Extensions.OpSource.ERC20
{
	public class WonkaEthERC20GetTotalOpSource : WonkaEthERC20OpSource
	{
		public WonkaEthERC20GetTotalOpSource(string psSourceId, string psSenderAddr, string psPwd, string psContractAddr, string psCustomOpMethodName, string psWeb3Url = "") :
			base(psSourceId, psSenderAddr, psPwd, psContractAddr, psCustomOpMethodName, psWeb3Url)
		{
			this.CustomOpDelegate = this.InvokeERC20GetTotalSupply;

			// NOTE: Should we be setting this property at all?
			// public delegate CustomOperatorRule BuildCustomOpRuleDelegate(WonkaBizSource poSource, int pnRuleID);
		}
	}
}

