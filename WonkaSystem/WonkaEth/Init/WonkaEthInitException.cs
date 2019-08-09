using System;

namespace WonkaEth.Init
{
	public class WonkaEthInitException : Exception
	{
		public readonly WonkaEthEngineInitialization InitData = null;

		public WonkaEthInitException(string psErroMessage, WonkaEthEngineInitialization poInitData) : base(psErroMessage)
		{
			InitData = poInitData;
		}
	}
}

