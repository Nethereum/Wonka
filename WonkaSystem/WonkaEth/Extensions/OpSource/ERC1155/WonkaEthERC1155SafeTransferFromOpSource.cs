using System;

namespace Wonka.Eth.Extensions.OpSource.ERC1155
{
	public class WonkaEthERC1155SafeTransferFromOpSource : WonkaEthERC1155OpSource
	{
		public WonkaEthERC1155SafeTransferFromOpSource(string psSourceId, string psSenderAddr, string psPwd, string psContractAddr, string psCustomOpMethodName, string psWeb3Url = "") :
			base(psSourceId, psSenderAddr, psPwd, psContractAddr, psCustomOpMethodName, psWeb3Url)
		{
			this.CustomOpDelegate = this.InvokeERC1155SafeTransferFrom;

			// NOTE: Should we be setting this property at all?
			// public delegate CustomOperatorRule BuildCustomOpRuleDelegate(WonkaBizSource poSource, int pnRuleID);
		}
	}
}
