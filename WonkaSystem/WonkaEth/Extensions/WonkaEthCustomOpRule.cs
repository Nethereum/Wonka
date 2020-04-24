using System;
using System.Globalization;
using System.Numerics;

using Nethereum.StandardTokenEIP20.ContractDefinition;
using Nethereum.Web3;

using Wonka.BizRulesEngine;
using Wonka.BizRulesEngine.Readers;
using Wonka.BizRulesEngine.RuleTree;
using Wonka.BizRulesEngine.RuleTree.RuleTypes;

namespace Wonka.Eth.Extensions
{
	public class WonkaEthCustomOpRule : CustomOperatorRule
	{
		#region PROPERTIES

		public Web3 OwnerWeb3 { get; set; }

		public string PrimaryContractAddress { get; set; }

		#endregion

		public WonkaEthCustomOpRule() : base()
        {}

        public WonkaEthCustomOpRule(int pnRuleID) : base(pnRuleID)
        {}

        public WonkaEthCustomOpRule(int pnRuleID,
                                  TARGET_RECORD peTargetRecord,
                                  int pnTargetAttrId,
                                  string psCustomOpName,
                                  WonkaBizRulesXmlReader.ExecuteCustomOperator poCustomOpDelegate,
                                  WonkaBizSource poCustomOpSource)
            : base(pnRuleID, peTargetRecord, pnTargetAttrId, psCustomOpName, poCustomOpDelegate, poCustomOpSource)
        {}

		public Nethereum.StandardTokenEIP20.StandardTokenService GetERC20TokenService()
		{
			return new Nethereum.StandardTokenEIP20.StandardTokenService(OwnerWeb3, PrimaryContractAddress);
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

			var trxReceipt = tokenService.TransferRequestAndWaitForReceiptAsync(new TransferFunction() { To = psToAccount, AmountToSend = nAmtToSend }).Result;

			return trxReceipt.TransactionHash;

		}
	}
}
