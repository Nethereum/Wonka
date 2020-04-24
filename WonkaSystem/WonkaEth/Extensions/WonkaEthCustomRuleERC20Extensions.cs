using System;
using Nethereum.Web3;

using Wonka.BizRulesEngine;
using Wonka.BizRulesEngine.RuleTree;
using Wonka.MetaData;

namespace Wonka.Eth.Extensions
{
	public static class WonkaEthCustomRuleERC20Extensions
	{
		private static int mnRuleCounter = 100000;

		public static void AddNethereumERC20GetBalanceRule(this WonkaBizRuleSet poRuleSet,
			                                                WonkaRefEnvironment poRefEnv, 
															               Web3 poWeb3,
															       WonkaRefAttr poTargetAttr,
															             string psAddRuleDesc,
															             string psTargetOwner,
																	     string psERC20ContractAddress)
		{
			WonkaBizRule NewRule = null;

			WonkaBizSource DummySource =
				new WonkaBizSource("ERC20", psERC20ContractAddress, "", "", "", "", "", null);

			WonkaEthCustomOpRule CustomOpRule =
				new WonkaEthCustomOpRule(mnRuleCounter++,
									     TARGET_RECORD.TRID_NEW_RECORD,
										 poTargetAttr.AttrId,
									     "GET_ERC20_BALANCE",
									     null,
									     DummySource);

			CustomOpRule.OwnerWeb3              = poWeb3;
			CustomOpRule.PrimaryContractAddress = psERC20ContractAddress;
			CustomOpRule.CustomOpDelegate       = CustomOpRule.InvokeERC20GetBalance;

			CustomOpRule.AddDomainValue(psTargetOwner, true, TARGET_RECORD.TRID_NONE);

			if (!String.IsNullOrEmpty(psAddRuleDesc))
				NewRule.DescRuleId = psAddRuleDesc;

			poRuleSet.AddRule(NewRule);
		}

		public static void AddNethereumERC20TransferRule(this WonkaBizRuleSet poRuleSet,
														  WonkaRefEnvironment poRefEnv,
														   			     Web3 poWeb3,
																 WonkaRefAttr poTargetAttr,
														       		   string psAddRuleDesc,
																	   string psTargetOwner,
																	   string psERC20ContractAddress,
																	   string psTransferReceiverAddress,
																	   string psAmountToSend)
		{
			WonkaBizRule NewRule = null;

			WonkaBizSource DummySource =
				new WonkaBizSource("ERC20", psERC20ContractAddress, "", "", "", "", "", null);

			WonkaEthCustomOpRule CustomOpRule =
				new WonkaEthCustomOpRule(mnRuleCounter++,
										 TARGET_RECORD.TRID_NEW_RECORD,
										 poTargetAttr.AttrId,
										 "ERC20_TRANSFER",
										 null,
										 DummySource);

			CustomOpRule.OwnerWeb3              = poWeb3;
			CustomOpRule.PrimaryContractAddress = psERC20ContractAddress;
			CustomOpRule.CustomOpDelegate       = CustomOpRule.InvokeERC20Transfer;

			CustomOpRule.AddDomainValue(psTransferReceiverAddress, true, TARGET_RECORD.TRID_NONE);
			CustomOpRule.AddDomainValue(psAmountToSend,            true, TARGET_RECORD.TRID_NONE);

			if (!String.IsNullOrEmpty(psAddRuleDesc))
				NewRule.DescRuleId = psAddRuleDesc;

			poRuleSet.AddRule(NewRule);
		}
	}
}

