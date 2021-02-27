// SPDX-License-Identifier: MIT
pragma solidity ^0.6.8;

import "./TransactionStateInterface.sol";
import "./WonkaLibrary.sol";

/// @title An Ethereum contract that contains the functionality for a rules engine
/// @author Aaron Kendall
/// @notice 1.) Certain steps are required in order to use this engine correctly + 2.) Deployment of this contract to a blockchain is expensive (~8000000 gas) + 3.) Various require() statements are commented out to save deployment costs
/// @dev Even though you can create rule trees by calling this contract directly, it is generally recommended that you create them using the Nethereum library
contract WonkaEngine {

    using WonkaLibrary for *;

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

        require(ruletrees[_RTOwner].isValue == true, "The specified RuleTree does not exist.");

        // Do not forget the "_;"! It will
        // be replaced by the actual function
        // body when the modifier is used.
        _;
    }

    /// @dev This method will add a new Attribute to the cache.  By adding Attributes, we expand the set of possible values that can be held by a record.
    /// @author Aaron Kendall
    /// @notice 
    function addAttribute(bytes32 pAttrName, uint pMaxLen, uint pMaxNumVal, string memory pDefVal, bool pIsStr, bool pIsNum) public onlyEngineOwner {

        attributes.push(WonkaAttr({
            attrId: attrCounter++,
            attrName: pAttrName,
            maxLength: pMaxLen,
            maxLengthTruncate: (pMaxLen > 0),
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

        ruletrees[ruler] = WonkaRuleTree({
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
            WonkaSource({
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
            WonkaRuleSet({
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
                WonkaRule({
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
                WonkaRule({
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
            WonkaSource({
                sourceName: srcName,
                status: sts,
                contractAddress: cntrtAddr,
                methodName: methName,
                setMethodName: setMethName,
                isValue: true
        });
    }

    /// @dev This method will invoke the ruler's RuleTree in order to validate their stored record.  This method should be invoked via a call() and not a transaction().
    /// @author Aaron Kendall
    /// @notice This method will only return a boolean
    function execute(address ruler) public onlyEngineOwnerOrTreeOwner(ruler) returns (bool executeSuccess) {

        executeSuccess = true;

        require(ruletrees[ruler].allRuleSetList.length > 0, "The specified RuleTree is empty.");

        // NOTE: Unnecessary and commented out in order to save deployment costs (in terms of gas)
        // require(ruletrees[ruler].rootRuleSetName != "", "The specified RuleTree has an invalid root.");

        // NOTE: USE WHEN DEBUGGING IS NEEDED
        emit CallRuleTree(ruler);

        lastSenderAddressProvided = ruler;

        WonkaRuleReport memory report = WonkaRuleReport({
            ruleFailCount: 0,
            ruleSetIds: new bytes32[](ruletrees[ruler].totalRuleCount),
            ruleIds: new bytes32[](ruletrees[ruler].totalRuleCount)
        });

        executeWithReport(ruler, ruletrees[ruler].allRuleSets[ruletrees[ruler].rootRuleSetName], report);

        executeSuccess = lastTransactionSuccess = (report.ruleFailCount == 0);
    }

    /// @dev This method will invoke the ruler's RuleTree in order to validate their stored record.  This method should be invoked via a call() and not a transaction().
    /// @author Aaron Kendall
    /// @notice This method will return a disassembled RuleReport that can be reassembled, especially by using the Nethereum library
    function executeWithReport(address ruler) public onlyEngineOwnerOrTreeOwner(ruler) returns (uint fails, bytes32[] memory rsets, bytes32[] memory rules) {

        require(ruletrees[ruler].allRuleSetList.length > 0, "The specified RuleTree is empty.");

        // NOTE: Unnecessary and commented out in order to save deployment costs (in terms of gas)
        // require(ruletrees[ruler].rootRuleSetName != "", "The specified RuleTree has an invalid root.");

        // NOTE: USE WHEN DEBUGGING IS NEEDED
        emit CallRuleTree(ruler);

        lastSenderAddressProvided = ruler;

        WonkaRuleReport memory report = WonkaRuleReport({
            ruleFailCount: 0,
            ruleSetIds: new bytes32[](ruletrees[ruler].totalRuleCount),
            ruleIds: new bytes32[](ruletrees[ruler].totalRuleCount)
            });

        executeWithReport(ruler, ruletrees[ruler].allRuleSets[ruletrees[ruler].rootRuleSetName], report);

        lastRuleReport = report;

        return (report.ruleFailCount, report.ruleSetIds, report.ruleIds);       
    }

    /// @dev This method will invoke one RuleSet within a RuleTree when validating a stored record
    /// @author Aaron Kendall
    /// @notice This method will return a boolean that assists with traversing the RuleTree
    function executeWithReport(address ruler, WonkaRuleSet storage targetRuleSet, WonkaRuleReport memory ruleReport) private returns (bool executeSuccess) {
       
        executeSuccess = true;

        // NOTE: USE WHEN DEBUGGING IS NEEDED
        emit CallRuleSet(ruler, targetRuleSet.ruleSetId);

        if (transStateInd[ruletrees[ruler].ruleTreeId]) {

            require(transStateMap[ruletrees[ruler].ruleTreeId].isTransactionConfirmed(), "Transaction has not been confirmed.");

            require(transStateMap[ruletrees[ruler].ruleTreeId].isExecutor(ruler), "Sender is not a permitted executor.");
        }

        bool tempResult = false;
        bool tempSetResult = true;
        bool useAndOp = targetRuleSet.andOp;
        bool failImmediately = targetRuleSet.failImmediately;
        bool severeFailure = targetRuleSet.severeFailure;

        // Now invoke the rules
        for (uint idx = 0; idx < targetRuleSet.evalRuleList.length; idx++) {
            
            WonkaRule storage tempRule = targetRuleSet.evaluativeRules[targetRuleSet.evalRuleList[idx]];

            tempResult = executeWithReport(ruler, tempRule, ruleReport);

            if (failImmediately)
                require(tempResult);

            if (idx == 0) {
                tempSetResult = tempResult;
            } else {
                if (useAndOp)
                    tempSetResult = (tempSetResult && tempResult);
                else
                    tempSetResult = (tempSetResult || tempResult);
            }
        }
		
        executeSuccess = tempSetResult;
		
        if (!executeSuccess) {
            emit RuleSetError(ruler, targetRuleSet.ruleSetId, severeFailure);
		}
		
        if (targetRuleSet.isLeaf && severeFailure)
            return executeSuccess;

        if (executeSuccess && (targetRuleSet.childRuleSetList.length > 0)) {

            // Now invoke the rulesets
            for (uint rsIdx = 0; rsIdx < targetRuleSet.childRuleSetList.length; rsIdx++) {
                tempResult = executeWithReport(ruler, ruletrees[ruler].allRuleSets[targetRuleSet.childRuleSetList[rsIdx]], ruleReport);
                executeSuccess = (executeSuccess && tempResult);
            }
        }
        else
            executeSuccess = true;

	    // NOTE: Should the transaction state be reset automatically upon the completion of the transaction?
        //if (transStateInd[ruletrees[ruler].ruleTreeId]) {
        //    transStateMap[ruletrees[ruler].ruleTreeId].revokeAllConfirmations();
        //}
    }

    /// @dev This method will invoke one Rule within a RuleSet when validating a stored record
    /// @author Aaron Kendall
    /// @notice This method will return a boolean that assists with traversing the RuleTree
    function executeWithReport(address ruler, WonkaRule storage targetRule, WonkaRuleReport memory ruleReport) private returns (bool ruleResult) {

        ruleResult = true;

        uint testNumValue = 0;
        uint ruleNumValue = 0;

        string memory tempValue = getValueOnRecord(ruler, targetRule.targetAttr.attrName);
        bool almostOpInd  = false;

        // NOTE: USE WHEN DEBUGGING IS NEEDED
        emit CallRule(ruler, targetRule.parentRuleSetId, targetRule.name, targetRule.ruleType);

        if (targetRule.targetAttr.isNumeric) {

            testNumValue = tempValue.parseInt(0);
            ruleNumValue = targetRule.ruleValue.parseInt(0);

            // NOTE: Too expensive to deploy?
            // if (keccak256(abi.encodePacked(targetRule.ruleValue)) != keccak256(abi.encodePacked("NOW"))) {

            // This indicates that we are doing a timestamp comparison with the value for NOW (and maybe looking for a window of one day ahead)
            if (targetRule.targetAttr.isString && targetRule.targetAttr.isNumeric && (ruleNumValue <= 1)) {

                if (ruleNumValue == 1) {
                    almostOpInd = true;
                }

                ruleNumValue = block.timestamp + (ruleNumValue * 1 days);
            }
			// This indicates that we are doing a block number comparison (i.e., the hex number is the keccak256() result for the string "BLOCKNUMOP")
            else if (keccak256(abi.encodePacked(targetRule.ruleValue)) == keccak256(abi.encodePacked("00000"))) {

                ruleNumValue = block.number;
            }
        }
        
        if (almostOpInd) {

            ruleResult = ((testNumValue > block.timestamp) && (testNumValue < ruleNumValue));

        } else if (uint(RuleTypes.IsEqual) == targetRule.ruleType) {

            if (targetRule.targetAttr.isNumeric) {
                ruleResult = (testNumValue == ruleNumValue);
            } else {
                ruleResult = (keccak256(abi.encodePacked(tempValue)) == keccak256(abi.encodePacked(targetRule.ruleValue)));
            }

        } else if (uint(RuleTypes.IsLessThan) == targetRule.ruleType) {

            if (targetRule.targetAttr.isNumeric)
                ruleResult = (testNumValue < ruleNumValue);

        } else if (uint(RuleTypes.IsGreaterThan) == targetRule.ruleType) {

            if (targetRule.targetAttr.isNumeric)
                ruleResult = (testNumValue > ruleNumValue);
        }
        else if (uint(RuleTypes.Populated) == targetRule.ruleType) {

            ruleResult = (keccak256(abi.encodePacked(tempValue)) != keccak256(abi.encodePacked("")));

        } else if (uint(RuleTypes.InDomain) == targetRule.ruleType) {

            ruleResult = (keccak256(abi.encodePacked(targetRule.ruleValueDomain[tempValue])) == keccak256(abi.encodePacked("Y")));

        } else if (uint(RuleTypes.Assign) == targetRule.ruleType) {

            setValueOnRecord(ruler, targetRule.targetAttr.attrName, targetRule.ruleValue);

        } else if ( (uint(RuleTypes.OpAdd) == targetRule.ruleType) ||
                    (uint(RuleTypes.OpSub) == targetRule.ruleType) || 
                    (uint(RuleTypes.OpMult) == targetRule.ruleType) ||
                    (uint(RuleTypes.OpDiv) == targetRule.ruleType) ) {

            uint calculatedValue = calculateValue(ruler, targetRule);

            string memory convertedValue = calculatedValue.uintToBytes().bytes32ToString();

            setValueOnRecord(ruler, targetRule.targetAttr.attrName, convertedValue);

        } else if (uint(RuleTypes.CustomOp) == targetRule.ruleType) {

            bytes32 customOpName = "";

            if (targetRule.ruleDomainKeys.length > 0)
                customOpName = targetRule.ruleDomainKeys[0].stringToBytes32();

            bytes32[] memory argsDomain = new bytes32[](CONST_CUSTOM_OP_ARGS);

            for (uint idx = 0; idx < CONST_CUSTOM_OP_ARGS; ++idx) {
                if (idx < targetRule.customOpArgs.length)
                    argsDomain[idx] = determineDomainValue(ruler, idx, targetRule).stringToBytes32();
                else
                    argsDomain[idx] = "";                    
            }

            string memory customOpResult = opMap[customOpName].contractAddress.invokeCustomOperator(ruler, opMap[customOpName].methodName, argsDomain[0], argsDomain[1], argsDomain[2], argsDomain[3]);

            setValueOnRecord(ruler, targetRule.targetAttr.attrName, customOpResult);
        }

        if (!ruleResult && ruletrees[ruler].allRuleSets[targetRule.parentRuleSetId].isLeaf) {            

            // NOTE: USE WHEN DEBUGGING IS NEEDED
            emit CallRule(ruler, targetRule.parentRuleSetId, targetRule.name, targetRule.ruleType);

            ruleReport.ruleSetIds[ruleReport.ruleFailCount] = targetRule.parentRuleSetId;

            ruleReport.ruleIds[ruleReport.ruleFailCount] = targetRule.name;

            ruleReport.ruleFailCount += 1;
        }

    }

    /// @dev This method will return the report generated by the engine's last execution
    /// @author Aaron Kendall
    function getLastRuleReport() public view returns (uint fails, bytes32[] memory rsets, bytes32[] memory rules) {

        return (lastRuleReport.ruleFailCount, lastRuleReport.ruleSetIds, lastRuleReport.ruleIds);
    }

    /// @dev This method will return the indicator of whether or not the last execuction of the engine was a validation success
    /// @author Aaron Kendall
    function getLastTransactionSuccess() public view returns(bool) {

        return lastTransactionSuccess;
    }

    /// @dev This method will return the data that composes a particular Rule
    /// @author Aaron Kendall
    function getRuleProps(address ruler, bytes32 rsId, bool evalRuleFlag, uint ruleIdx) public view returns (bytes32, uint, bytes32, string memory, bool, bytes32[] memory) {

        require(ruletrees[ruler].isValue == true, "The specified RuleTree does not exist.");

        WonkaRule storage targetRule = (evalRuleFlag) ? ruletrees[ruler].allRuleSets[rsId].evaluativeRules[ruletrees[ruler].allRuleSets[rsId].evalRuleList[ruleIdx]] : ruletrees[ruler].allRuleSets[rsId].assertiveRules[ruletrees[ruler].allRuleSets[rsId].assertiveRuleList[ruleIdx]];
        
        return (targetRule.name, targetRule.ruleType, targetRule.targetAttr.attrName, targetRule.ruleValue, targetRule.notOpFlag, targetRule.customOpArgs);
    }

    /// @dev This method will return the ID of a RuleSet that is the child of a parent RuleSet
    /// @author Aaron Kendall
    function getRuleSetChildId(address ruler, bytes32 rsId, uint rsChildIdx) public view returns (bytes32) {

        require(ruletrees[ruler].isValue == true, "The specified RuleTree does not exist.");

        return ruletrees[ruler].allRuleSets[rsId].childRuleSetList[rsChildIdx];
    }

    /// @dev This method will return the data that composes a particular RuleSet
    /// @author Aaron Kendall
    function getRuleSetProps(address ruler, bytes32 rsId) public view returns (string memory, bool, bool, uint, uint, uint) {

        require(ruletrees[ruler].isValue == true, "The specified RuleTree does not exist.");

        return (ruletrees[ruler].allRuleSets[rsId].description, ruletrees[ruler].allRuleSets[rsId].severeFailure, ruletrees[ruler].allRuleSets[rsId].andOp, ruletrees[ruler].allRuleSets[rsId].evalRuleList.length, ruletrees[ruler].allRuleSets[rsId].assertiveRuleList.length, ruletrees[ruler].allRuleSets[rsId].childRuleSetList.length);
    }

    /// @dev This method will return the data that composes a particular RuleTree
    /// @author Aaron Kendall
    function getRuleTreeProps(address ruler) public view returns (bytes32, string memory, bytes32) { 

        require(ruletrees[ruler].isValue == true, "The specified RuleTree does not exist.");

        return (ruletrees[ruler].ruleTreeId, ruletrees[ruler].description, ruletrees[ruler].rootRuleSetName);
    }

    /// @dev This method will return the value for an Attribute that is currently stored within the ruler's record
    /// @author Aaron Kendall
    /// @notice This method should only be used for debugging purposes.
    function getValueOnRecord(address ruler, bytes32 key) public returns(string memory) { 

        // NOTE: Likely to retire this check
        // require(ruletrees[ruler].isValue, "The provided user does not own anything on this instance of the contract.");
        // require (attrMap[key].isValue == true, "The specified Attribute does not exist.");

        if (!orchestrationMode) {
            return (currentRecords[ruler])[key];
        }
        else {

            if (sourceMap[key].isValue){
                return sourceMap[key].contractAddress.invokeValueRetrieval(ruler, sourceMap[key].methodName, key);
            }
            else if (sourceMap[defaultTargetSource].isValue){
                return sourceMap[defaultTargetSource].contractAddress.invokeValueRetrieval(ruler, sourceMap[defaultTargetSource].methodName, key);
            }
            else
                return blankValue;
        }
    }

	/// @dev This method will indicate whether or not a particular source exists
    /// @author Aaron Kendall
    /// @notice This method should only be used for debugging purposes.
    function getIsSourceMapped(bytes32 key) public view returns(bool) {

        return sourceMap[key].isValue;
    }

    /// @dev This method will return the current number of Attributes in the cache
    /// @author Aaron Kendall
    /// @notice This method should only be used for debugging purposes.
    function getNumberOfAttributes() public view returns(uint) {

        return attributes.length;
    }

	/// @dev This method will indicate whether or not the Orchestration mode has been enabled
    /// @author Aaron Kendall
    /// @notice This method should only be used for debugging purposes.
    function getOrchestrationMode() public view returns(bool) {

        return orchestrationMode;
    }	

    /// @dev This method will indicate whether or not the provided address/account has a RuleTree associated with it
    /// @author Aaron Kendall
    /// @notice This method should only be used for debugging purposes.
    function hasRuleTree(address ruler) public view returns(bool) {

        return (ruletrees[ruler].isValue == true);
    }

    /// @dev This method will set the flag as to whether or not the engine should run in Orchestration mode (i.e., use the sourceMap)
    /// @author Aaron Kendall
    function setOrchestrationMode(bool orchMode, bytes32 defSource) public onlyEngineOwner { 

        orchestrationMode = orchMode;

        defaultTargetSource = defSource;
    }

    /// @dev This method will set the transaction state to be used by a RuleTree
    /// @author Aaron Kendall
    function setTransactionState(address ruler, address transStateAddr) public {

        transStateInd[ruletrees[ruler].ruleTreeId] = true;
        transStateMap[ruletrees[ruler].ruleTreeId] = TransactionStateInterface(transStateAddr);
    }

    /// @dev This method will set an Attribute value on the record associated with the provided address/account
    /// @author Aaron Kendall
    /// @notice We do not currently check here to see if the value qualifies according to the Attribute's definition
    function setValueOnRecord(address ruler, bytes32 key, string memory value) public returns(string memory) { 

        // NOTE: Likely to retire this check
        // require(ruletrees[ruler].isValue, "The provided user does not own anything on this instance of the contract.");
        // require(attrMap[key].isValue == true, "The specified Attribute does not exist.");
        
        if (!orchestrationMode) {
            (currentRecords[ruler])[key] = value;
            return (currentRecords[ruler])[key];
        }
        else {

            bytes32 bytes32Value = value.stringToBytes32();

            if (sourceMap[key].isValue && (keccak256(abi.encodePacked(sourceMap[key].setMethodName)) != keccak256(abi.encodePacked("")))) {
                return sourceMap[key].contractAddress.invokeValueSetter(ruler, sourceMap[key].setMethodName, key, bytes32Value);
            }
            else if (sourceMap[defaultTargetSource].isValue && (keccak256(abi.encodePacked(sourceMap[defaultTargetSource].setMethodName)) != keccak256(abi.encodePacked("")))){                
                return sourceMap[defaultTargetSource].contractAddress.invokeValueSetter(ruler, sourceMap[defaultTargetSource].setMethodName, key, bytes32Value);
            }
        }
    }

    /***********************
     *   SUPPORT METHODS   *
     ***********************/

    /// @dev This method will calculate the value for a Rule according to its type (Add, Subtract, etc.) and its domain values
    /// @notice 
    function calculateValue(address ruler, WonkaRule storage targetRule) private returns (uint calcValue){  

        uint tmpValue = 0;
        uint finalValue = 0;

        for (uint i = 0; i < targetRule.ruleDomainKeys.length; i++) {

            bytes32 keyName = targetRule.ruleDomainKeys[i].stringToBytes32();

            if (attrMap[keyName].isValue)
                tmpValue = getValueOnRecord(ruler, keyName).parseInt(0);
            else
                tmpValue = targetRule.ruleDomainKeys[i].parseInt(0);

            if (i == 0)
                finalValue = tmpValue;
            else {

                if ( uint(RuleTypes.OpAdd) == targetRule.ruleType )
                    finalValue += tmpValue;
                else if ( uint(RuleTypes.OpSub) == targetRule.ruleType )
                    finalValue -= tmpValue;
                else if ( uint(RuleTypes.OpMult) == targetRule.ruleType )
                    finalValue *= tmpValue;
                else if ( uint(RuleTypes.OpDiv) == targetRule.ruleType )
                    finalValue /= tmpValue;                    
            }

        }

        calcValue = finalValue;
    }

    /// @author Aaron Kendall
    /// @dev This method will assist by returning the correct value, either a literal static value or one obtained through retrieval
    function determineDomainValue(address ruler, uint domainIdx, WonkaRule storage targetRule) private returns (string memory retValue) {

        bytes32 keyName = targetRule.customOpArgs[domainIdx];

        if (attrMap[keyName].isValue)
            retValue = getValueOnRecord(ruler, keyName);
        else
            retValue = keyName.bytes32ToString();
    }  

    /// @dev This method will parse a delimited string and insert them into the Domain map of a Rule
    /// @notice 
    function splitStrIntoMap(string memory str, string memory delimiter, WonkaRule storage targetRule, bool isOpRule) private {  

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