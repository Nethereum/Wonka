using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Nethereum.Web3;

using Wonka.BizRulesEngine;
using Wonka.BizRulesEngine.RuleTree;
using Wonka.MetaData;

using Wonka.Eth.Extensions.OpSource.ERC20;
using Wonka.Eth.Extensions.OpSource.ERC721;
using Wonka.Eth.Extensions.OpSource.ERC1155;

namespace Wonka.Eth.Extensions
{
	public static class WonkaEthCustomRuleTokenExtensions
	{
		private static int mnRuleCounter = 100000;

		public static string CONST_ERC20_DUMMY_SOURCE     = "ERC20_SOURCE";
		public static string CONST_ERC20_APPROVE_OP       = "ERC20_APPROVE";
		public static string CONST_ERC20_GET_ALLOWANCE_OP = "ERC20_GET_ALLOWANCE";
		public static string CONST_ERC20_GET_BALANCE_OP   = "ERC20_GET_BALANCE";
		public static string CONST_ERC20_GET_TOTAL_OP     = "ERC20_GET_TOTAL";
		public static string CONST_ERC20_TRANSFER_OP      = "ERC20_TRANSFER";

		public static string CONST_ERC721_DUMMY_SOURCE     = "ERC721_SOURCE";
		public static string CONST_ERC721_APPROVE_OP       = "ERC721_APPROVE";
		public static string CONST_ERC721_GET_BALANCE_OP   = "ERC721_GET_BALANCE";
		public static string CONST_ERC721_GET_OWNER_OF_OP  = "ERC721_OWNER_OF";
		public static string CONST_ERC721_SAFE_TRANSFER_OP = "ERC721_SAFE_TRANSFER";
		public static string CONST_ERC721_TRANSFER_OP      = "ERC721_TRANSFER";

		public static string CONST_ERC1155_DUMMY_SOURCE     = "ERC1155_SOURCE";
		public static string CONST_ERC1155_GET_BALANCE_OP   = "ERC1155_GET_BALANCE";
		public static string CONST_ERC1155_SAFE_TRANSFER_OP = "ERC1155_SAFE_TRANSFER";

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
										 CONST_ERC20_GET_BALANCE_OP,
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
										 CONST_ERC20_TRANSFER_OP,
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

		public static WonkaBizSource CreateERC20ApproveOperator(string psEthSender, string psEthPwd, string psEthContractAddress, string psWeb3Url = "")
		{
			var ERC20ApproveSource =
				new WonkaEthERC20ApproveOpSource(CONST_ERC20_DUMMY_SOURCE, psEthSender, psEthPwd, psEthContractAddress, CONST_ERC20_APPROVE_OP, psWeb3Url);

			return ERC20ApproveSource;
		}

		public static WonkaBizSource CreateERC20GetAllowanceOperator(string psEthSender, string psEthPwd, string psEthContractAddress, string psWeb3Url = "")
		{
			var ERC20GetAllowanceSource =
				new WonkaEthERC20GetAllowanceOpSource(CONST_ERC20_DUMMY_SOURCE, psEthSender, psEthPwd, psEthContractAddress, CONST_ERC20_APPROVE_OP, psWeb3Url);

			return ERC20GetAllowanceSource;
		}

		public static WonkaBizSource CreateERC20GetBalanceOperator(string psEthSender, string psEthPwd, string psEthContractAddress, string psWeb3Url = "")
		{
			var ERC20GetBalanceSource =
				new WonkaEthERC20GetBalanceOpSource(CONST_ERC20_DUMMY_SOURCE, psEthSender, psEthPwd, psEthContractAddress, CONST_ERC20_GET_BALANCE_OP, psWeb3Url);

			return ERC20GetBalanceSource;
		}

		public static WonkaBizSource CreateERC20GetTotalOperator(string psEthSender, string psEthPwd, string psEthContractAddress, string psWeb3Url = "")
		{
			var ERC20GetTotalSource =
				new WonkaEthERC20GetTotalOpSource(CONST_ERC20_DUMMY_SOURCE, psEthSender, psEthPwd, psEthContractAddress, CONST_ERC20_GET_TOTAL_OP, psWeb3Url);

			return ERC20GetTotalSource;
		}

		public static WonkaBizSource CreateERC20TransferOperator(string psEthSender, string psEthPwd, string psEthContractAddress, string psWeb3Url = "")
		{
			var ERC20TransferSource =
				new WonkaEthERC20TransferOpSource(CONST_ERC20_DUMMY_SOURCE, psEthSender, psEthPwd, psEthContractAddress, CONST_ERC20_TRANSFER_OP, psWeb3Url);

			return ERC20TransferSource;
		}

		public static WonkaBizSource CreateERC721ApproveOperator(string psEthSender, string psEthPwd, string psEthContractAddress, string psWeb3Url = "")
		{
			var ERC721ApproveSource =
				new WonkaEthERC721ApproveOpSource(CONST_ERC721_DUMMY_SOURCE, psEthSender, psEthPwd, psEthContractAddress, CONST_ERC721_APPROVE_OP, psWeb3Url);

			return ERC721ApproveSource;
		}

		public static WonkaBizSource CreateERC721GetBalanceOperator(string psEthSender, string psEthPwd, string psEthContractAddress, string psWeb3Url = "")
		{
			var ERC721GetBalanceSource =
				new WonkaEthERC721GetBalanceOpSource(CONST_ERC721_DUMMY_SOURCE, psEthSender, psEthPwd, psEthContractAddress, CONST_ERC721_GET_BALANCE_OP, psWeb3Url);

			return ERC721GetBalanceSource;
		}

		public static WonkaBizSource CreateERC721OwnerOfOperator(string psEthSender, string psEthPwd, string psEthContractAddress, string psWeb3Url = "")
		{
			var ERC20OwnerOfSource =
				new WonkaEthERC721OwnerOfOpSource(CONST_ERC721_DUMMY_SOURCE, psEthSender, psEthPwd, psEthContractAddress, CONST_ERC721_GET_OWNER_OF_OP, psWeb3Url);

			return ERC20OwnerOfSource;
		}

		public static WonkaBizSource CreateERC721SafeTransferOperator(string psEthSender, string psEthPwd, string psEthContractAddress, string psWeb3Url = "")
		{
			var ERC20SafeTransferSource =
				new WonkaEthERC721SafeTransferOpSource(CONST_ERC721_DUMMY_SOURCE, psEthSender, psEthPwd, psEthContractAddress, CONST_ERC721_SAFE_TRANSFER_OP, psWeb3Url);

			return ERC20SafeTransferSource;
		}

		public static WonkaBizSource CreateERC721TransferOperator(string psEthSender, string psEthPwd, string psEthContractAddress, string psWeb3Url = "")
		{
			var ERC20TransferSource =
				new WonkaEthERC721TransferOpSource(CONST_ERC721_DUMMY_SOURCE, psEthSender, psEthPwd, psEthContractAddress, CONST_ERC721_TRANSFER_OP, psWeb3Url);

			return ERC20TransferSource;
		}

		public static WonkaBizSource CreateERC1155GetBalanceOperator(string psEthSender, string psEthPwd, string psEthContractAddress, string psWeb3Url = "")
		{
			var ERC1155GetBalanceSource =
				new WonkaEthERC1155BalanceOfOpSource(CONST_ERC1155_DUMMY_SOURCE, psEthSender, psEthPwd, psEthContractAddress, CONST_ERC1155_GET_BALANCE_OP, psWeb3Url);

			return ERC1155GetBalanceSource;
		}

		public static WonkaBizSource CreateERC1155SafeTransferOperator(string psEthSender, string psEthPwd, string psEthContractAddress, string psWeb3Url = "")
		{
			var ERC1155SafeTransferSource =
				new WonkaEthERC1155SafeTransferFromOpSource(CONST_ERC1155_DUMMY_SOURCE, psEthSender, psEthPwd, psEthContractAddress, CONST_ERC1155_SAFE_TRANSFER_OP, psWeb3Url);

			return ERC1155SafeTransferSource;
		}

		public static Dictionary<string, WonkaBizSource> InitializeTokenOperationsMap(string psEthSender, string psEthPwd, string psERC20Addr, string psERC721Addr, string psWeb3Url = "")
		{
			var TokenOpsMap = new Dictionary<string, WonkaBizSource>();

			var ERC20_Ops_Map   = InitializeERC20OpMap(psEthSender, psEthPwd, psERC20Addr, psWeb3Url);
			var ERC721_Ops_Map  = InitializeERC721OpMap(psEthSender, psEthPwd, psERC721Addr, psWeb3Url);
			var ERC1155_Ops_Map = InitializeERC1155OpMap(psEthSender, psEthPwd, psERC721Addr, psWeb3Url);

			ERC20_Ops_Map.ToList().ForEach(x => TokenOpsMap.Add(x.Key, x.Value));
			ERC721_Ops_Map.ToList().ForEach(x => TokenOpsMap.Add(x.Key, x.Value));
			ERC1155_Ops_Map.ToList().ForEach(x => TokenOpsMap.Add(x.Key, x.Value));

			return TokenOpsMap;
		}

		public static Dictionary<string, WonkaBizSource> InitializeERC20OpMap(string psEthSender, string psEthPwd, string psEthContractAddress, string psWeb3Url = "")
		{
			var OpMapERC20 = new Dictionary<string, WonkaBizSource>();

			OpMapERC20[CONST_ERC20_TRANSFER_OP]      = CreateERC20TransferOperator(psEthSender, psEthPwd, psEthContractAddress, psWeb3Url);
			OpMapERC20[CONST_ERC20_GET_ALLOWANCE_OP] = CreateERC20GetAllowanceOperator(psEthSender, psEthPwd, psEthContractAddress, psWeb3Url);
			OpMapERC20[CONST_ERC20_GET_BALANCE_OP]   = CreateERC20GetBalanceOperator(psEthSender, psEthPwd, psEthContractAddress, psWeb3Url);
			OpMapERC20[CONST_ERC20_APPROVE_OP]       = CreateERC20ApproveOperator(psEthSender, psEthPwd, psEthContractAddress, psWeb3Url);
			OpMapERC20[CONST_ERC20_GET_TOTAL_OP]     = CreateERC20GetTotalOperator(psEthSender, psEthPwd, psEthContractAddress, psWeb3Url);

			return OpMapERC20;
		}

		public static Dictionary<string, WonkaBizSource> InitializeERC721OpMap(string psEthSender, string psEthPwd, string psEthContractAddress, string psWeb3Url = "")
		{
			var OpMapERC721 = new Dictionary<string, WonkaBizSource>();

			OpMapERC721[CONST_ERC721_APPROVE_OP]       = CreateERC721ApproveOperator(psEthSender, psEthPwd, psEthContractAddress, psWeb3Url);
			OpMapERC721[CONST_ERC721_GET_BALANCE_OP]   = CreateERC721GetBalanceOperator(psEthSender, psEthPwd, psEthContractAddress, psWeb3Url);
			OpMapERC721[CONST_ERC721_GET_OWNER_OF_OP]  = CreateERC721OwnerOfOperator(psEthSender, psEthPwd, psEthContractAddress, psWeb3Url);
			OpMapERC721[CONST_ERC721_SAFE_TRANSFER_OP] = CreateERC721SafeTransferOperator(psEthSender, psEthPwd, psEthContractAddress, psWeb3Url);
			OpMapERC721[CONST_ERC721_TRANSFER_OP]      = CreateERC721TransferOperator(psEthSender, psEthPwd, psEthContractAddress, psWeb3Url);

			return OpMapERC721;
		}

		public static Dictionary<string, WonkaBizSource> InitializeERC1155OpMap(string psEthSender, string psEthPwd, string psEthContractAddress, string psWeb3Url = "")
		{
			var OpMapERC1155 = new Dictionary<string, WonkaBizSource>();

		    OpMapERC1155[CONST_ERC1155_GET_BALANCE_OP]   = CreateERC1155GetBalanceOperator(psEthSender, psEthPwd, psEthContractAddress, psWeb3Url);
			OpMapERC1155[CONST_ERC1155_SAFE_TRANSFER_OP] = CreateERC1155SafeTransferOperator(psEthSender, psEthPwd, psEthContractAddress, psWeb3Url);

			return OpMapERC1155;
		}

	}
}

