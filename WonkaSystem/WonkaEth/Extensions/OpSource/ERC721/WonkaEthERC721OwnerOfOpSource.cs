using System;

namespace Wonka.Eth.Extensions.OpSource.ERC721
{
	public class WonkaEthERC721OwnerOfOpSource : WonkaEthERC721OpSource
	{
		public WonkaEthERC721OwnerOfOpSource(string psSourceId, string psSenderAddr, string psPwd, string psContractAddr, string psCustomOpMethodName, string psWeb3Url = "") :
			base(psSourceId, psSenderAddr, psPwd, psContractAddr, psCustomOpMethodName, psWeb3Url)
		{
			this.CustomOpDelegate = this.InvokeERC721OwnerOf;

			// NOTE: Should we be setting this property at all?
			// public delegate CustomOperatorRule BuildCustomOpRuleDelegate(WonkaBizSource poSource, int pnRuleID);
		}
	}
}