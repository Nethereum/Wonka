using System;
using System.Globalization;
using System.Numerics;

using Nethereum.StandardTokenEIP20.ContractDefinition;

using Wonka.BizRulesEngine.RuleTree;

namespace Wonka.Eth.Extensions.OpSource.ERC725
{
	public class WonkaEthERC725OpSource : WonkaBizSource
    {
        public const string CONST_ERC_725_ABI =
@"
[
    {
      ""anonymous"": false,
      ""inputs"": [
        {
          ""indexed"": true,
          ""internalType"": ""address"",
          ""name"": ""contractAddress"",
          ""type"": ""address""
        }
      ],
      ""name"": ""ContractCreated"",
      ""type"": ""event""
    },
    {
      ""anonymous"": false,
      ""inputs"": [
        {
          ""indexed"": true,
          ""internalType"": ""bytes32"",
          ""name"": ""key"",
          ""type"": ""bytes32""
        },
        {
          ""indexed"": true,
          ""internalType"": ""bytes32"",
          ""name"": ""value"",
          ""type"": ""bytes32""
        }
      ],
      ""name"": ""DataChanged"",
      ""type"": ""event""
    },
    {
      ""anonymous"": false,
      ""inputs"": [
        {
          ""indexed"": true,
          ""internalType"": ""address"",
          ""name"": ""ownerAddress"",
          ""type"": ""address""
        }
      ],
      ""name"": ""OwnerChanged"",
      ""type"": ""event""
    },
    {
      ""inputs"": [
        {
          ""internalType"": ""address"",
          ""name"": ""owner"",
          ""type"": ""address""
        }
      ],
      ""name"": ""changeOwner"",
      ""outputs"": [],
      ""stateMutability"": ""nonpayable"",
      ""type"": ""function""
    },
    {
      ""inputs"": [
        {
          ""internalType"": ""bytes32"",
          ""name"": ""key"",
          ""type"": ""bytes32""
        }
      ],
      ""name"": ""getData"",
      ""outputs"": [
        {
          ""internalType"": ""bytes32"",
          ""name"": ""value"",
          ""type"": ""bytes32""
        }
      ],
      ""stateMutability"": ""view"",
      ""type"": ""function""
    },
    {
      ""inputs"": [
        {
          ""internalType"": ""bytes32"",
          ""name"": ""key"",
          ""type"": ""bytes32""
        },
        {
          ""internalType"": ""bytes32"",
          ""name"": ""value"",
          ""type"": ""bytes32""
        }
      ],
      ""name"": ""setData"",
      ""outputs"": [],
      ""stateMutability"": ""nonpayable"",
      ""type"": ""function""
    },
    {
      ""inputs"": [
        {
          ""internalType"": ""uint256"",
          ""name"": ""operationType"",
          ""type"": ""uint256""
        },
        {
          ""internalType"": ""address"",
          ""name"": ""to"",
          ""type"": ""address""
        },
        {
          ""internalType"": ""uint256"",
          ""name"": ""value"",
          ""type"": ""uint256""
        },
        {
          ""internalType"": ""bytes"",
          ""name"": ""data"",
          ""type"": ""bytes""
        }
      ],
      ""name"": ""execute"",
      ""outputs"": [],
      ""stateMutability"": ""nonpayable"",
      ""type"": ""function""
    }
  ]
";

        #region PROPERTIES

        public readonly Nethereum.Web3.Web3 SenderWeb3;

        #endregion

        public WonkaEthERC725OpSource(string psSourceId, string psSenderAddr, string psPwd, string psContractAddr, string psCustomOpMethodName, string psWeb3Url = "") :
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
