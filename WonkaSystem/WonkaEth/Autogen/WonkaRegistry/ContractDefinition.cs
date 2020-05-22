using System;
using System.Collections.Generic;
using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Wonka.Eth.Autogen.WonkaRegistry
{
    public partial class WonkaRegistryDeployment : WonkaRegistryDeploymentBase
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
          ""internalType"": ""bytes32"",
          ""name"": ""groveId"",
          ""type"": ""bytes32""
        },
        {
          ""internalType"": ""string"",
          ""name"": ""desc"",
          ""type"": ""string""
        },
        {
          ""internalType"": ""address"",
          ""name"": ""groveOwner"",
          ""type"": ""address""
        },
        {
          ""internalType"": ""uint256"",
          ""name"": ""createTime"",
          ""type"": ""uint256""
        }
      ],
      ""name"": ""addRuleGrove"",
      ""outputs"": [],
      ""stateMutability"": ""nonpayable"",
      ""type"": ""function""
    },
    {
      ""inputs"": [
        {
          ""internalType"": ""address"",
          ""name"": ""ruler"",
          ""type"": ""address""
        },
        {
          ""internalType"": ""bytes32"",
          ""name"": ""rsId"",
          ""type"": ""bytes32""
        },
        {
          ""internalType"": ""string"",
          ""name"": ""desc"",
          ""type"": ""string""
        },
        {
          ""internalType"": ""bytes32"",
          ""name"": ""ruleTreeGrpId"",
          ""type"": ""bytes32""
        },
        {
          ""internalType"": ""uint256"",
          ""name"": ""grpIdx"",
          ""type"": ""uint256""
        },
        {
          ""internalType"": ""address"",
          ""name"": ""host"",
          ""type"": ""address""
        },
        {
          ""internalType"": ""uint256"",
          ""name"": ""minCost"",
          ""type"": ""uint256""
        },
        {
          ""internalType"": ""uint256"",
          ""name"": ""maxCost"",
          ""type"": ""uint256""
        },
        {
          ""internalType"": ""address[]"",
          ""name"": ""associates"",
          ""type"": ""address[]""
        },
        {
          ""internalType"": ""bytes32[]"",
          ""name"": ""attributes"",
          ""type"": ""bytes32[]""
        },
        {
          ""internalType"": ""bytes32[]"",
          ""name"": ""ops"",
          ""type"": ""bytes32[]""
        },
        {
          ""internalType"": ""uint256"",
          ""name"": ""createTime"",
          ""type"": ""uint256""
        }
      ],
      ""name"": ""addRuleTreeIndex"",
      ""outputs"": [],
      ""stateMutability"": ""nonpayable"",
      ""type"": ""function""
    },
    {
      ""inputs"": [
        {
          ""internalType"": ""bytes32"",
          ""name"": ""groveId"",
          ""type"": ""bytes32""
        },
        {
          ""internalType"": ""bytes32"",
          ""name"": ""treeId"",
          ""type"": ""bytes32""
        }
      ],
      ""name"": ""addRuleTreeToGrove"",
      ""outputs"": [],
      ""stateMutability"": ""nonpayable"",
      ""type"": ""function""
    },
    {
      ""inputs"": [],
      ""name"": ""getAllRegisteredRuleTrees"",
      ""outputs"": [
        {
          ""internalType"": ""bytes32[]"",
          ""name"": """",
          ""type"": ""bytes32[]""
        }
      ],
      ""stateMutability"": ""view"",
      ""type"": ""function"",
      ""constant"": true
    },
    {
      ""inputs"": [
        {
          ""internalType"": ""bytes32"",
          ""name"": ""groveId"",
          ""type"": ""bytes32""
        }
      ],
      ""name"": ""getRuleGrove"",
      ""outputs"": [
        {
          ""internalType"": ""bytes32"",
          ""name"": ""id"",
          ""type"": ""bytes32""
        },
        {
          ""internalType"": ""string"",
          ""name"": ""desc"",
          ""type"": ""string""
        },
        {
          ""internalType"": ""bytes32[]"",
          ""name"": ""members"",
          ""type"": ""bytes32[]""
        },
        {
          ""internalType"": ""address"",
          ""name"": ""owner"",
          ""type"": ""address""
        },
        {
          ""internalType"": ""uint256"",
          ""name"": ""createTime"",
          ""type"": ""uint256""
        }
      ],
      ""stateMutability"": ""view"",
      ""type"": ""function"",
      ""constant"": true
    },
    {
      ""inputs"": [
        {
          ""internalType"": ""bytes32"",
          ""name"": ""groveId"",
          ""type"": ""bytes32""
        }
      ],
      ""name"": ""getRuleGroveDesc"",
      ""outputs"": [
        {
          ""internalType"": ""string"",
          ""name"": ""desc"",
          ""type"": ""string""
        }
      ],
      ""stateMutability"": ""view"",
      ""type"": ""function"",
      ""constant"": true
    },
    {
      ""inputs"": [
        {
          ""internalType"": ""bytes32"",
          ""name"": ""rsId"",
          ""type"": ""bytes32""
        }
      ],
      ""name"": ""getRuleTreeIndex"",
      ""outputs"": [
        {
          ""internalType"": ""bytes32"",
          ""name"": ""rtid"",
          ""type"": ""bytes32""
        },
        {
          ""internalType"": ""string"",
          ""name"": ""rtdesc"",
          ""type"": ""string""
        },
        {
          ""internalType"": ""address"",
          ""name"": ""hostaddr"",
          ""type"": ""address""
        },
        {
          ""internalType"": ""address"",
          ""name"": ""owner"",
          ""type"": ""address""
        },
        {
          ""internalType"": ""uint256"",
          ""name"": ""maxGasCost"",
          ""type"": ""uint256""
        },
        {
          ""internalType"": ""uint256"",
          ""name"": ""createTime"",
          ""type"": ""uint256""
        },
        {
          ""internalType"": ""bytes32[]"",
          ""name"": ""attributes"",
          ""type"": ""bytes32[]""
        }
      ],
      ""stateMutability"": ""view"",
      ""type"": ""function"",
      ""constant"": true
    },
    {
      ""inputs"": [
        {
          ""internalType"": ""bytes32"",
          ""name"": ""groveId"",
          ""type"": ""bytes32""
        }
      ],
      ""name"": ""getGroveMembers"",
      ""outputs"": [
        {
          ""internalType"": ""bytes32[]"",
          ""name"": """",
          ""type"": ""bytes32[]""
        }
      ],
      ""stateMutability"": ""view"",
      ""type"": ""function"",
      ""constant"": true
    },
    {
      ""inputs"": [
        {
          ""internalType"": ""bytes32"",
          ""name"": ""groveId"",
          ""type"": ""bytes32""
        },
        {
          ""internalType"": ""bytes32"",
          ""name"": ""rsId"",
          ""type"": ""bytes32""
        }
      ],
      ""name"": ""getGroveOrderPosition"",
      ""outputs"": [
        {
          ""internalType"": ""uint256"",
          ""name"": """",
          ""type"": ""uint256""
        }
      ],
      ""stateMutability"": ""view"",
      ""type"": ""function"",
      ""constant"": true
    },
    {
      ""inputs"": [
        {
          ""internalType"": ""bytes32"",
          ""name"": ""rsId"",
          ""type"": ""bytes32""
        }
      ],
      ""name"": ""isRuleTreeRegistered"",
      ""outputs"": [
        {
          ""internalType"": ""bool"",
          ""name"": """",
          ""type"": ""bool""
        }
      ],
      ""stateMutability"": ""view"",
      ""type"": ""function"",
      ""constant"": true
    },
    {
      ""inputs"": [
        {
          ""internalType"": ""bytes32"",
          ""name"": ""groveId"",
          ""type"": ""bytes32""
        },
        {
          ""internalType"": ""bytes32[]"",
          ""name"": ""rsIdList"",
          ""type"": ""bytes32[]""
        },
        {
          ""internalType"": ""uint256[]"",
          ""name"": ""orderList"",
          ""type"": ""uint256[]""
        }
      ],
      ""name"": ""resetGroveOrder"",
      ""outputs"": [],
      ""stateMutability"": ""nonpayable"",
      ""type"": ""function""
    }
  ]";

        public WonkaRegistryDeployment() : base(BYTECODE) { }
        public WonkaRegistryDeployment(string byteCode) : base(byteCode) { }
    }

    public class WonkaRegistryDeploymentBase : ContractDeploymentMessage
    {
        public static string BYTECODE = "0x608060405234801561001057600080fd5b50611a26806100206000396000f3fe608060405234801561001057600080fd5b50600436106100a95760003560e01c8063b440890111610071578063b440890114610505578063c68d1e8c1461050d578063cb0800fb1461062a578063d084e94d146106e3578063e07b435114610714578063fb7464fa14610811576100a9565b806331a832c0146100ae5780634cc30f9b14610316578063596aafec1461034b5780635a51b84a1461036e578063a699068f14610498575b600080fd5b61031460048036036101808110156100c557600080fd5b6001600160a01b0382351691602081013591810190606081016040820135600160201b8111156100f457600080fd5b82018360208201111561010657600080fd5b803590602001918460018302840111600160201b8311171561012757600080fd5b91908080601f01602080910402602001604051908101604052809392919081815260200183838082843760009201919091525092958435956020860135956001600160a01b036040820135169550606081013594506080810135935060c081019060a00135600160201b81111561019d57600080fd5b8201836020820111156101af57600080fd5b803590602001918460208302840111600160201b831117156101d057600080fd5b9190808060200260200160405190810160405280939291908181526020018383602002808284376000920191909152509295949360208101935035915050600160201b81111561021f57600080fd5b82018360208201111561023157600080fd5b803590602001918460208302840111600160201b8311171561025257600080fd5b9190808060200260200160405190810160405280939291908181526020018383602002808284376000920191909152509295949360208101935035915050600160201b8111156102a157600080fd5b8201836020820111156102b357600080fd5b803590602001918460208302840111600160201b831117156102d457600080fd5b91908080602002602001604051908101604052809392919081815260200183836020028082843760009201919091525092955050913592506108a3915050565b005b6103396004803603604081101561032c57600080fd5b5080359060200135610bf9565b60408051918252519081900360200190f35b6103146004803603604081101561036157600080fd5b5080359060200135610c86565b6103146004803603606081101561038457600080fd5b81359190810190604081016020820135600160201b8111156103a557600080fd5b8201836020820111156103b757600080fd5b803590602001918460208302840111600160201b831117156103d857600080fd5b9190808060200260200160405190810160405280939291908181526020018383602002808284376000920191909152509295949360208101935035915050600160201b81111561042757600080fd5b82018360208201111561043957600080fd5b803590602001918460208302840111600160201b8311171561045a57600080fd5b919080806020026020016040519081016040528093929190818152602001838360200280828437600092019190915250929550610df1945050505050565b6104b5600480360360208110156104ae57600080fd5b5035610ff1565b60408051602080825283518183015283519192839290830191858101910280838360005b838110156104f15781810151838201526020016104d9565b505050509050019250505060405180910390f35b6104b5611142565b61052a6004803603602081101561052357600080fd5b503561119b565b6040518088815260200180602001876001600160a01b03166001600160a01b03168152602001866001600160a01b03166001600160a01b0316815260200185815260200184815260200180602001838103835289818151815260200191508051906020019080838360005b838110156105ad578181015183820152602001610595565b50505050905090810190601f1680156105da5780820380516001836020036101000a031916815260200191505b508381038252845181528451602091820191808701910280838360005b8381101561060f5781810151838201526020016105f7565b50505050905001995050505050505050505060405180910390f35b6103146004803603608081101561064057600080fd5b81359190810190604081016020820135600160201b81111561066157600080fd5b82018360208201111561067357600080fd5b803590602001918460018302840111600160201b8311171561069457600080fd5b91908080601f016020809104026020016040519081016040528093929190818152602001838380828437600092019190915250929550506001600160a01b038335169350505060200135611350565b610700600480360360208110156106f957600080fd5b503561151d565b604080519115158252519081900360200190f35b6107316004803603602081101561072a57600080fd5b503561153c565b604051808681526020018060200180602001856001600160a01b03166001600160a01b03168152602001848152602001838103835287818151815260200191508051906020019080838360005b8381101561079657818101518382015260200161077e565b50505050905090810190601f1680156107c35780820380516001836020036101000a031916815260200191505b508381038252865181528651602091820191808901910280838360005b838110156107f85781810151838201526020016107e0565b5050505090500197505050505050505060405180910390f35b61082e6004803603602081101561082757600080fd5b50356116c3565b6040805160208082528351818301528351919283929083019185019080838360005b83811015610868578181015183820152602001610850565b50505050905090810190601f1680156108955780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b8a6108df5760405162461bcd60e51b815260040180806020018281038252602681526020018061195e6026913960400191505060405180910390fd5b60008b8152600260205260409020600b015460ff1615156001141561094b576040805162461bcd60e51b815260206004820152601f60248201527f52756c655472656520666f7220494420616c7265616479206578697374732e00604482015290519081900360640190fd5b60408051610180810182528c815260208082018d9052825160008082529181018452919283019190508152602001886001600160a01b031681526020018d6001600160a01b0316815260200187815260200186815260200185815260200184815260200183815260200182815260200160011515815250600260008d81526020019081526020016000206000820151816000015560208201518160010190805190602001906109fb9291906117c1565b5060408201518051610a1791600284019160209091019061183f565b5060608201516003820180546001600160a01b039283166001600160a01b031991821617909155608084015160048401805491909316911617905560a0820151600582015560c0820151600682015560e08201518051610a81916007840191602090910190611879565b506101008201518051610a9e91600884019160209091019061183f565b506101208201518051610abb91600984019160209091019061183f565b50610140820151600a82015561016090910151600b909101805460ff1916911515919091179055600380546001810182556000919091527fc2575a0e9e593c00f959f8c92f12db2869c3395a3b0502d05e2516446f71f85b018b90558815610beb5760008981526020819052604090206006015460ff161515600114610b6657610b668960405180604001604052806007815260200166111959985d5b1d60ca1b8152508e84611350565b60008b81526002602081815260408084208301805460018181018355918652838620018e90558d8552848352908420909201805492830181558352909120018b90558715801590610bc857506000898152602081905260409020600201548811155b15610beb576000898152602081815260408083208e845260030190915290208890555b505050505050505050505050565b60008281526020819052604081206006015460ff161515600114610c52576040805162461bcd60e51b815260206004820152601d602482015260008051602061193e833981519152604482015290519081900360640190fd5b620f423f8315610c7f578215610c7f57506000838152602081815260408083208584526003019091529020545b9392505050565b60008281526020819052604090206006015460ff161515600114610cf1576040805162461bcd60e51b815260206004820152601c60248201527f47726f766520666f7220494420646f6573206e6f742065786973742e00000000604482015290519081900360640190fd5b6000818152600260205260409020600b015460ff161515600114610d5c576040805162461bcd60e51b815260206004820152601f60248201527f52756c655472656520666f7220494420646f6573206e6f742065786973742e00604482015290519081900360640190fd5b60008281526020818152604080832084845260030190915290205415610db35760405162461bcd60e51b81526004018080602001828103825260258152602001806119196025913960400191505060405180910390fd5b600091825260208281526040808420600281018054600181810183558288528588209091018690559054948652600390910190925290922091019055565b6000825111610e47576040805162461bcd60e51b815260206004820181905260248201527f50726f76696465642052756c6554726565206c69737420697320656d7074792e604482015290519081900360640190fd5b6000815111610e9d576040805162461bcd60e51b815260206004820152601d60248201527f50726f766964656420696e646578206c69737420697320656d7074792e000000604482015290519081900360640190fd5b8051825114610edd5760405162461bcd60e51b81526004018080602001828103825260338152602001806119846033913960400191505060405180910390fd5b815160008481526020819052604090206002015414610f2d5760405162461bcd60e51b815260040180806020018281038252603a8152602001806119b7603a913960400191505060405180910390fd5b600080805b8451831015610fe957848381518110610f4757fe5b60200260200101519050806000808881526020019081526020016000206002018481548110610f7257fe5b60009182526020808320909101929092558781528082526040808220848352600301909252205491508115801590610fbb57506000868152602081905260409020600201548211155b15610fde5760008681526020818152604080832084845260030190915290208290555b826001019250610f32565b505050505050565b60008181526020819052604090206006015460609060ff16151560011461104d576040805162461bcd60e51b815260206004820152601d602482015260008051602061193e833981519152604482015290519081900360640190fd5b600082815260208181526040808320600201548151818152818402810190930190915260609190801561108a578160200160208202803883390190505b50905060005b60008581526020819052604090206002015481101561113a5760008581526020819052604081206002018054839081106110c657fe5b6000918252602080832090910154888352828252604080842082855260030190925291205494509050831580159061110f57506000868152602081905260409020600201548411155b15611131578083600186038151811061112457fe5b6020026020010181815250505b50600101611090565b509392505050565b6060600380548060200260200160405190810160405280929190818152602001828054801561119057602002820191906000526020600020905b81548152602001906001019080831161117c575b505050505090505b90565b6000818152600260205260408120600b01546060908290819081908190859060ff161515600114611213576040805162461bcd60e51b815260206004820181905260248201527f52756c6554726565207769746820494420646f6573206e6f742065786973742e604482015290519081900360640190fd5b6000888152600260208181526040928390208054600382015460048301546006840154600a850154600180870180548b516101009382161593909302600019011699909904601f8101899004890282018901909a5289815294986001600160a01b039485169794909316959194909360089092019290918891908301828280156112de5780601f106112b3576101008083540402835291602001916112de565b820191906000526020600020905b8154815290600101906020018083116112c157829003601f168201915b505050505095508080548060200260200160405190810160405280929190818152602001828054801561133057602002820191906000526020600020905b81548152602001906001019080831161131c575b505050505090509650965096509650965096509650919395979092949650565b836113a2576040805162461bcd60e51b815260206004820181905260248201527f426c616e6b2047726f7665494420686173206265656e2070726f76696465642e604482015290519081900360640190fd5b60008481526020819052604090206006015460ff1615156001141561140e576040805162461bcd60e51b815260206004820152601d60248201527f47726f7665207769746820494420616c7265616479206578697374732e000000604482015290519081900360640190fd5b6040805160c0810182528581526020808201868152835160008082528184018652848601919091526001600160a01b038716606085015260808401869052600160a0850181905289825281845294902083518155905180519394919361147c939285019291909101906117c1565b506040820151805161149891600284019160209091019061183f565b5060608201516004820180546001600160a01b0319166001600160a01b039092169190911790556080820151600582015560a0909101516006909101805460ff191691151591909117905550506001805480820182556000919091527fb10e2d527612073b26eecdfd717e6a320cf44b4afac2b0732d9fcbe2b7fa0cf6019190915550565b6000818152600260205260409020600b015460ff161515600114919050565b60008181526020819052604081206006015460609081908390819060ff16151560011461159e576040805162461bcd60e51b815260206004820152601d602482015260008051602061193e833981519152604482015290519081900360640190fd5b600086815260208181526040918290208054600482015460058301546001808501805488516002610100948316159490940260001901909116839004601f81018990048902820189019099528881529497909691909501946001600160a01b0390931693919286918301828280156116575780601f1061162c57610100808354040283529160200191611657565b820191906000526020600020905b81548152906001019060200180831161163a57829003601f168201915b50505050509350828054806020026020016040519081016040528092919081815260200182805480156116a957602002820191906000526020600020905b815481526020019060010190808311611695575b505050505092509450945094509450945091939590929450565b60008181526020819052604090206006015460609060ff16151560011461171f576040805162461bcd60e51b815260206004820152601d602482015260008051602061193e833981519152604482015290519081900360640190fd5b60008281526020818152604091829020600190810180548451600293821615610100026000190190911692909204601f8101849004840283018401909452838252909290918301828280156117b55780601f1061178a576101008083540402835291602001916117b5565b820191906000526020600020905b81548152906001019060200180831161179857829003601f168201915b50505050509050919050565b828054600181600116156101000203166002900490600052602060002090601f016020900481019282601f1061180257805160ff191683800117855561182f565b8280016001018555821561182f579182015b8281111561182f578251825591602001919060010190611814565b5061183b9291506118da565b5090565b82805482825590600052602060002090810192821561182f579160200282018281111561182f578251825591602001919060010190611814565b8280548282559060005260206000209081019282156118ce579160200282015b828111156118ce57825182546001600160a01b0319166001600160a01b03909116178255602090920191600190910190611899565b5061183b9291506118f4565b61119891905b8082111561183b57600081556001016118e0565b61119891905b8082111561183b5780546001600160a01b03191681556001016118fa56fe52756c655472656520616c7265616479206578697374732077697468696e2047726f76652e47726f7665207769746820494420646f6573206e6f742065786973742e000000426c616e6b20494420666f722052756c6553657420686173206265656e2070726f766964656452756c6554726565206c69737420616e6420696e646578206c6973742061726520646966666572656e74206c656e677468732e47726f7665206d656d626572206c69737420616e642052756c6554726565206c6973742061726520646966666572656e74206c656e677468732ea2646970667358221220ecb281686d3e55fa067e4b678091ea57e8a447231a3181d51b9ddbe64d59215164736f6c63430006000033";
        public WonkaRegistryDeploymentBase() : base(BYTECODE) { }
        public WonkaRegistryDeploymentBase(string byteCode) : base(byteCode) { }

    }

    public partial class AddRuleGroveFunction : AddRuleGroveFunctionBase { }

    [Function("addRuleGrove")]
    public class AddRuleGroveFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "groveId", 1)]
        public virtual byte[] GroveId { get; set; }
        [Parameter("string", "desc", 2)]
        public virtual string Desc { get; set; }
        [Parameter("address", "groveOwner", 3)]
        public virtual string GroveOwner { get; set; }
        [Parameter("uint256", "createTime", 4)]
        public virtual BigInteger CreateTime { get; set; }
    }

    public partial class AddRuleTreeIndexFunction : AddRuleTreeIndexFunctionBase { }

    [Function("addRuleTreeIndex")]
    public class AddRuleTreeIndexFunctionBase : FunctionMessage
    {
        [Parameter("address", "ruler", 1)]
        public virtual string Ruler { get; set; }
        [Parameter("bytes32", "rsId", 2)]
        public virtual byte[] RsId { get; set; }
        [Parameter("string", "desc", 3)]
        public virtual string Desc { get; set; }
        [Parameter("bytes32", "ruleTreeGrpId", 4)]
        public virtual byte[] RuleTreeGrpId { get; set; }
        [Parameter("uint256", "grpIdx", 5)]
        public virtual BigInteger GrpIdx { get; set; }
        [Parameter("address", "host", 6)]
        public virtual string Host { get; set; }
        [Parameter("uint256", "minCost", 7)]
        public virtual BigInteger MinCost { get; set; }
        [Parameter("uint256", "maxCost", 8)]
        public virtual BigInteger MaxCost { get; set; }
        [Parameter("address[]", "associates", 9)]
        public virtual List<string> Associates { get; set; }
        [Parameter("bytes32[]", "attributes", 10)]
        public virtual List<byte[]> Attributes { get; set; }
        [Parameter("bytes32[]", "ops", 11)]
        public virtual List<byte[]> Ops { get; set; }
        [Parameter("uint256", "createTime", 12)]
        public virtual BigInteger CreateTime { get; set; }
    }

    public partial class AddRuleTreeToGroveFunction : AddRuleTreeToGroveFunctionBase { }

    [Function("addRuleTreeToGrove")]
    public class AddRuleTreeToGroveFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "groveId", 1)]
        public virtual byte[] GroveId { get; set; }
        [Parameter("bytes32", "treeId", 2)]
        public virtual byte[] TreeId { get; set; }
    }

    public partial class GetAllRegisteredRuleTreesFunction : GetAllRegisteredRuleTreesFunctionBase { }

    [Function("getAllRegisteredRuleTrees", "bytes32[]")]
    public class GetAllRegisteredRuleTreesFunctionBase : FunctionMessage
    {

    }

    public partial class GetRuleGroveFunction : GetRuleGroveFunctionBase { }

    [Function("getRuleGrove", typeof(GetRuleGroveOutputDTO))]
    public class GetRuleGroveFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "groveId", 1)]
        public virtual byte[] GroveId { get; set; }
    }

    public partial class GetRuleGroveDescFunction : GetRuleGroveDescFunctionBase { }

    [Function("getRuleGroveDesc", "string")]
    public class GetRuleGroveDescFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "groveId", 1)]
        public virtual byte[] GroveId { get; set; }
    }

    public partial class GetRuleTreeIndexFunction : GetRuleTreeIndexFunctionBase { }

    [Function("getRuleTreeIndex", typeof(GetRuleTreeIndexOutputDTO))]
    public class GetRuleTreeIndexFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "rsId", 1)]
        public virtual byte[] RsId { get; set; }
    }

    public partial class GetGroveMembersFunction : GetGroveMembersFunctionBase { }

    [Function("getGroveMembers", "bytes32[]")]
    public class GetGroveMembersFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "groveId", 1)]
        public virtual byte[] GroveId { get; set; }
    }

    public partial class GetGroveOrderPositionFunction : GetGroveOrderPositionFunctionBase { }

    [Function("getGroveOrderPosition", "uint256")]
    public class GetGroveOrderPositionFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "groveId", 1)]
        public virtual byte[] GroveId { get; set; }
        [Parameter("bytes32", "rsId", 2)]
        public virtual byte[] RsId { get; set; }
    }

    public partial class IsRuleTreeRegisteredFunction : IsRuleTreeRegisteredFunctionBase { }

    [Function("isRuleTreeRegistered", "bool")]
    public class IsRuleTreeRegisteredFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "rsId", 1)]
        public virtual byte[] RsId { get; set; }
    }

    public partial class ResetGroveOrderFunction : ResetGroveOrderFunctionBase { }

    [Function("resetGroveOrder")]
    public class ResetGroveOrderFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "groveId", 1)]
        public virtual byte[] GroveId { get; set; }
        [Parameter("bytes32[]", "rsIdList", 2)]
        public virtual List<byte[]> RsIdList { get; set; }
        [Parameter("uint256[]", "orderList", 3)]
        public virtual List<BigInteger> OrderList { get; set; }
    }

    public partial class GetAllRegisteredRuleTreesOutputDTO : GetAllRegisteredRuleTreesOutputDTOBase { }

    [FunctionOutput]
    public class GetAllRegisteredRuleTreesOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("bytes32[]", "", 1)]
        public virtual List<byte[]> ReturnValue1 { get; set; }
    }

    public partial class GetRuleGroveOutputDTO : GetRuleGroveOutputDTOBase { }

    [FunctionOutput]
    public class GetRuleGroveOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("bytes32", "id", 1)]
        public virtual byte[] Id { get; set; }
        [Parameter("string", "desc", 2)]
        public virtual string Desc { get; set; }
        [Parameter("bytes32[]", "members", 3)]
        public virtual List<byte[]> Members { get; set; }
        [Parameter("address", "owner", 4)]
        public virtual string Owner { get; set; }
        [Parameter("uint256", "createTime", 5)]
        public virtual BigInteger CreateTime { get; set; }
    }

    public partial class GetRuleGroveDescOutputDTO : GetRuleGroveDescOutputDTOBase { }

    [FunctionOutput]
    public class GetRuleGroveDescOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("string", "desc", 1)]
        public virtual string Desc { get; set; }
    }

    public partial class GetRuleTreeIndexOutputDTO : GetRuleTreeIndexOutputDTOBase { }

    [FunctionOutput]
    public class GetRuleTreeIndexOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("bytes32", "rtid", 1)]
        public virtual byte[] Rtid { get; set; }
        [Parameter("string", "rtdesc", 2)]
        public virtual string Rtdesc { get; set; }
        [Parameter("address", "hostaddr", 3)]
        public virtual string Hostaddr { get; set; }
        [Parameter("address", "owner", 4)]
        public virtual string Owner { get; set; }
        [Parameter("uint256", "maxGasCost", 5)]
        public virtual BigInteger MaxGasCost { get; set; }
        [Parameter("uint256", "createTime", 6)]
        public virtual BigInteger CreateTime { get; set; }
        [Parameter("bytes32[]", "attributes", 7)]
        public virtual List<byte[]> Attributes { get; set; }
    }

    public partial class GetGroveMembersOutputDTO : GetGroveMembersOutputDTOBase { }

    [FunctionOutput]
    public class GetGroveMembersOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("bytes32[]", "", 1)]
        public virtual List<byte[]> ReturnValue1 { get; set; }
    }

    public partial class GetGroveOrderPositionOutputDTO : GetGroveOrderPositionOutputDTOBase { }

    [FunctionOutput]
    public class GetGroveOrderPositionOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class IsRuleTreeRegisteredOutputDTO : IsRuleTreeRegisteredOutputDTOBase { }

    [FunctionOutput]
    public class IsRuleTreeRegisteredOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("bool", "", 1)]
        public virtual bool ReturnValue1 { get; set; }
    }
}
