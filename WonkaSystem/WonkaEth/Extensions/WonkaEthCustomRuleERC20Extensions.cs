using System;
using System.Collections.Generic;

using Nethereum.Web3;

using Wonka.BizRulesEngine;
using Wonka.BizRulesEngine.RuleTree;
using Wonka.MetaData;

using Wonka.Eth.Extensions.OpSource;

namespace Wonka.Eth.Extensions
{
	public static class WonkaEthCustomRuleERC20Extensions
	{
		private static int mnRuleCounter = 100000;

		public static string CONST_ERC_DUMMY_SOURCE   = "ERC20_SOURCE";
		public static string CONST_ERC_GET_BALANCE_OP = "ERC20_GET_BALANCE";
		public static string CONST_ERC_TRANSFER_OP    = "ERC20_TRANSFER";

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
										 CONST_ERC_GET_BALANCE_OP,
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
										 CONST_ERC_TRANSFER_OP,
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

		public static Dictionary<string, WonkaBizSource> CreateERC20OpMap(string psEthSender, string psEthPwd, string psEthContractAddress, string psWeb3Url = "")
		{
			var OpMapERC20 = new Dictionary<string, WonkaBizSource>();

			OpMapERC20[CONST_ERC_GET_BALANCE_OP] = CreateERC20GetBalanceOperator(psEthSender, psEthPwd, psEthContractAddress, psWeb3Url);
			OpMapERC20[CONST_ERC_TRANSFER_OP]    = CreateERC20TransferOperator(psEthSender, psEthPwd, psEthContractAddress, psWeb3Url);

			return OpMapERC20;
		}

		public static WonkaBizSource CreateERC20GetBalanceOperator(string psEthSender, string psEthPwd, string psEthContractAddress, string psWeb3Url = "")
		{
			WonkaEthERC20GetBalanceOpSource ERC20GetBalanceSource =
				new WonkaEthERC20GetBalanceOpSource(CONST_ERC_DUMMY_SOURCE, psEthSender, psEthPwd, psEthContractAddress, CONST_ERC_GET_BALANCE_OP, psWeb3Url);

			return ERC20GetBalanceSource;
		}

		public static WonkaBizSource CreateERC20TransferOperator(string psEthSender, string psEthPwd, string psEthContractAddress, string psWeb3Url = "")
		{
			WonkaEthERC20TransferOpSource ERC20TransferSource =
				new WonkaEthERC20TransferOpSource(CONST_ERC_DUMMY_SOURCE, psEthSender, psEthPwd, psEthContractAddress, CONST_ERC_TRANSFER_OP, psWeb3Url);

			return ERC20TransferSource;
		}
	}
}

