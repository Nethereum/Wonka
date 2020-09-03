using System;
using System.Collections.Generic;
using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Wonka.Eth.Autogen.ChronoLog
{
    public partial class ChronoLogDeployment : ChronoLogDeploymentBase
    {
        public static string ABI =
@"
[
    {
      ""inputs"": [],
      ""stateMutability"": ""nonpayable"",
      ""type"": ""constructor""
    },
    {
      ""inputs"": [
        {
          ""internalType"": ""uint256"",
          ""name"": """",
          ""type"": ""uint256""
        }
      ],
      ""name"": ""chronoEvents"",
      ""outputs"": [
        {
          ""internalType"": ""uint256"",
          ""name"": ""eventId"",
          ""type"": ""uint256""
        },
        {
          ""internalType"": ""bytes32"",
          ""name"": ""eventName"",
          ""type"": ""bytes32""
        },
        {
          ""internalType"": ""bytes32"",
          ""name"": ""eventType"",
          ""type"": ""bytes32""
        },
        {
          ""internalType"": ""string"",
          ""name"": ""eventDescription"",
          ""type"": ""string""
        },
        {
          ""internalType"": ""uint256"",
          ""name"": ""eventEpochTime"",
          ""type"": ""uint256""
        },
        {
          ""internalType"": ""bytes32"",
          ""name"": ""publicData"",
          ""type"": ""bytes32""
        },
        {
          ""internalType"": ""bytes32"",
          ""name"": ""privateHash"",
          ""type"": ""bytes32""
        },
        {
          ""internalType"": ""string"",
          ""name"": ""eventInfoUrl"",
          ""type"": ""string""
        },
        {
          ""internalType"": ""bool"",
          ""name"": ""isValue"",
          ""type"": ""bool""
        }
      ],
      ""stateMutability"": ""view"",
      ""type"": ""function""
    },
    {
      ""inputs"": [
        {
          ""internalType"": ""bytes32"",
          ""name"": ""uniqueName"",
          ""type"": ""bytes32""
        },
        {
          ""internalType"": ""bytes32"",
          ""name"": ""eType"",
          ""type"": ""bytes32""
        },
        {
          ""internalType"": ""string"",
          ""name"": ""desc"",
          ""type"": ""string""
        },
        {
          ""internalType"": ""bytes32"",
          ""name"": ""data"",
          ""type"": ""bytes32""
        },
        {
          ""internalType"": ""bytes32"",
          ""name"": ""hash"",
          ""type"": ""bytes32""
        },
        {
          ""internalType"": ""string"",
          ""name"": ""url"",
          ""type"": ""string""
        }
      ],
      ""name"": ""addChronoLogEvent"",
      ""outputs"": [],
      ""stateMutability"": ""nonpayable"",
      ""type"": ""function""
    },
    {
      ""inputs"": [
        {
          ""internalType"": ""bytes32"",
          ""name"": ""uniqueName"",
          ""type"": ""bytes32""
        }
      ],
      ""name"": ""getChronoLogBasic"",
      ""outputs"": [
        {
          ""internalType"": ""uint256"",
          ""name"": """",
          ""type"": ""uint256""
        },
        {
          ""internalType"": ""bytes32"",
          ""name"": """",
          ""type"": ""bytes32""
        },
        {
          ""internalType"": ""bytes32"",
          ""name"": """",
          ""type"": ""bytes32""
        },
        {
          ""internalType"": ""string"",
          ""name"": """",
          ""type"": ""string""
        }
      ],
      ""stateMutability"": ""view"",
      ""type"": ""function""
    },
    {
      ""inputs"": [
        {
          ""internalType"": ""bytes32"",
          ""name"": ""uniqueName"",
          ""type"": ""bytes32""
        }
      ],
      ""name"": ""getChronoLogEvent"",
      ""outputs"": [
        {
          ""internalType"": ""bytes32"",
          ""name"": """",
          ""type"": ""bytes32""
        },
        {
          ""internalType"": ""string"",
          ""name"": """",
          ""type"": ""string""
        },
        {
          ""internalType"": ""uint256"",
          ""name"": """",
          ""type"": ""uint256""
        },
        {
          ""internalType"": ""bytes32"",
          ""name"": """",
          ""type"": ""bytes32""
        },
        {
          ""internalType"": ""bytes32"",
          ""name"": """",
          ""type"": ""bytes32""
        },
        {
          ""internalType"": ""string"",
          ""name"": """",
          ""type"": ""string""
        }
      ],
      ""stateMutability"": ""view"",
      ""type"": ""function""
    },
    {
      ""inputs"": [
        {
          ""internalType"": ""bytes32"",
          ""name"": ""eType"",
          ""type"": ""bytes32""
        }
      ],
      ""name"": ""getChronoLogEventsByType"",
      ""outputs"": [
        {
          ""internalType"": ""bytes32[]"",
          ""name"": """",
          ""type"": ""bytes32[]""
        }
      ],
      ""stateMutability"": ""view"",
      ""type"": ""function""
    },
    {
      ""inputs"": [
        {
          ""internalType"": ""bytes32"",
          ""name"": ""eType"",
          ""type"": ""bytes32""
        },
        {
          ""internalType"": ""uint256"",
          ""name"": ""startTime"",
          ""type"": ""uint256""
        },
        {
          ""internalType"": ""uint256"",
          ""name"": ""endTime"",
          ""type"": ""uint256""
        }
      ],
      ""name"": ""getChronoLogEventsByType"",
      ""outputs"": [
        {
          ""internalType"": ""bytes32[]"",
          ""name"": """",
          ""type"": ""bytes32[]""
        }
      ],
      ""stateMutability"": ""view"",
      ""type"": ""function""
    }
  ]
";

        public ChronoLogDeployment() : base(BYTECODE) { }
        public ChronoLogDeployment(string byteCode) : base(byteCode) { }
    }

    public class ChronoLogDeploymentBase : ContractDeploymentMessage
    {
        public static string BYTECODE = "0x6080604052600160005534801561001557600080fd5b50600180546001600160a01b03191633179055611003806100376000396000f3fe608060405234801561001057600080fd5b50600436106100625760003560e01c806329d20c151461006757806366fe3cbf14610197578063778a4c8514610241578063926677a514610358578063b571eb00146103c5578063f094336d1461050c575b600080fd5b6100846004803603602081101561007d57600080fd5b5035610535565b604051808a8152602001898152602001888152602001806020018781526020018681526020018581526020018060200184151515158152602001838103835289818151815260200191508051906020019080838360005b838110156100f35781810151838201526020016100db565b50505050905090810190601f1680156101205780820380516001836020036101000a031916815260200191505b50838103825285518152855160209182019187019080838360005b8381101561015357818101518382015260200161013b565b50505050905090810190601f1680156101805780820380516001836020036101000a031916815260200191505b509b50505050505050505050505060405180910390f35b6101b4600480360360208110156101ad57600080fd5b50356106b8565b6040518085815260200184815260200183815260200180602001828103825283818151815260200191508051906020019080838360005b838110156102035781810151838201526020016101eb565b50505050905090810190601f1680156102305780820380516001836020036101000a031916815260200191505b509550505050505060405180910390f35b61025e6004803603602081101561025757600080fd5b503561077c565b604051808781526020018060200186815260200185815260200184815260200180602001838103835288818151815260200191508051906020019080838360005b838110156102b757818101518382015260200161029f565b50505050905090810190601f1680156102e45780820380516001836020036101000a031916815260200191505b50838103825284518152845160209182019186019080838360005b838110156103175781810151838201526020016102ff565b50505050905090810190601f1680156103445780820380516001836020036101000a031916815260200191505b509850505050505050505060405180910390f35b6103756004803603602081101561036e57600080fd5b50356108e5565b60408051602080825283518183015283519192839290830191858101910280838360005b838110156103b1578181015183820152602001610399565b505050509050019250505060405180910390f35b61050a600480360360c08110156103db57600080fd5b81359160208101359181019060608101604082013564010000000081111561040257600080fd5b82018360208201111561041457600080fd5b8035906020019184600183028401116401000000008311171561043657600080fd5b91908080601f016020809104026020016040519081016040528093929190818152602001838380828437600092019190915250929584359560208601359591945092506060810191506040013564010000000081111561049557600080fd5b8201836020820111156104a757600080fd5b803590602001918460018302840111640100000000831117156104c957600080fd5b91908080601f016020809104026020016040519081016040528093929190818152602001838380828437600092019190915250929550610947945050505050565b005b6103756004803603606081101561052257600080fd5b5080359060208101359060400135610ccd565b6003818154811061054257fe5b9060005260206000209060090201600091509050806000015490806001015490806002015490806003018054600181600116156101000203166002900480601f0160208091040260200160405190810160405280929190818152602001828054600181600116156101000203166002900480156106005780601f106105d557610100808354040283529160200191610600565b820191906000526020600020905b8154815290600101906020018083116105e357829003601f168201915b505050600484015460058501546006860154600787018054604080516020601f600260001961010060018816150201909516949094049384018190048102820181019092528281529899959894975092955090918301828280156106a55780601f1061067a576101008083540402835291602001916106a5565b820191906000526020600020905b81548152906001019060200180831161068857829003601f168201915b5050506008909301549192505060ff1689565b60008181526002602081815260408084206004810154600582015460068301546007909301805485516001821615610100026000190190911697909704601f8101879004870288018701909552848752879687966060969495939483918301828280156107665780601f1061073b57610100808354040283529160200191610766565b820191906000526020600020905b81548152906001019060200180831161074957829003601f168201915b5050505050905093509350935093509193509193565b60008181526002602081815260408084208084015460048201546005830154600684015460038501805487516001821615610100026000190190911699909904601f810189900489028a0189019097528689526060988a98899889988c989794969495936007909101929091879183018282801561083b5780601f106108105761010080835404028352916020019161083b565b820191906000526020600020905b81548152906001019060200180831161081e57829003601f168201915b5050845460408051602060026001851615610100026000190190941693909304601f8101849004840282018401909252818152959a50869450925084019050828280156108c95780601f1061089e576101008083540402835291602001916108c9565b820191906000526020600020905b8154815290600101906020018083116108ac57829003601f168201915b5050505050905095509550955095509550955091939550919395565b60008181526005602090815260409182902080548351818402810184019094528084526060939283018282801561093b57602002820191906000526020600020905b815481526020019060010190808311610927575b50505050509050919050565b6001546001600160a01b031633146109905760405162461bcd60e51b8152600401808060200182810382526043815260200180610f676043913960600191505060405180910390fd5b60008681526002602052604090206008015460ff16156109e15760405162461bcd60e51b8152600401808060200182810382526024815260200180610faa6024913960400191505060405180910390fd5b604080516101208101825260008054825260208083018a81529383018981526060840189815242608086015260a0850189905260c0850188905260e0850187905260016101008601819052600380549182018155909452845160099094027fc2575a0e9e593c00f959f8c92f12db2869c3395a3b0502d05e2516446f71f85b810194855595517fc2575a0e9e593c00f959f8c92f12db2869c3395a3b0502d05e2516446f71f85c87015590517fc2575a0e9e593c00f959f8c92f12db2869c3395a3b0502d05e2516446f71f85d86015551805193949293610ae9937fc2575a0e9e593c00f959f8c92f12db2869c3395a3b0502d05e2516446f71f85e01929190910190610e56565b506080820151600482015560a0820151600582015560c0820151600682015560e08201518051610b23916007840191602090910190610e56565b5061010091909101516008909101805460ff1916911515919091179055600380546000198101908110610b5257fe5b906000526020600020906009020160026000600360016003805490500381548110610b7957fe5b906000526020600020906009020160010154815260200190815260200160002060008201548160000155600182015481600101556002820154816002015560038201816003019080546001816001161561010002031660029004610bde929190610ed4565b5060048201548160040155600582015481600501556006820154816006015560078201816007019080546001816001161561010002031660029004610c24929190610ed4565b506008918201549101805460ff191660ff9092161515919091179055600380546000916018916000198101908110610c5857fe5b90600052602060002090600902016004015481610c7157fe5b04600081815260046020526040902054909150610c9b576000805482825260046020526040909120555b505060008054600190810182559481526005602090815260408220805496870181558252902090930193909355505050565b6018820460009081526004602052604090205460609081908015610e4d5760018101600090815260046020526040812054908115610d5257508082810367ffffffffffffffff81118015610d2057600080fd5b50604051908082528060200260200182016040528015610d4a578160200160208202803683370190505b509350610d9d565b5060035482810367ffffffffffffffff81118015610d6f57600080fd5b50604051908082528060200260200182016040528015610d99578160200160208202803683370190505b5093505b6000835b82811015610e485760038181548110610db657fe5b9060005260206000209060090201600201548a148015610df457508760038281548110610ddf57fe5b90600052602060002090600902016004015411155b15610e3b5760038181548110610e0657fe5b906000526020600020906009020160010154868380600101945081518110610e2a57fe5b602002602001018181525050610e40565b610e48565b600101610da1565b505050505b50949350505050565b828054600181600116156101000203166002900490600052602060002090601f016020900481019282601f10610e9757805160ff1916838001178555610ec4565b82800160010185558215610ec4579182015b82811115610ec4578251825591602001919060010190610ea9565b50610ed0929150610f49565b5090565b828054600181600116156101000203166002900490600052602060002090601f016020900481019282601f10610f0d5780548555610ec4565b82800160010185558215610ec457600052602060002091601f016020900482015b82811115610ec4578254825591600101919060010190610f2e565b610f6391905b80821115610ed05760008155600101610f4f565b9056fe5468652063616c6c6572206f662074686973206d6574686f6420646f6573206e6f742068617665207065726d697373696f6e20666f72207468697320616374696f6e2e4576656e74207769746820756e6971756520494420616c7265616479206578697374732ea2646970667358221220fb255d672d44212f2bba962c0612c23d1bba577831631a34df420f1bec0aa03d64736f6c63430006080033";
        public ChronoLogDeploymentBase() : base(BYTECODE) { }
        public ChronoLogDeploymentBase(string byteCode) : base(byteCode) { }

    }

    public partial class ChronoEventsFunction : ChronoEventsFunctionBase { }

    [Function("chronoEvents", typeof(ChronoEventsOutputDTO))]
    public class ChronoEventsFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class AddChronoLogEventFunction : AddChronoLogEventFunctionBase { }

    [Function("addChronoLogEvent")]
    public class AddChronoLogEventFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "uniqueName", 1)]
        public virtual string UniqueName { get; set; }
        [Parameter("bytes32", "eType", 2)]
        public virtual string EType { get; set; }
        [Parameter("string", "desc", 3)]
        public virtual string Desc { get; set; }
        [Parameter("bytes32", "data", 4)]
        public virtual string Data { get; set; }
        [Parameter("bytes32", "hash", 5)]
        public virtual string Hash { get; set; }
        [Parameter("string", "url", 6)]
        public virtual string Url { get; set; }
    }

    public partial class GetChronoLogBasicFunction : GetChronoLogBasicFunctionBase { }

    [Function("getChronoLogBasic", typeof(GetChronoLogBasicOutputDTO))]
    public class GetChronoLogBasicFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "uniqueName", 1)]
        public virtual byte[] UniqueName { get; set; }
    }

    public partial class GetChronoLogEventFunction : GetChronoLogEventFunctionBase { }

    [Function("getChronoLogEvent", typeof(GetChronoLogEventOutputDTO))]
    public class GetChronoLogEventFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "uniqueName", 1)]
        public virtual byte[] UniqueName { get; set; }
    }

    public partial class GetChronoLogEventsByTypeFunction : GetChronoLogEventsByTypeFunctionBase { }

    [Function("getChronoLogEventsByType", "bytes32[]")]
    public class GetChronoLogEventsByTypeFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "eType", 1)]
        public virtual byte[] EType { get; set; }
    }

    public partial class GetChronoLogEventsByTypeAndTimeFunction : GetChronoLogEventsByTypeAndTimeFunctionBase { }

    [Function("getChronoLogEventsByTypeAndTime", "bytes32[]")]
    public class GetChronoLogEventsByTypeAndTimeFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "eType", 1)]
        public virtual byte[] EType { get; set; }
        [Parameter("uint256", "startTime", 2)]
        public virtual BigInteger StartTime { get; set; }
        [Parameter("uint256", "endTime", 3)]
        public virtual BigInteger EndTime { get; set; }
    }

    public partial class ChronoEventsOutputDTO : ChronoEventsOutputDTOBase { }

    [FunctionOutput]
    public class ChronoEventsOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("uint256", "eventId", 1)]
        public virtual BigInteger EventId { get; set; }
        [Parameter("bytes32", "eventName", 2)]
        public virtual byte[] EventName { get; set; }
        [Parameter("bytes32", "eventType", 3)]
        public virtual byte[] EventType { get; set; }
        [Parameter("string", "eventDescription", 4)]
        public virtual string EventDescription { get; set; }
        [Parameter("uint256", "eventEpochTime", 5)]
        public virtual BigInteger EventEpochTime { get; set; }
        [Parameter("bytes32", "publicData", 6)]
        public virtual byte[] PublicData { get; set; }
        [Parameter("bytes32", "privateHash", 7)]
        public virtual byte[] PrivateHash { get; set; }
        [Parameter("string", "eventInfoUrl", 8)]
        public virtual string EventInfoUrl { get; set; }
        [Parameter("bool", "isValue", 9)]
        public virtual bool IsValue { get; set; }
    }

    public partial class GetChronoLogBasicOutputDTO : GetChronoLogBasicOutputDTOBase { }

    [FunctionOutput]
    public class GetChronoLogBasicOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
        [Parameter("bytes32", "", 2)]
        public virtual byte[] ReturnValue2 { get; set; }
        [Parameter("bytes32", "", 3)]
        public virtual byte[] ReturnValue3 { get; set; }
        [Parameter("string", "", 4)]
        public virtual string ReturnValue4 { get; set; }
    }

    public partial class GetChronoLogEventOutputDTO : GetChronoLogEventOutputDTOBase { }

    [FunctionOutput]
    public class GetChronoLogEventOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("bytes32", "", 1)]
        public virtual byte[] ReturnValue1 { get; set; }
        [Parameter("string", "", 2)]
        public virtual string ReturnValue2 { get; set; }
        [Parameter("uint256", "", 3)]
        public virtual BigInteger ReturnValue3 { get; set; }
        [Parameter("bytes32", "", 4)]
        public virtual byte[] ReturnValue4 { get; set; }
        [Parameter("bytes32", "", 5)]
        public virtual byte[] ReturnValue5 { get; set; }
        [Parameter("string", "", 6)]
        public virtual string ReturnValue6 { get; set; }
    }

    public partial class GetChronoLogEventsByTypeOutputDTO : GetChronoLogEventsByTypeOutputDTOBase { }

    [FunctionOutput]
    public class GetChronoLogEventsByTypeOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("bytes32[]", "", 1)]
        public virtual List<byte[]> ReturnValue1 { get; set; }
    }

    public partial class GetChronoLogEventsByTypeAndTimeOutputDTO : GetChronoLogEventsByTypeAndTimeOutputDTOBase { }

    [FunctionOutput]
    public class GetChronoLogEventsByTypeAndTimeOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("bytes32[]", "", 1)]
        public virtual List<byte[]> ReturnValue1 { get; set; }
    }

}
