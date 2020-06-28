using System;
using System.Globalization;
using System.Numerics;

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

        public const string CONST_FUNCTION_CHANGE_OWNER = "changeOwner";
        public const string CONST_FUNCTION_GET_DATA     = "getData";
        public const string CONST_FUNCTION_SET_DATA     = "setData";
        public const string CONST_FUNCTION_EXECUTE      = "execute";

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

        public Nethereum.Contracts.Contract GetERC725IdentityContract()
        {
            var contract = SenderWeb3.Eth.GetContract(CONST_ERC_725_ABI, this.ContractAddress);

            return contract;
        }

        public string InvokeERC725ChangeOwner(string psNewOwner, string psDummyVal1 = "", string psDummyVal2 = "", string psDummyVal3 = "")
        {
            var contract = GetERC725IdentityContract();

            var changeOwnerFunction = contract.GetFunction(CONST_FUNCTION_CHANGE_OWNER);

            var gas = changeOwnerFunction.EstimateGasAsync(psNewOwner).Result;

            var receipt = changeOwnerFunction.SendTransactionAndWaitForReceiptAsync(this.SenderAddress, gas, null, null, psNewOwner).Result;

            return receipt.TransactionHash;
        }

        public string InvokeERC725Execute(string psOpType, string psTo, string psValue, string psData)
        {
            var contract = GetERC725IdentityContract();

            var setDataFunction = contract.GetFunction(CONST_FUNCTION_EXECUTE);

            var OpTypeBigInt =
                System.Numerics.BigInteger.Parse(psOpType, System.Globalization.NumberStyles.HexNumber);

            var ValueBigInt =
                System.Numerics.BigInteger.Parse(psValue, System.Globalization.NumberStyles.HexNumber);

            var gas = setDataFunction.EstimateGasAsync(OpTypeBigInt, psTo, ValueBigInt, psData).Result;

            var receipt = setDataFunction.SendTransactionAndWaitForReceiptAsync(this.SenderAddress, gas, null, null, OpTypeBigInt, psTo, ValueBigInt, psData).Result;

            return receipt.TransactionHash;
        }

        public string InvokeERC725GetData(string psKey, string psDummyVal1 = "", string psDummyVal2 = "", string psDummyVal3 = "")
        {
            var contract = GetERC725IdentityContract();

            var getDataFunction = contract.GetFunction(CONST_FUNCTION_GET_DATA);

            var valueData = getDataFunction.CallAsync<string>(psKey).Result;

            return valueData;
        }

        public string InvokeERC725SetData(string psKey, string psValue = "", string psDummyVal1 = "", string psDummyVal2 = "")
        {
            var contract = GetERC725IdentityContract();

            var setDataFunction = contract.GetFunction(CONST_FUNCTION_SET_DATA);

            var gas = setDataFunction.EstimateGasAsync(psKey, psValue).Result;

            var receipt = setDataFunction.SendTransactionAndWaitForReceiptAsync(this.SenderAddress, gas, null, null, psKey, psValue).Result;

            return receipt.TransactionHash;
        }

    }
}
