using System;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

using Wonka.BizRulesEngine.RuleTree;
using Wonka.Eth.Autogen.Baseline.Registry;

namespace Wonka.Eth.Extensions.OpSource.Baseline.Registry
{
    /**
     ** NOTE: Under construction
     **/
	public class WonkaEthBaselineOrgRegistryOpSource : WonkaBizSource
	{
		const string CONST_ORG_REGISTRY_ABI =
@"
[
    {
      ""anonymous"": false,
      ""inputs"": [
        {
          ""indexed"": false,
          ""internalType"": ""bytes32"",
          ""name"": ""_name"",
          ""type"": ""bytes32""
        },
        {
          ""indexed"": false,
          ""internalType"": ""address"",
          ""name"": ""_address"",
          ""type"": ""address""
        },
        {
          ""indexed"": false,
          ""internalType"": ""bytes"",
          ""name"": ""_messagingEndpoint"",
          ""type"": ""bytes""
        },
        {
          ""indexed"": false,
          ""internalType"": ""bytes"",
          ""name"": ""_whisperKey"",
          ""type"": ""bytes""
        },
        {
          ""indexed"": false,
          ""internalType"": ""bytes"",
          ""name"": ""_zkpPublicKey"",
          ""type"": ""bytes""
        },
        {
          ""indexed"": false,
          ""internalType"": ""bytes"",
          ""name"": ""_metadata"",
          ""type"": ""bytes""
        }
      ],
      ""name"": ""RegisterOrg"",
      ""type"": ""event""
    },
    {
      ""inputs"": [
        {
          ""internalType"": ""uint256"",
          ""name"": """",
          ""type"": ""uint256""
        }
      ],
      ""name"": ""orgs"",
      ""outputs"": [
        {
          ""internalType"": ""address"",
          ""name"": ""orgAddress"",
          ""type"": ""address""
        },
        {
          ""internalType"": ""bytes32"",
          ""name"": ""name"",
          ""type"": ""bytes32""
        },
        {
          ""internalType"": ""bytes"",
          ""name"": ""messagingEndpoint"",
          ""type"": ""bytes""
        },
        {
          ""internalType"": ""bytes"",
          ""name"": ""whisperKey"",
          ""type"": ""bytes""
        },
        {
          ""internalType"": ""bytes"",
          ""name"": ""zkpPublicKey"",
          ""type"": ""bytes""
        },
        {
          ""internalType"": ""bytes"",
          ""name"": ""metadata"",
          ""type"": ""bytes""
        }
      ],
      ""stateMutability"": ""view"",
      ""type"": ""function""
    },
    {
      ""inputs"": [],
      ""name"": ""getInterfaces"",
      ""outputs"": [
        {
          ""internalType"": ""bytes4"",
          ""name"": """",
          ""type"": ""bytes4""
        }
      ],
      ""stateMutability"": ""pure"",
      ""type"": ""function""
    },
    {
      ""inputs"": [
        {
          ""internalType"": ""address"",
          ""name"": ""_newManager"",
          ""type"": ""address""
        }
      ],
      ""name"": ""assignManager"",
      ""outputs"": [],
      ""stateMutability"": ""nonpayable"",
      ""type"": ""function""
    },
    {
      ""inputs"": [
        {
          ""internalType"": ""address"",
          ""name"": ""_address"",
          ""type"": ""address""
        },
        {
          ""internalType"": ""bytes32"",
          ""name"": ""_name"",
          ""type"": ""bytes32""
        },
        {
          ""internalType"": ""bytes"",
          ""name"": ""_messagingEndpoint"",
          ""type"": ""bytes""
        },
        {
          ""internalType"": ""bytes"",
          ""name"": ""_whisperKey"",
          ""type"": ""bytes""
        },
        {
          ""internalType"": ""bytes"",
          ""name"": ""_zkpPublicKey"",
          ""type"": ""bytes""
        },
        {
          ""internalType"": ""bytes"",
          ""name"": ""_metadata"",
          ""type"": ""bytes""
        }
      ],
      ""name"": ""registerOrg"",
      ""outputs"": [
        {
          ""internalType"": ""bool"",
          ""name"": """",
          ""type"": ""bool""
        }
      ],
      ""stateMutability"": ""nonpayable"",
      ""type"": ""function""
    },
    {
      ""inputs"": [
        {
          ""internalType"": ""bytes32"",
          ""name"": ""_groupName"",
          ""type"": ""bytes32""
        },
        {
          ""internalType"": ""address"",
          ""name"": ""_tokenAddress"",
          ""type"": ""address""
        },
        {
          ""internalType"": ""address"",
          ""name"": ""_shieldAddress"",
          ""type"": ""address""
        },
        {
          ""internalType"": ""address"",
          ""name"": ""_verifierAddress"",
          ""type"": ""address""
        }
      ],
      ""name"": ""registerInterfaces"",
      ""outputs"": [
        {
          ""internalType"": ""bool"",
          ""name"": """",
          ""type"": ""bool""
        }
      ],
      ""stateMutability"": ""nonpayable"",
      ""type"": ""function""
    },
    {
      ""inputs"": [],
      ""name"": ""getOrgCount"",
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
          ""internalType"": ""address"",
          ""name"": ""_address"",
          ""type"": ""address""
        }
      ],
      ""name"": ""getOrg"",
      ""outputs"": [
        {
          ""internalType"": ""address"",
          ""name"": """",
          ""type"": ""address""
        },
        {
          ""internalType"": ""bytes32"",
          ""name"": """",
          ""type"": ""bytes32""
        },
        {
          ""internalType"": ""bytes"",
          ""name"": """",
          ""type"": ""bytes""
        },
        {
          ""internalType"": ""bytes"",
          ""name"": """",
          ""type"": ""bytes""
        },
        {
          ""internalType"": ""bytes"",
          ""name"": """",
          ""type"": ""bytes""
        },
        {
          ""internalType"": ""bytes"",
          ""name"": """",
          ""type"": ""bytes""
        }
      ],
      ""stateMutability"": ""view"",
      ""type"": ""function""
    },
    {
      ""inputs"": [],
      ""name"": ""getInterfaceAddresses"",
      ""outputs"": [
        {
          ""internalType"": ""bytes32[]"",
          ""name"": """",
          ""type"": ""bytes32[]""
        },
        {
          ""internalType"": ""address[]"",
          ""name"": """",
          ""type"": ""address[]""
        },
        {
          ""internalType"": ""address[]"",
          ""name"": """",
          ""type"": ""address[]""
        },
        {
          ""internalType"": ""address[]"",
          ""name"": """",
          ""type"": ""address[]""
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

        public WonkaEthBaselineOrgRegistryOpSource(string psSourceId, string psSenderAddr, string psPwd, string psContractAddr, string psCustomOpMethodName, string psWeb3Url = "") :
            base(psSourceId, psSenderAddr, psPwd, psContractAddr, null, null, psCustomOpMethodName)
        {
            var account = new Nethereum.Web3.Accounts.Account(psPwd);

            if (!String.IsNullOrEmpty(psWeb3Url))
                SenderWeb3 = new Nethereum.Web3.Web3(account, psWeb3Url);
            else
                SenderWeb3 = new Nethereum.Web3.Web3(account);
        }

        public string InvokeGetOrg(string psAddress, string psDummyVal1, string psDummyValue2, string psDummyValue3)
		{
            var orgFunction = new GetOrgFunction() { Address = psAddress };

            var getOrgHandler = SenderWeb3.Eth.GetContractQueryHandler<GetOrgFunction>();

            var orgData =
                getOrgHandler.QueryDeserializingToObjectAsync<GetOrgOutputDTO>(orgFunction, this.ContractAddress).Result;

            return orgData.name;

        }

        public string InvokeGetOrgCount(string psDummyValue0, string psDummyVal1, string psDummyValue2, string psDummyValue3)
        {
            var orgCountFunction = new GetOrgCountFunction();

            var getOrgCountHandler = SenderWeb3.Eth.GetContractQueryHandler<GetOrgCountFunction>();

            var nOrgCount =
                getOrgCountHandler.QueryAsync<long>(this.ContractAddress, orgCountFunction).Result;

            return Convert.ToString(nOrgCount);

        }

        public string InvokeRegisterOrg(string psAddress, string psName, string psMsgEndpointMetadata, string psWhisperAndZkpPublicKeys)
        {
            var registerFunction =
                new RegisterOrgFunction() { Address = psAddress };

            if (!String.IsNullOrEmpty(psName))
			{
                registerFunction.Name = psName;
			}

            if (!String.IsNullOrEmpty(psMsgEndpointMetadata) && psMsgEndpointMetadata.Contains("|"))
			{
                string[] values = psMsgEndpointMetadata.Split(new char[1] { '|' });

                if (values.Length >= 1)
				{
                    registerFunction.MessagingEndpoint = values[0];

                    if (values.Length >= 2)
                        registerFunction.Metadata = values[1];
				}
			}

            if (!String.IsNullOrEmpty(psWhisperAndZkpPublicKeys) && psWhisperAndZkpPublicKeys.Contains("|"))
            {
                string[] values = psWhisperAndZkpPublicKeys.Split(new char[1] { '|' });

                if (values.Length >= 1)
                {
                    registerFunction.WhisperKey = values[0];

                    if (values.Length >= 2)
                        registerFunction.ZkpPublicKey = values[1];
                }
            }

            var registerOrgHandler = SenderWeb3.Eth.GetContractTransactionHandler<RegisterOrgFunction>();

            var receipt =
                registerOrgHandler.SendRequestAndWaitForReceiptAsync(this.ContractAddress, registerFunction).Result;

            return receipt.TransactionHash;
        }

    }
}
