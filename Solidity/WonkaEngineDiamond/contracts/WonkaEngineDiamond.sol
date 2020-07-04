// SPDX-License-Identifier: MIT
pragma solidity ^0.6.8;

import "./DiamondStorageContract.sol";
import "./DiamondHeaders.sol";
import "./DiamondFacet.sol";
import "./DiamondLoupeFacet.sol";

import "./TransactionStateInterface.sol";
import "./WonkaEngineSupportFacet.sol";

/// @title An Ethereum library that contains the functionality for a rules engine
/// @author Aaron Kendall
/// @notice 1.) Certain steps are required in order to use this engine correctly + 2.) Deployment of this contract to a blockchain is expensive (~8000000 gas) + 3.) Various require() statements are commented out to save deployment costs
/// @dev Even though you can create rule trees by calling this contract directly, it is generally recommended that you create them using the Nethereum library
contract WonkaEngineDiamond is DiamondStorageContract {

    event OwnershipTransferred(address indexed previousOwner, address indexed newOwner);

    /// @title Holds metadata which represents an Attribute (i.e., a unique point of data in a user's record)
    /// @author Aaron Kendall
    /// @notice Not all struct members are currently used
    struct WonkaAttr {
 
        uint attrId;

        bytes32 attrName;

        uint maxLength;

        bool maxLengthTruncate;

        uint maxNumValue;

        string defaultValue;

        bool isString;

        bool isDecimal;
        
        bool isNumeric;

        bool isValue;
    }

    /// @title A data structure that represents a Source (i.e., a provider of a record)
    /// @author Aaron Kendall
    /// @notice This structure isn't currently used
    struct WonkaSource {

        bytes32 sourceName;

        bytes32 status;

        // For retrieving an Attribute value
        bytes32 methodName;

        // For setting an Attribute value
        bytes32 setMethodName;
        
        address contractAddress;

        bool isValue;
    }

    /// @title Defines a rule (i.e., a logical unit for testing the validity of an Attribute value in a record)
    /// @author Aaron Kendall
    /// @notice 1.) Only one Attribute can be targeted now, but in the future, rules could be able to target multiple Attributes + 2.) A Rule can only be owned by one RuleSet
    /// @dev 
    struct WonkaRule {

        uint ruleId;

        bytes32 name;

        uint ruleType;

        WonkaAttr targetAttr;

        string ruleValue;

        mapping(string => string) ruleValueDomain;

        string[] ruleDomainKeys;

        bytes32[] customOpArgs;

        bytes32 parentRuleSetId;

        bool notOpFlag;

        bool isPassiveFlag;
    }

    /// @title Contains a list of all rules that have failed during a validation (and the rulesets to which they belong)
    /// @author Aaron Kendall
    /// @notice 
    /// @dev The arrays will be set to the total number of rules in a RuleTree, but 'ruleFailCount' will indicate how many of them are actually populated
    struct WonkaRuleReport {

        uint ruleFailCount;

        bytes32[] ruleSetIds;

        bytes32[] ruleIds;
    }

    /// @title Defines a ruleset (i.e., a logical grouping of rules)
    /// @author Aaron Kendall
    /// @notice A RuleSet can only be owned by one RuleTree
    /// @dev The collective evaluation of its rules will make a determination (such as how to navigate the RuleTree or whether or not the provided record is valid)  
    struct WonkaRuleSet {

        bytes32     ruleSetId;

        string      description;
        bytes32     parentRuleSetId;
        bool        severeFailure;
        // string      customFailureMsg;

        uint[]                           evalRuleList;
        mapping(uint => WonkaRule)       evaluativeRules;

        uint[]                           assertiveRuleList;
        mapping(uint => WonkaRule)       assertiveRules;

        bytes32[]                        childRuleSetList;

        bool        andOp;
        bool        failImmediately;
        bool        isLeaf;
        bool        isValue;
    }

    /// @title Defines a ruletree (i.e., a logical, hierarchical grouping of rulesets)
    /// @author Aaron Kendall
    /// @notice Currently, only one ruletree can be defined for any given address/account
    /// @dev The collective evaluation of its rulesets will determine whether or not the provided record is valid  
    struct WonkaRuleTree {

        bytes32     ruleTreeId;
        string      description;

        bytes32 rootRuleSetName;

        bytes32[]                        allRuleSetList;
        mapping(bytes32 => WonkaRuleSet) allRuleSets;

        uint totalRuleCount;

        bool isValue;
    }

    /// @dev Defines an event that will report when a ruletree has been invoked to validate a provided record.
    /// @author Aaron Kendall
    /// @notice 
    event CallRuleTree(
        address indexed ruler
    );

    /// @dev Defines an event that will report when a ruleset has been invoked when validating a provided record.
    /// @author Aaron Kendall
    /// @notice 
    event CallRuleSet(
        address indexed ruler,
        bytes32 indexed tmpRuleSetId
    );

    /// @dev Defines an event that will report when a rule has been invoked when validating a provided record.
    /// @author Aaron Kendall
    /// @notice 
    event CallRule(
        address indexed ruler,
        bytes32 indexed ruleSetId,
        bytes32 indexed ruleId,
        uint ruleType
    );
	
    /// @dev Defines an event that will report when the record does not satisfy a ruleset.
    /// @author Aaron Kendall
    event RuleSetError (
        address indexed ruler,
        bytes32 indexed ruleSetId,
        bool severeFailure
    );	

    // An enum for the type of rules currently supported
    enum RuleTypes { IsEqual, IsLessThan, IsGreaterThan, Populated, InDomain, Assign, OpAdd, OpSub, OpMult, OpDiv, CustomOp, MAX_TYPE }
    RuleTypes constant defaultType = RuleTypes.IsEqual;

    string constant blankValue = "";

    uint constant CONST_CUSTOM_OP_ARGS = 4;

    address public rulesMaster;
    uint    public attrCounter;
    uint    public ruleCounter;
    uint    public lastRuleId;

    address         lastSenderAddressProvided;
    bool            lastTransactionSuccess;
    WonkaRuleReport lastRuleReport;

    bool    orchestrationMode;
    bytes32 defaultTargetSource;

    // The Attributes known by this instance of the rules engine
    mapping(bytes32 => WonkaAttr) private attrMap;    
    WonkaAttr[] public attributes;

    // The cache of rule trees that are owned by owner 
    mapping(address => WonkaRuleTree) private ruletrees;

    // The cache of all created rulesets
    WonkaRuleSet[] public rulesets;

    // The cache of records that are owned by "rulers" and that are validated when invoking a rule tree
    mapping(address => mapping(bytes32 => string)) currentRecords;

    // The cache of available sources for retrieving and setting attribute values found on other contracts
    mapping(bytes32 => WonkaSource) sourceMap;

    // The cache of available sources for calling 'op' methods (i.e., that contain special logic to implement a custom operator)
    mapping(bytes32 => WonkaSource) opMap;

    // The cache that indicates if a transaction state exist for a RuleTree
    mapping(bytes32 => bool) transStateInd;

    // The cache of transaction states assigned to RuleTrees
    mapping(bytes32 => TransactionStateInterface) transStateMap;

    // For the function splitStr(...)
    // Currently unsure how the function will perform in a multithreaded scenario
    bytes splitTempStr; // temporarily holds the string part until a space is received

    /// @dev Constructor for the rules engine
    /// @author Aaron Kendall
    /// @notice Currently, the engine will create three dummy Attributes within the cache by default, but they will be removed later
    constructor() public {

        orchestrationMode = false;
        lastTransactionSuccess = false;

        rulesMaster = msg.sender;
        ruleCounter = lastRuleId = 1;

        attributes.push(WonkaAttr({
            attrId: 1,
            attrName: "Title",
            maxLength: 256,
            maxLengthTruncate: true,
            maxNumValue: 0,
            defaultValue: "Blank",
            isString: true,
            isDecimal: false,
            isNumeric: false,
            isValue: true                
        }));

        attrMap[attributes[attributes.length-1].attrName] = attributes[attributes.length-1];

        attributes.push(WonkaAttr({
            attrId: 2,
            attrName: "Price",
            maxLength: 128,
            maxLengthTruncate: false,
            maxNumValue: 1000000,
            defaultValue: "000",
            isString: false,
            isDecimal: false,
            isNumeric: true,
            isValue: true               
        }));

        attrMap[attributes[attributes.length-1].attrName] = attributes[attributes.length-1];
        
        attributes.push(WonkaAttr({
            attrId: 3,
            attrName: "PageAmount",
            maxLength: 256,
            maxLengthTruncate: false,
            maxNumValue: 1000,
            defaultValue: "",
            isString: false,
            isDecimal: false,
            isNumeric: true,
            isValue: true              
        }));

        attrMap[attributes[attributes.length-1].attrName] = attributes[attributes.length-1];

        attrCounter = 4;

        DiamondStorage storage ds = diamondStorage();
        ds.contractOwner = msg.sender; 

        emit OwnershipTransferred(address(0), msg.sender);

        // Create a DiamondFacet contract which implements the Diamond interface
        DiamondFacet diamondFacet = new DiamondFacet();

        // Create a DiamondLoupeFacet contract which implements the Diamond Loupe interface
        DiamondLoupeFacet diamondLoupeFacet = new DiamondLoupeFacet(); 

        // Create a WonkaEngineSupportFacet contract
        WonkaEngineSupportFacet diamondSupportFacet = new WonkaEngineSupportFacet(); 

        bytes[] memory diamondCut = new bytes[](3);

        // Adding cut function
        diamondCut[0] = abi.encodePacked(diamondFacet, Diamond.diamondCut.selector);

        // Adding diamond loupe functions                
        diamondCut[1] = abi.encodePacked(
            diamondLoupeFacet,
            DiamondLoupe.facetFunctionSelectors.selector,
            DiamondLoupe.facets.selector,
            DiamondLoupe.facetAddress.selector,
            DiamondLoupe.facetAddresses.selector            
        );    

        diamondCut[2] = abi.encodePacked(
            diamondSupportFacet,
            Diamond.diamondCut.selector,
            WonkaEngineSupportFacet.bytes32ToString.selector,
            WonkaEngineSupportFacet.invokeCustomOperator.selector,
            WonkaEngineSupportFacet.invokeValueRetrieval.selector,
            WonkaEngineSupportFacet.invokeValueSetter.selector,
            WonkaEngineSupportFacet.parseInt.selector,
            WonkaEngineSupportFacet.strConcat.selector,
            WonkaEngineSupportFacet.stringToBytes32.selector,
            WonkaEngineSupportFacet.uintToBytes.selector                       
        );

        /**
         ** NOTE: Replace this section with adding WonkaEngine functions
         **
        // Adding supportsInterface function
        diamondCut[2] = abi.encodePacked(address(this), ERC165.supportsInterface.selector);

        // execute cut function
        bytes memory cutFunction = abi.encodeWithSelector(Diamond.diamondCut.selector, diamondCut);
        (bool success,) = address(diamondFacet).delegatecall(cutFunction);
        require(success, "Adding functions failed.");        

        // adding ERC165 data
        ds.supportedInterfaces[ERC165.supportsInterface.selector] = true;
        ds.supportedInterfaces[Diamond.diamondCut.selector] = true;
        bytes4 interfaceID = DiamondLoupe.facets.selector ^ DiamondLoupe.facetFunctionSelectors.selector ^ DiamondLoupe.facetAddresses.selector ^ DiamondLoupe.facetAddress.selector;
        ds.supportedInterfaces[interfaceID] = true;
         **/
    }

}