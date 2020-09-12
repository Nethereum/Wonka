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
          ""internalType"": ""string"",
          ""name"": ""privateHash"",
          ""type"": ""string""
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
          ""internalType"": ""string"",
          ""name"": ""hash"",
          ""type"": ""string""
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
          ""internalType"": ""string"",
          ""name"": """",
          ""type"": ""string""
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
          ""internalType"": ""string"",
          ""name"": """",
          ""type"": ""string""
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
      ""name"": ""getChronoLogEventsByTypeAndTime"",
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
        }
      ],
      ""name"": ""getChronoLogEventFirst"",
      ""outputs"": [
        {
          ""internalType"": ""bytes32"",
          ""name"": """",
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
          ""name"": ""eType"",
          ""type"": ""bytes32""
        }
      ],
      ""name"": ""getChronoLogEventLatest"",
      ""outputs"": [
        {
          ""internalType"": ""bytes32"",
          ""name"": """",
          ""type"": ""bytes32""
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
        public static string BYTECODE = "0x6080604052600160005534801561001557600080fd5b50600180546001600160a01b03191633179055611447806100376000396000f3fe608060405234801561001057600080fd5b50600436106100885760003560e01c8063778a4c851161005b578063778a4c8514610518578063926677a51461068e578063d9c4e255146106fb578063fcba5e3a1461071857610088565b806306e14ce11461008d57806329d20c151461025157806366fe3cbf146103e05780636c060be4146104e9575b600080fd5b61024f600480360360c08110156100a357600080fd5b813591602081013591810190606081016040820135600160201b8111156100c957600080fd5b8201836020820111156100db57600080fd5b803590602001918460018302840111600160201b831117156100fc57600080fd5b91908080601f01602080910402602001604051908101604052809392919081815260200183838082843760009201919091525092958435959094909350604081019250602001359050600160201b81111561015657600080fd5b82018360208201111561016857600080fd5b803590602001918460018302840111600160201b8311171561018957600080fd5b91908080601f0160208091040260200160405190810160405280939291908181526020018383808284376000920191909152509295949360208101935035915050600160201b8111156101db57600080fd5b8201836020820111156101ed57600080fd5b803590602001918460018302840111600160201b8311171561020e57600080fd5b91908080601f016020809104026020016040519081016040528093929190818152602001838380828437600092019190915250929550610741945050505050565b005b61026e6004803603602081101561026757600080fd5b5035610af9565b604051808a81526020018981526020018881526020018060200187815260200186815260200180602001806020018515151515815260200184810384528a818151815260200191508051906020019080838360005b838110156102db5781810151838201526020016102c3565b50505050905090810190601f1680156103085780820380516001836020036101000a031916815260200191505b50848103835287518152875160209182019189019080838360005b8381101561033b578181015183820152602001610323565b50505050905090810190601f1680156103685780820380516001836020036101000a031916815260200191505b50848103825286518152865160209182019188019080838360005b8381101561039b578181015183820152602001610383565b50505050905090810190601f1680156103c85780820380516001836020036101000a031916815260200191505b509c5050505050505050505050505060405180910390f35b6103fd600480360360208110156103f657600080fd5b5035610d11565b604051808581526020018481526020018060200180602001838103835285818151815260200191508051906020019080838360005b8381101561044a578181015183820152602001610432565b50505050905090810190601f1680156104775780820380516001836020036101000a031916815260200191505b50838103825284518152845160209182019186019080838360005b838110156104aa578181015183820152602001610492565b50505050905090810190601f1680156104d75780820380516001836020036101000a031916815260200191505b50965050505050505060405180910390f35b610506600480360360208110156104ff57600080fd5b5035610e63565b60408051918252519081900360200190f35b6105356004803603602081101561052e57600080fd5b5035610e90565b60405180878152602001806020018681526020018581526020018060200180602001848103845289818151815260200191508051906020019080838360005b8381101561058c578181015183820152602001610574565b50505050905090810190601f1680156105b95780820380516001836020036101000a031916815260200191505b50848103835286518152865160209182019188019080838360005b838110156105ec5781810151838201526020016105d4565b50505050905090810190601f1680156106195780820380516001836020036101000a031916815260200191505b50848103825285518152855160209182019187019080838360005b8381101561064c578181015183820152602001610634565b50505050905090810190601f1680156106795780820380516001836020036101000a031916815260200191505b50995050505050505050505060405180910390f35b6106ab600480360360208110156106a457600080fd5b5035611086565b60408051602080825283518183015283519192839290830191858101910280838360005b838110156106e75781810151838201526020016106cf565b505050509050019250505060405180910390f35b6105066004803603602081101561071157600080fd5b50356110e8565b6106ab6004803603606081101561072e57600080fd5b5080359060208101359060400135611108565b6001546001600160a01b0316331461078a5760405162461bcd60e51b81526004018080602001828103825260438152602001806113ab6043913960600191505060405180910390fd5b60008681526002602052604090206008015460ff16156107db5760405162461bcd60e51b81526004018080602001828103825260248152602001806113ee6024913960400191505060405180910390fd5b604080516101208101825260008054825260208083018a81529383018981526060840189815242608086015260a0850189905260c0850188905260e0850187905260016101008601819052600380549182018155909452845160099094027fc2575a0e9e593c00f959f8c92f12db2869c3395a3b0502d05e2516446f71f85b810194855595517fc2575a0e9e593c00f959f8c92f12db2869c3395a3b0502d05e2516446f71f85c87015590517fc2575a0e9e593c00f959f8c92f12db2869c3395a3b0502d05e2516446f71f85d860155518051939492936108e3937fc2575a0e9e593c00f959f8c92f12db2869c3395a3b0502d05e2516446f71f85e0192919091019061129a565b506080820151600482015560a0820151600582015560c0820151805161091391600684019160209091019061129a565b5060e0820151805161092f91600784019160209091019061129a565b5061010091909101516008909101805460ff191691151591909117905560038054600019810190811061095e57fe5b90600052602060002090600902016002600060036001600380549050038154811061098557fe5b9060005260206000209060090201600101548152602001908152602001600020600082015481600001556001820154816001015560028201548160020155600382018160030190805460018160011615610100020316600290046109ea929190611318565b50600482015481600401556005820154816005015560068201816006019080546001816001161561010002031660029004610a26929190611318565b5060078201816007019080546001816001161561010002031660029004610a4e929190611318565b506008918201549101805460ff191660ff90921615159190911790556003805460009162015180916000198101908110610a8457fe5b90600052602060002090600902016004015481610a9d57fe5b04600081815260046020526040902054909150610ac7576000805482825260046020526040909120555b505060008054600190810182559481526005602090815260408220805496870181558252902090930193909355505050565b60038181548110610b0657fe5b9060005260206000209060090201600091509050806000015490806001015490806002015490806003018054600181600116156101000203166002900480601f016020809104026020016040519081016040528092919081815260200182805460018160011615610100020316600290048015610bc45780601f10610b9957610100808354040283529160200191610bc4565b820191906000526020600020905b815481529060010190602001808311610ba757829003601f168201915b505050505090806004015490806005015490806006018054600181600116156101000203166002900480601f016020809104026020016040519081016040528092919081815260200182805460018160011615610100020316600290048015610c6e5780601f10610c4357610100808354040283529160200191610c6e565b820191906000526020600020905b815481529060010190602001808311610c5157829003601f168201915b5050505060078301805460408051602060026001851615610100026000190190941693909304601f8101849004840282018401909252818152949594935090830182828015610cfe5780601f10610cd357610100808354040283529160200191610cfe565b820191906000526020600020905b815481529060010190602001808311610ce157829003601f168201915b5050506008909301549192505060ff1689565b60008181526002602081815260408084206004810154600582015460068301805485516001821615610100026000190190911697909704601f810187900487028801870190955284875287966060968796949593949293600701928491830182828015610dbf5780601f10610d9457610100808354040283529160200191610dbf565b820191906000526020600020905b815481529060010190602001808311610da257829003601f168201915b5050845460408051602060026001851615610100026000190190941693909304601f810184900484028201840190925281815295975086945092508401905082828015610e4d5780601f10610e2257610100808354040283529160200191610e4d565b820191906000526020600020905b815481529060010190602001808311610e3057829003601f168201915b5050505050905093509350935093509193509193565b6000818152600560205260408120805481908390610e7d57fe5b9060005260206000200154915050919050565b6000818152600260208181526040808420808401546004820154600583015460038401805486516001821615610100026000190190911698909804601f8101889004880289018801909652858852606097899788978a9788979694959360068201936007909201929091879190830182828015610f4e5780601f10610f2357610100808354040283529160200191610f4e565b820191906000526020600020905b815481529060010190602001808311610f3157829003601f168201915b5050855460408051602060026001851615610100026000190190941693909304601f8101849004840282018401909252818152959a5087945092508401905082828015610fdc5780601f10610fb157610100808354040283529160200191610fdc565b820191906000526020600020905b815481529060010190602001808311610fbf57829003601f168201915b5050845460408051602060026001851615610100026000190190941693909304601f81018490048402820184019092528181529597508694509250840190508282801561106a5780601f1061103f5761010080835404028352916020019161106a565b820191906000526020600020905b81548152906001019060200180831161104d57829003601f168201915b5050505050905095509550955095509550955091939550919395565b6000818152600560209081526040918290208054835181840281018401909452808452606093928301828280156110dc57602002820191906000526020600020905b8154815260200190600101908083116110c8575b50505050509050919050565b6000818152600560205260408120805481906000198101908110610e7d57fe5b6201518082046000908152600460205260409020546060908190801561129157600081815260046020526040812054600019909201919081156111965750600019810182820367ffffffffffffffff8111801561116457600080fd5b5060405190808252806020026020018201604052801561118e578160200160208202803683370190505b5093506111e1565b5060035482810367ffffffffffffffff811180156111b357600080fd5b506040519080825280602002602001820160405280156111dd578160200160208202803683370190505b5093505b6000835b8281101561128c57600381815481106111fa57fe5b9060005260206000209060090201600201548a1480156112385750876003828154811061122357fe5b90600052602060002090600902016004015411155b1561127f576003818154811061124a57fe5b90600052602060002090600902016001015486838060010194508151811061126e57fe5b602002602001018181525050611284565b61128c565b6001016111e5565b505050505b50949350505050565b828054600181600116156101000203166002900490600052602060002090601f016020900481019282601f106112db57805160ff1916838001178555611308565b82800160010185558215611308579182015b828111156113085782518255916020019190600101906112ed565b5061131492915061138d565b5090565b828054600181600116156101000203166002900490600052602060002090601f016020900481019282601f106113515780548555611308565b8280016001018555821561130857600052602060002091601f016020900482015b82811115611308578254825591600101919060010190611372565b6113a791905b808211156113145760008155600101611393565b9056fe5468652063616c6c6572206f662074686973206d6574686f6420646f6573206e6f742068617665207065726d697373696f6e20666f72207468697320616374696f6e2e4576656e74207769746820756e6971756520494420616c7265616479206578697374732ea26469706673582212205234bc0eb3071b69d04f1f70c05b90dcc717d35c63138641beb05ebc0a8d758e64736f6c63430006080033";
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
        [Parameter("string", "hash", 5)]
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
        [Parameter("string", "privateHash", 7)]
        public virtual string PrivateHash { get; set; }
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
