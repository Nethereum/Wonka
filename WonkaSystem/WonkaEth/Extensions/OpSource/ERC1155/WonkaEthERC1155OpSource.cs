using System;
using System.Globalization;
using System.Numerics;

using Nethereum.StandardTokenEIP20.ContractDefinition;

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
	}
}