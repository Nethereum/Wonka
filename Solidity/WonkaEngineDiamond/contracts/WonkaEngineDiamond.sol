// SPDX-License-Identifier: MIT
pragma solidity ^0.6.8;

import "./DiamondStorageContract.sol";
import "./DiamondHeaders.sol";
import "./DiamondFacet.sol";
import "./DiamondLoupeFacet.sol";

import "./TransactionStateInterface.sol";
import "./WonkaEngineStructs.sol";
import "./WonkaEngineMainFacet.sol";
import "./WonkaEngineSupportFacet.sol";

/// @title An Ethereum library that contains the functionality for a rules engine
/// @author Aaron Kendall
/// @notice 1.) Certain steps are required in order to use this engine correctly + 2.) Deployment of this contract to a blockchain is expensive (~8000000 gas) + 3.) Various require() statements are commented out to save deployment costs
/// @dev Even though you can create rule trees by calling this contract directly, it is generally recommended that you create them using the Nethereum library
contract WonkaEngineDiamond is DiamondStorageContract {

    event OwnershipTransferred(address indexed previousOwner, address indexed newOwner);

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

    WonkaEngineStructs.WonkaRuleReport lastRuleReport;

    bool    orchestrationMode;
    bytes32 defaultTargetSource;

    // The Attributes known by this instance of the rules engine
    mapping(bytes32 => WonkaEngineStructs.WonkaAttr) public attrMap;    
    WonkaEngineStructs.WonkaAttr[] public attributes;

    // The cache of rule trees that are owned by owner 
    mapping(address => WonkaEngineStructs.WonkaRuleTree) public ruletrees;

    // The cache of all created rulesets
    WonkaEngineStructs.WonkaRuleSet[] public rulesets;

    // The cache of records that are owned by "rulers" and that are validated when invoking a rule tree
    mapping(address => mapping(bytes32 => string)) public currentRecords;

    // The cache of available sources for retrieving and setting attribute values found on other contracts
    mapping(bytes32 => WonkaEngineStructs.WonkaSource) public sourceMap;

    // The cache of available sources for calling 'op' methods (i.e., that contain special logic to implement a custom operator)
    mapping(bytes32 => WonkaEngineStructs.WonkaSource) public opMap;

    // The cache that indicates if a transaction state exist for a RuleTree
    mapping(bytes32 => bool) public transStateInd;

    // The cache of transaction states assigned to RuleTrees
    mapping(bytes32 => TransactionStateInterface) public transStateMap;

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

        attributes.push(WonkaEngineStructs.WonkaAttr({
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

        attributes.push(WonkaEngineStructs.WonkaAttr({
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
        
        attributes.push(WonkaEngineStructs.WonkaAttr({
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
        WonkaEngineMainFacet diamondMainFacet = new WonkaEngineMainFacet(); 
        diamondMainFacet.setDiamondAddress(address(this), rulesMaster);

        // Create a WonkaEngineSupportFacet contract
        WonkaEngineSupportFacet diamondSupportFacet = new WonkaEngineSupportFacet(); 

        bytes[] memory diamondCut = new bytes[](4);

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

        diamondCut[3] = abi.encodePacked(
            diamondMainFacet,
            Diamond.diamondCut.selector)
        ;

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

    modifier onlyEngineOwner()
    {
        require(msg.sender == rulesMaster, "The caller of this method does not have permission for this action.");

        // Do not forget the "_;"! It will
        // be replaced by the actual function
        // body when the modifier is used.
        _;
    }

    modifier onlyEngineOwnerOrTreeOwner(address _RTOwner) {

        require((msg.sender == rulesMaster) || (msg.sender == _RTOwner), "The caller of this method does not have permission for this action.");

        require(hasRuleTree(_RTOwner) == true, "The specified RuleTree does not exist.");

        // Do not forget the "_;"! It will
        // be replaced by the actual function
        // body when the modifier is used.
        _;
    }

    /// @dev This method will add a new Attribute to the cache.  By adding Attributes, we expand the set of possible values that can be held by a record.
    /// @author Aaron Kendall
    /// @notice 
    function addAttribute(bytes32 pAttrName, uint pMaxLen, uint pMaxNumVal, string memory pDefVal, bool pIsStr, bool pIsNum) public onlyEngineOwner {

        bool maxLenTrun = (pMaxLen > 0);

        attributes.push(WonkaEngineStructs.WonkaAttr({
            attrId: attrCounter++,
            attrName: pAttrName,
            maxLength: pMaxLen,
            maxLengthTruncate: maxLenTrun,
            maxNumValue: pMaxNumVal,
            defaultValue: pDefVal,
            isString: pIsStr,
            isDecimal: false,
            isNumeric: pIsNum,
            isValue: true              
        }));

        attrMap[attributes[attributes.length-1].attrName] = attributes[attributes.length-1];
    }   

    /// @dev This method will add a new Attribute to the cache.  Using flagFailImmediately is not recommended and will likely be deprecated in the near future.
    /// @author Aaron Kendall
    /// @notice Currently, only one ruletree can be defined for any given address/account
    function addRuleTree(address ruler, bytes32 rsName, string memory desc, bool severeFailureFlag, bool useAndOperator, bool flagFailImmediately) public onlyEngineOwner {

        require(ruletrees[ruler].isValue != true, "A RuleTree with this ID already exists.");

        ruletrees[ruler] = WonkaEngineStructs.WonkaRuleTree({
            ruleTreeId: rsName,
            description: desc,
            rootRuleSetName: rsName,
            allRuleSetList: new bytes32[](0),
            totalRuleCount: 0,
            isValue: true
        });

        addRuleSet(ruler, rsName, desc, "", severeFailureFlag, useAndOperator, flagFailImmediately);

        transStateInd[ruletrees[ruler].ruleTreeId] = false;
    }

    /// @dev This method will add a new custom operator to the cache.
    /// @author Aaron Kendall
    /// @notice 
    function addCustomOp(bytes32 srcName, bytes32 sts, address cntrtAddr, bytes32 methName) public onlyEngineOwner {

        opMap[srcName] = 
            WonkaEngineStructs.WonkaSource({
                sourceName: srcName,
                status: sts,
                contractAddress: cntrtAddr,
                methodName: methName,
                setMethodName: "",
                isValue: true
        });
    }

    /// @dev This method will add a new RuleSet to the cache and to the indicated RuleTree.  Using flagFailImmediately is not recommended and will likely be deprecated in the near future.
    /// @author Aaron Kendall
    /// @notice Currently, a RuleSet can only belong to one RuleTree and be a child of one parent RuleSet, though there are plans to have a RuleSet capable of being shared among parents
    function addRuleSet(address ruler, bytes32 ruleSetName, string memory desc, bytes32 parentRSName, bool severeFailureFlag, bool useAndOperator, bool flagFailImmediately) public onlyEngineOwnerOrTreeOwner(ruler) {

        if (parentRSName != "") {
            require(ruletrees[ruler].allRuleSets[parentRSName].isValue == true, "The specified parent RuleSet does not exist.");
        }

        // NOTE: Unnecessary and commented out in order to save deployment costs (in terms of gas)
        // require(ruletrees[ruler].allRuleSets[ruleSetName].isValue == false, "The specified RuleSet with the provided ID already exists.");

        ruletrees[ruler].allRuleSetList.push(ruleSetName);

        ruletrees[ruler].allRuleSets[ruleSetName] = 
            WonkaEngineStructs.WonkaRuleSet({
                ruleSetId: ruleSetName,
                description: desc,
                parentRuleSetId: parentRSName,
                severeFailure: severeFailureFlag,
                andOp: useAndOperator,
                failImmediately: flagFailImmediately,
                evalRuleList: new uint[](0),
                assertiveRuleList: new uint[](0),
                childRuleSetList: new bytes32[](0),
                isLeaf: true,
                isValue: true                
            });

        if (parentRSName != "") {
            ruletrees[ruler].allRuleSets[parentRSName].childRuleSetList.push(ruleSetName);
            ruletrees[ruler].allRuleSets[parentRSName].isLeaf = false;
        }
    }

    /// @dev This method will add a new Rule to the indicated RuleSet
    /// @author Aaron Kendall
    /// @notice Currently, a Rule can only belong to one RuleSet
    function addRule(address ruler, bytes32 ruleSetId, bytes32 ruleName, bytes32 attrName, uint rType, string memory rVal, bool notFlag, bool passiveFlag) public onlyEngineOwnerOrTreeOwner(ruler) {

        require(ruletrees[ruler].allRuleSets[ruleSetId].isValue == true, "The specified RuleSet does not exist.");

        require(attrMap[attrName].isValue, "The specified Attribute of the Rule does not exist.");

        require(rType < uint(RuleTypes.MAX_TYPE), "The specified type of the Rule does not exist.");

        uint currRuleId = lastRuleId = ruleCounter;

        ruleCounter = ruleCounter + 1;

        ruletrees[ruler].totalRuleCount += 1; 

        if (passiveFlag) {
            ruletrees[ruler].allRuleSets[ruleSetId].evalRuleList.push(currRuleId);

            ruletrees[ruler].allRuleSets[ruleSetId].evaluativeRules[currRuleId] = 
                WonkaEngineStructs.WonkaRule({
                    ruleId: currRuleId,
                    name: ruleName,
                    ruleType: rType,
                    targetAttr: attrMap[attrName],
                    ruleValue: rVal,
                    ruleDomainKeys: new string[](0),   
                    customOpArgs: new bytes32[](CONST_CUSTOM_OP_ARGS),
                    parentRuleSetId: ruleSetId,
                    notOpFlag: notFlag,
                    isPassiveFlag: passiveFlag
                });

            bool isOpRule = ((uint(RuleTypes.OpAdd) == rType) || (uint(RuleTypes.OpSub) == rType) || (uint(RuleTypes.OpMult) == rType) || (uint(RuleTypes.OpDiv) == rType) || (uint(RuleTypes.CustomOp) == rType));
 
            if ( (uint(RuleTypes.InDomain) == rType) || isOpRule)  {                     
                
                splitStrIntoMap(rVal, ",", ruletrees[ruler].allRuleSets[ruleSetId].evaluativeRules[currRuleId], isOpRule);
            }

        } else {
            ruletrees[ruler].allRuleSets[ruleSetId].assertiveRuleList.push(currRuleId);

            ruletrees[ruler].allRuleSets[ruleSetId].assertiveRules[currRuleId] = 
                WonkaEngineStructs.WonkaRule({
                    ruleId: currRuleId,
                    name: ruleName,
                    ruleType: rType,
                    targetAttr: attrMap[attrName],
                    ruleValue: rVal,
                    ruleDomainKeys: new string[](0),
                    customOpArgs: new bytes32[](CONST_CUSTOM_OP_ARGS),
                    parentRuleSetId: ruleSetId,
                    notOpFlag: notFlag,
                    isPassiveFlag: passiveFlag
                });

        }
    }

    /// @dev This method will supply the args to the last rule added (of type Custom Operator)
    /// @author Aaron Kendall
    /// @notice Currently, a Rule can only belong to one RuleSet
    function addRuleCustomOpArgs(address ruler, bytes32 ruleSetId, bytes32 arg1, bytes32 arg2, bytes32 arg3, bytes32 arg4) public onlyEngineOwnerOrTreeOwner(ruler) {

        require(ruletrees[ruler].allRuleSets[ruleSetId].isValue == true, "The specified RuleSet does not exist.");

        require(ruletrees[ruler].allRuleSets[ruleSetId].evaluativeRules[lastRuleId].ruleType == uint(RuleTypes.CustomOp), "The last rule added to this RuleTree was not a Custom Op rule.");

        ruletrees[ruler].allRuleSets[ruleSetId].evaluativeRules[lastRuleId].customOpArgs[0] = arg1;
        ruletrees[ruler].allRuleSets[ruleSetId].evaluativeRules[lastRuleId].customOpArgs[1] = arg2;
        ruletrees[ruler].allRuleSets[ruleSetId].evaluativeRules[lastRuleId].customOpArgs[2] = arg3;
        ruletrees[ruler].allRuleSets[ruleSetId].evaluativeRules[lastRuleId].customOpArgs[3] = arg4;
    }    

    /// @dev This method will add a new source to the mapping cache.
    /// @author Aaron Kendall
    /// @notice 
    function addSource(bytes32 srcName, bytes32 sts, address cntrtAddr, bytes32 methName, bytes32 setMethName) public onlyEngineOwner {

        sourceMap[srcName] = 
            WonkaEngineStructs.WonkaSource({
                sourceName: srcName,
                status: sts,
                contractAddress: cntrtAddr,
                methodName: methName,
                setMethodName: setMethName,
                isValue: true
        });
    }    

    /// @dev This method will return the current number of Attributes in the cache
    /// @author Aaron Kendall
    /// @notice This method should only be used for debugging purposes.
    function getAttributesLength() public view returns(uint) {

        return attributes.length;
    }

    /// @dev This method will indicate whether or not the provided address/account has a RuleTree associated with it
    /// @author Aaron Kendall
    /// @notice This method should only be used for debugging purposes.
    function hasRuleTree(address ruler) public view returns(bool) {

        return (ruletrees[ruler].isValue == true);
    }

    /// @dev This method will indicate if the Attribute exists in the cache
    /// @author Aaron Kendall
    function isAttribute(bytes32 keyName) public view returns(bool) {

        return attrMap[keyName].isValue;
    }

    // Finds facet for function that is called and executes the
    // function if it is found and returns any value.
    fallback() external payable {
        DiamondStorage storage ds = diamondStorage();
        address facet = address(bytes20(ds.facets[msg.sig]));
        require(facet != address(0), "Function does not exist.");
        assembly {
            let ptr := mload(0x40)
            calldatacopy(ptr, 0, calldatasize())
            let result := delegatecall(gas(), facet, ptr, calldatasize(), 0, 0)
            let size := returndatasize()
            returndatacopy(ptr, 0, size)
            switch result
            case 0 {revert(ptr, size)}
            default {return (ptr, size)}
        }
    }
   
    /**
     ** SUPPORT METHODS
     **/
    /// @dev This method will parse a delimited string and insert them into the Domain map of a Rule
    /// @notice 
    function splitStrIntoMap(string memory str, string memory delimiter, WonkaEngineStructs.WonkaRule storage targetRule, bool isOpRule) private {  

        bytes memory b = bytes(str); //cast the string to bytes to iterate
        bytes memory delm = bytes(delimiter); 

        splitTempStr = "";

        for(uint i; i<b.length ; i++){          

            if(b[i] != delm[0]) { //check if a not space
                splitTempStr.push(b[i]);             
            }
            else { 
                string memory sTempVal = string(splitTempStr);
                targetRule.ruleValueDomain[sTempVal] = "Y";

                if (isOpRule)
                    targetRule.ruleDomainKeys.push(sTempVal);

                splitTempStr = "";                 
            }                
        }

        if(b[b.length-1] != delm[0]) { 
            string memory sTempValLast = string(splitTempStr);
            targetRule.ruleValueDomain[sTempValLast] = "Y";

            if (isOpRule)
                targetRule.ruleDomainKeys.push(sTempValLast);
        }
    }     
}