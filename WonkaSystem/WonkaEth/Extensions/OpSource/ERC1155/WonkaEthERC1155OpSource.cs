using System;
using System.Globalization;
using System.Numerics;

using Wonka.BizRulesEngine.RuleTree;

namespace Wonka.Eth.Extensions.OpSource.ERC1155
{
	public class WonkaEthERC1155OpSource : WonkaBizSource
	{
		public const string CONST_ERC_1155_ABI =
@"
[
    {
      ""anonymous"": false,
      ""inputs"": [
        {
          ""indexed"": true,
          ""internalType"": ""address"",
          ""name"": ""_owner"",
          ""type"": ""address""
        },
        {
          ""indexed"": true,
          ""internalType"": ""address"",
          ""name"": ""_operator"",
          ""type"": ""address""
        },
        {
          ""indexed"": false,
          ""internalType"": ""bool"",
          ""name"": ""_approved"",
          ""type"": ""bool""
        }
      ],
      ""name"": ""ApprovalForAll"",
      ""type"": ""event""
    },
    {
      ""anonymous"": false,
      ""inputs"": [
        {
          ""indexed"": true,
          ""internalType"": ""address"",
          ""name"": ""_operator"",
          ""type"": ""address""
        },
        {
          ""indexed"": true,
          ""internalType"": ""address"",
          ""name"": ""_from"",
          ""type"": ""address""
        },
        {
          ""indexed"": true,
          ""internalType"": ""address"",
          ""name"": ""_to"",
          ""type"": ""address""
        },
        {
          ""indexed"": false,
          ""internalType"": ""uint256[]"",
          ""name"": ""_ids"",
          ""type"": ""uint256[]""
        },
        {
          ""indexed"": false,
          ""internalType"": ""uint256[]"",
          ""name"": ""_values"",
          ""type"": ""uint256[]""
        }
      ],
      ""name"": ""TransferBatch"",
      ""type"": ""event""
    },
    {
      ""anonymous"": false,
      ""inputs"": [
        {
          ""indexed"": true,
          ""internalType"": ""address"",
          ""name"": ""_operator"",
          ""type"": ""address""
        },
        {
          ""indexed"": true,
          ""internalType"": ""address"",
          ""name"": ""_from"",
          ""type"": ""address""
        },
        {
          ""indexed"": true,
          ""internalType"": ""address"",
          ""name"": ""_to"",
          ""type"": ""address""
        },
        {
          ""indexed"": false,
          ""internalType"": ""uint256"",
          ""name"": ""_id"",
          ""type"": ""uint256""
        },
        {
          ""indexed"": false,
          ""internalType"": ""uint256"",
          ""name"": ""_value"",
          ""type"": ""uint256""
        }
      ],
      ""name"": ""TransferSingle"",
      ""type"": ""event""
    },
    {
      ""anonymous"": false,
      ""inputs"": [
        {
          ""indexed"": false,
          ""internalType"": ""string"",
          ""name"": ""_value"",
          ""type"": ""string""
        },
        {
          ""indexed"": true,
          ""internalType"": ""uint256"",
          ""name"": ""_id"",
          ""type"": ""uint256""
        }
      ],
      ""name"": ""URI"",
      ""type"": ""event""
    },
    {
      ""inputs"": [
        {
          ""internalType"": ""address"",
          ""name"": ""_from"",
          ""type"": ""address""
        },
        {
          ""internalType"": ""address"",
          ""name"": ""_to"",
          ""type"": ""address""
        },
        {
          ""internalType"": ""uint256"",
          ""name"": ""_id"",
          ""type"": ""uint256""
        },
        {
          ""internalType"": ""uint256"",
          ""name"": ""_value"",
          ""type"": ""uint256""
        },
        {
          ""internalType"": ""bytes"",
          ""name"": ""_data"",
          ""type"": ""bytes""
        }
      ],
      ""name"": ""safeTransferFrom"",
      ""outputs"": [],
      ""stateMutability"": ""nonpayable"",
      ""type"": ""function""
    },
    {
      ""inputs"": [
        {
          ""internalType"": ""address"",
          ""name"": ""_from"",
          ""type"": ""address""
        },
        {
          ""internalType"": ""address"",
          ""name"": ""_to"",
          ""type"": ""address""
        },
        {
          ""internalType"": ""uint256[]"",
          ""name"": ""_ids"",
          ""type"": ""uint256[]""
        },
        {
          ""internalType"": ""uint256[]"",
          ""name"": ""_values"",
          ""type"": ""uint256[]""
        },
        {
          ""internalType"": ""bytes"",
          ""name"": ""_data"",
          ""type"": ""bytes""
        }
      ],
      ""name"": ""safeBatchTransferFrom"",
      ""outputs"": [],
      ""stateMutability"": ""nonpayable"",
      ""type"": ""function""
    },
    {
      ""inputs"": [
        {
          ""internalType"": ""address"",
          ""name"": ""_owner"",
          ""type"": ""address""
        },
        {
          ""internalType"": ""uint256"",
          ""name"": ""_id"",
          ""type"": ""uint256""
        }
      ],
      ""name"": ""balanceOf"",
      ""outputs"": [
        {
          ""internalType"": ""uint256"",
          ""name"": """",
          ""type"": ""uint256""
        }
      ],
      ""stateMutability"": ""view"",
      ""type"": ""function""
    },
    {
      ""inputs"": [
        {
          ""internalType"": ""address[]"",
          ""name"": ""_owners"",
          ""type"": ""address[]""
        },
        {
          ""internalType"": ""uint256[]"",
          ""name"": ""_ids"",
          ""type"": ""uint256[]""
        }
      ],
      ""name"": ""balanceOfBatch"",
      ""outputs"": [
        {
          ""internalType"": ""uint256[]"",
          ""name"": """",
          ""type"": ""uint256[]""
        }
      ],
      ""stateMutability"": ""view"",
      ""type"": ""function""
    },
    {
      ""inputs"": [
        {
          ""internalType"": ""address"",
          ""name"": ""_operator"",
          ""type"": ""address""
        },
        {
          ""internalType"": ""bool"",
          ""name"": ""_approved"",
          ""type"": ""bool""
        }
      ],
      ""name"": ""setApprovalForAll"",
      ""outputs"": [],
      ""stateMutability"": ""nonpayable"",
      ""type"": ""function""
    },
    {
      ""inputs"": [
        {
          ""internalType"": ""address"",
          ""name"": ""_owner"",
          ""type"": ""address""
        },
        {
          ""internalType"": ""address"",
          ""name"": ""_operator"",
          ""type"": ""address""
        }
      ],
      ""name"": ""isApprovedForAll"",
      ""outputs"": [
        {
          ""internalType"": ""bool"",
          ""name"": """",
          ""type"": ""bool""
        }
      ],
      ""stateMutability"": ""view"",
      ""type"": ""function""
    }
]
";

        public const string CONST_FUNCTION_SAFE_TRANSFER_FROM       = "safeTransferFrom";
        public const string CONST_FUNCTION_SAFE_BATCH_TRANSFER_FROM = "safeBatchTransferFrom";
        public const string CONST_FUNCTION_BALANCE_OF               = "balanceOf";
        public const string CONST_FUNCTION_BALANCE_OF_BATCH         = "balanceOfBatch";
        public const string CONST_FUNCTION_SET_APPROVAL_FOR_ALL     = "setApprovalForAll";
        public const string CONST_FUNCTION_IS_APPROVED_FOR_ALL      = "isApprovedForAll";

        #region PROPERTIES

        public readonly Nethereum.Web3.Web3 SenderWeb3;

		#endregion

		public WonkaEthERC1155OpSource(string psSourceId, string psSenderAddr, string psPwd, string psContractAddr, string psCustomOpMethodName, string psWeb3Url = "") :
			base(psSourceId, psSenderAddr, psPwd, psContractAddr, null, null, psCustomOpMethodName)
		{
			var account = new Nethereum.Web3.Accounts.Account(psPwd);

			if (!String.IsNullOrEmpty(psWeb3Url))
				SenderWeb3 = new Nethereum.Web3.Web3(account, psWeb3Url);
			else
				SenderWeb3 = new Nethereum.Web3.Web3(account);
		}

        public Nethereum.Contracts.Contract GetERC1155TokenContract()
        {
            var contract = SenderWeb3.Eth.GetContract(CONST_ERC_1155_ABI, this.ContractAddress);

            return contract;
        }

        public string InvokeERC1155BalanceOf(string psOwner, string psTokenId, string psDummyVal1 = "", string psDummyVal2 = "")
        {
            var contract = GetERC1155TokenContract();

            var getBalanceOfFunction = contract.GetFunction(CONST_FUNCTION_BALANCE_OF);

            var balance = getBalanceOfFunction.CallAsync<string>(psOwner, psTokenId).Result;

            return balance;
        }

        public string InvokeERC1155IsApprovedForAll(string psOwner, string psOperator, string psDummyVal1 = "", string psDummyVal2 = "")
        {
            var contract = GetERC1155TokenContract();

            var getBalanceOfFunction = contract.GetFunction(CONST_FUNCTION_IS_APPROVED_FOR_ALL);

            var isApproved = getBalanceOfFunction.CallAsync<string>(psOwner, psOperator).Result;

            return isApproved;
        }


        /**
         ** NOTE: Not yet supported
         **
        public string InvokeERC1155BalanceOfBatch(string[] paOwners, string[] psTokenIds, string psDummyVal1 = "", string psDummyVal2 = "")
        {           
        }
         **/

        public string InvokeERC1155SafeTransferFrom(string psFromAcct, string psToAcct, string psTokenId, string psTokenAmt)
        {
            var contract = GetERC1155TokenContract();

            var getSafeTransferFromFunction = contract.GetFunction(CONST_FUNCTION_SAFE_TRANSFER_FROM);

            var gas = getSafeTransferFromFunction.EstimateGasAsync(psFromAcct, psToAcct, psTokenId, psTokenAmt).Result;

            var receipt = getSafeTransferFromFunction.SendTransactionAndWaitForReceiptAsync(this.SenderAddress, gas, null, null, psFromAcct, psToAcct, psTokenId, psTokenAmt, "").Result;

            return psTokenAmt;
        }

        /**
         ** NOTE: Not yet supported
         **
        public string InvokeERC1155SafeBatchTransferFrom(string[] paOwners, string[] psTokenIds, string psDummyVal1 = "", string psDummyVal2 = "")
        {           
        }
         **/

        public string InvokeERC1155SetApprovalForAll(string psOperator, string psIsApproved, string psDummyVal1 = "", string psDummyVal2 = "")
        {
            var contract = GetERC1155TokenContract();

            var getSetApprovalForAllFunction = contract.GetFunction(CONST_FUNCTION_SET_APPROVAL_FOR_ALL);

            bool bIsApproved = Convert.ToBoolean(psIsApproved);

            var gas = getSetApprovalForAllFunction.EstimateGasAsync(psOperator, bIsApproved).Result;

            var receipt = getSetApprovalForAllFunction.SendTransactionAndWaitForReceiptAsync(this.SenderAddress, gas, null, null, psOperator, bIsApproved).Result;

            return psOperator;
        }

    }
}