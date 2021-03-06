// SPDX-License-Identifier: MIT
pragma solidity ^0.7.6;

import "./ERC2746.sol";
import "./TransactionStateInterface.sol";
import "./WonkaLibrary.sol";

/// @title An Ethereum contract that contains the functionality for a rules engine
/// @author Aaron Kendall
/// @notice 1.) Certain steps are required in order to use this engine correctly + 2.) Deployment of this contract to a blockchain is expensive (~8000000 gas) + 3.) Various require() statements are commented out to save deployment costs
/// @dev Even though you can create rule trees by calling this contract directly, it is generally recommended that you create them using the Nethereum library
contract WonkaEngine is ERC2746 {

    using WonkaLibrary for *;

    // An enum for the type of rules currently supported
    enum RuleTypes { IsEqual, IsLessThan, IsGreaterThan, Populated, InDomain, Assign, OpAdd, OpSub, OpMult, OpDiv, CustomOp, MAX_TYPE }
    RuleTypes constant defaultType = RuleTypes.IsEqual;

    string constant blankValue = "";

    uint constant CONST_CUSTOM_OP_ARGS = 4;

    address public rulesMaster;
    uint    public attrCounter;
    uint    public ruleCounter;
    uint    public lastRuleId;

    address lastSenderAddressProvided;
    bool    lastTransactionSuccess;

    bool    orchestrationMode;
    bytes32 defaultTargetSource;

    // The Attributes known by this instance of the rules engine
    mapping(bytes32 => WonkaLibrary.WonkaAttr) private attrMap;    
    WonkaLibrary.WonkaAttr[] public attributes;

    // The cache of rule trees that are owned by owner 
    mapping(address => WonkaLibrary.WonkaRuleTree) private ruletrees;

    // The cache of all created rulesets
    WonkaLibrary.WonkaRuleSet[] public rulesets;

    // The cache of records that are owned by "rulers" and that are validated when invoking a rule tree
    mapping(address => mapping(bytes32 => string)) currentRecords;

    // The cache of available sources for retrieving and setting attribute values found on other contracts
    mapping(bytes32 => WonkaLibrary.WonkaSource) sourceMap;

    // The cache of available sources for calling 'op' methods (i.e., that contain special logic to implement a custom operator)
    mapping(bytes32 => WonkaLibrary.WonkaSource) opMap;

    // The cache that indicates if a transaction state exist for a RuleTree
    mapping(bytes32 => bool) transStateInd;

    // The cache of transaction states assigned to RuleTrees
    mapping(bytes32 => TransactionStateInterface) transStateMap;

    // For the function splitStr(...)
    // Currently unsure how the function will perform in a multithreaded scenario
    bytes splitTempStr; // temporarily holds the string part until a space is received

    /// @dev Constructor for the rules engine
    /// @notice Currently, the engine will create three dummy Attributes within the cache by default, but they will be removed later
    constructor() {

        orchestrationMode = false;
        lastTransactionSuccess = false;

        rulesMaster = msg.sender;
        ruleCounter = lastRuleId = attrCounter = 1;

        attributes.push(WonkaLibrary.WonkaAttr({
            attrId: 1,
            attrName: "Title",
            maxLength: 256,
            maxLengthTruncate: true,
            maxNumValue: 0,
            defaultValue: "",
            isString: true,
            isDecimal: false,
            isNumeric: false,
            isValue: true                
        }));

        attrMap[attributes[attributes.length-1].attrName] = attributes[attributes.length-1];

        attributes.push(WonkaLibrary.WonkaAttr({
            attrId: 2,
            attrName: "Price",
            maxLength: 128,
            maxLengthTruncate: false,
            maxNumValue: 1000000,
            defaultValue: "",
            isString: false,
            isDecimal: false,
            isNumeric: true,
            isValue: true               
        }));

        attrMap[attributes[attributes.length-1].attrName] = attributes[attributes.length-1];
        
        attributes.push(WonkaLibrary.WonkaAttr({
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

    modifier onlyEngineOwner() {
        
        require(msg.sender == rulesMaster, "No exec perm");

        // Do not forget the "_;"! It will
        // be replaced by the actual function
        // body when the modifier is used.
        _;
    }

    modifier onlyEngineOwnerOrTreeOwner(address _RTOwner) {

        require((msg.sender == rulesMaster) || (msg.sender == _RTOwner), "No exec perm");

        require(ruletrees[_RTOwner].isValue == true, "No RT");

        // Do not forget the "_;"! It will
        // be replaced by the actual function
        // body when the modifier is used.
        _;
    }

    /// @dev This method will add a new Attribute to the cache.  By adding Attributes, we expand the set of possible values that can be held by a record.
    /// @notice 
    function addAttribute(bytes32 pAttrName, uint pMaxLen, uint pMaxNumVal, string memory pDefVal, bool pIsStr, bool pIsNum) public onlyEngineOwner override {

        attributes.push(WonkaLibrary.WonkaAttr({
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
    /// @notice Currently, only one ruletree can be defined for any given address/account
    function addRuleTree(address ruler, bytes32 rsName, string memory desc, bool severeFailureFlag, bool useAndOperator, bool flagFailImmediately) public onlyEngineOwner override {

        require(ruletrees[ruler].isValue != true, "RT already exists");

        WonkaLibrary.WonkaRuleTree storage NewRuleTree = ruletrees[ruler];
        NewRuleTree.ruleTreeId = rsName;
        NewRuleTree.description = desc;
        NewRuleTree.rootRuleSetName = rsName;
        NewRuleTree.allRuleSetList = new bytes32[](0);
        NewRuleTree.totalRuleCount = 0;
        NewRuleTree.isValue = true;

        addRuleSet(ruler, rsName, desc, "", severeFailureFlag, useAndOperator, flagFailImmediately);

        transStateInd[ruletrees[ruler].ruleTreeId] = false;
    }

    /// @dev This method will add a new custom operator to the cache.
    /// @notice 
    function addCustomOp(bytes32 srcName, bytes32 sts, address cntrtAddr, bytes32 methName) public onlyEngineOwner {

        opMap[srcName] = 
            WonkaLibrary.WonkaSource({
                sourceName: srcName,
                status: sts,
                contractAddress: cntrtAddr,
                methodName: methName,
                setMethodName: "",
                isValue: true
        });
    }

    /// @dev This method will add a new RuleSet to the cache and to the indicated RuleTree.  Using flagFailImmediately is not recommended and will likely be deprecated in the near future.
    /// @notice Currently, a RuleSet can only belong to one RuleTree and be a child of one parent RuleSet, though there are plans to have a RuleSet capable of being shared among parents
    function addRuleSet(address ruler, bytes32 ruleSetName, string memory desc, bytes32 parentRSName, bool severeFailureFlag, bool useAndOperator, bool flagFailImmediately) public onlyEngineOwnerOrTreeOwner(ruler) override {

        if (parentRSName != "") {
            require(ruletrees[ruler].allRuleSets[parentRSName].isValue == true, "No parent RS");
        }

        // NOTE: Unnecessary and commented out in order to save deployment costs (in terms of gas)
        // require(ruletrees[ruler].allRuleSets[ruleSetName].isValue == false, "The specified RuleSet with the provided ID already exists.");

        ruletrees[ruler].allRuleSetList.push(ruleSetName);

        WonkaLibrary.WonkaRuleSet storage NewRuleSet = ruletrees[ruler].allRuleSets[ruleSetName];

        NewRuleSet.ruleSetId = ruleSetName;
        NewRuleSet.description = desc;
        NewRuleSet.parentRuleSetId = parentRSName;
        NewRuleSet.severeFailure = severeFailureFlag;
        NewRuleSet.andOp = useAndOperator;
        NewRuleSet.failImmediately = flagFailImmediately;
        NewRuleSet.evalRuleList = new uint[](0);
        NewRuleSet.assertiveRuleList = new uint[](0);
        NewRuleSet.childRuleSetList = new bytes32[](0);
        NewRuleSet.isLeaf = true;
        NewRuleSet.isValue = true; 

        if (parentRSName != "") {
            ruletrees[ruler].allRuleSets[parentRSName].childRuleSetList.push(ruleSetName);
            ruletrees[ruler].allRuleSets[parentRSName].isLeaf = false;
        }
    }

    /// @dev This method will add a new Rule to the indicated RuleSet
    /// @notice Currently, a Rule can only belong to one RuleSet
    function addRule(address ruler, bytes32 ruleSetId, bytes32 ruleName, bytes32 attrName, uint rType, string memory rVal, bool notFlag, bool passiveFlag) public onlyEngineOwnerOrTreeOwner(ruler) override {

        require(ruletrees[ruler].allRuleSets[ruleSetId].isValue == true, "No RS");

        require(attrMap[attrName].isValue, "No Attr");

        require(rType < uint(RuleTypes.MAX_TYPE), "No RuleType");

        uint currRuleId = lastRuleId = ruleCounter;

        ruleCounter = ruleCounter + 1;

        ruletrees[ruler].totalRuleCount += 1; 

        if (passiveFlag) {
            ruletrees[ruler].allRuleSets[ruleSetId].evalRuleList.push(currRuleId);

            ruletrees[ruler].allRuleSets[ruleSetId].evaluativeRules[currRuleId].ruleId = currRuleId;
            ruletrees[ruler].allRuleSets[ruleSetId].evaluativeRules[currRuleId].name = ruleName;
            ruletrees[ruler].allRuleSets[ruleSetId].evaluativeRules[currRuleId].ruleType = rType;
            ruletrees[ruler].allRuleSets[ruleSetId].evaluativeRules[currRuleId].targetAttr = attrMap[attrName];
            ruletrees[ruler].allRuleSets[ruleSetId].evaluativeRules[currRuleId].ruleValue = rVal;
            ruletrees[ruler].allRuleSets[ruleSetId].evaluativeRules[currRuleId].ruleDomainKeys = new string[](0);   
            ruletrees[ruler].allRuleSets[ruleSetId].evaluativeRules[currRuleId].customOpArgs = new bytes32[](CONST_CUSTOM_OP_ARGS);
            ruletrees[ruler].allRuleSets[ruleSetId].evaluativeRules[currRuleId].parentRuleSetId = ruleSetId;
            ruletrees[ruler].allRuleSets[ruleSetId].evaluativeRules[currRuleId].notOpFlag = notFlag;
            ruletrees[ruler].allRuleSets[ruleSetId].evaluativeRules[currRuleId].isPassiveFlag = passiveFlag;
            
            bool isOpRule = ((uint(RuleTypes.OpAdd) == rType) || (uint(RuleTypes.OpSub) == rType) || (uint(RuleTypes.OpMult) == rType) || (uint(RuleTypes.OpDiv) == rType) || (uint(RuleTypes.CustomOp) == rType));

            if ( (uint(RuleTypes.InDomain) == rType) || isOpRule)  {
                splitStrIntoMap(rVal, ",", ruletrees[ruler].allRuleSets[ruleSetId].evaluativeRules[currRuleId], isOpRule);
            }

        } else {
            ruletrees[ruler].allRuleSets[ruleSetId].assertiveRuleList.push(currRuleId);

            WonkaLibrary.WonkaRule storage NewRule = ruletrees[ruler].allRuleSets[ruleSetId].assertiveRules[currRuleId];

            NewRule.ruleId = currRuleId;
            NewRule.name = ruleName;
            NewRule.ruleType = rType;
            NewRule.targetAttr = attrMap[attrName];
            NewRule.ruleValue = rVal;
            NewRule.ruleDomainKeys = new string[](0);
            NewRule.customOpArgs = new bytes32[](CONST_CUSTOM_OP_ARGS);
            NewRule.parentRuleSetId = ruleSetId;
            NewRule.notOpFlag = notFlag;
            NewRule.isPassiveFlag = passiveFlag;

        }
    }

    /// @dev This method will supply the args to the last rule added (of type Custom Operator)
    /// @notice Currently, a Rule can only belong to one RuleSet
    function addRuleCustomOpArgs(address ruler, bytes32 ruleSetId, bytes32 arg1, bytes32 arg2, bytes32 arg3, bytes32 arg4) public onlyEngineOwnerOrTreeOwner(ruler) {

        require(ruletrees[ruler].allRuleSets[ruleSetId].isValue == true, "No RS");

        require(ruletrees[ruler].allRuleSets[ruleSetId].evaluativeRules[lastRuleId].ruleType == uint(RuleTypes.CustomOp), "LR not CO");

        ruletrees[ruler].allRuleSets[ruleSetId].evaluativeRules[lastRuleId].customOpArgs[0] = arg1;
        ruletrees[ruler].allRuleSets[ruleSetId].evaluativeRules[lastRuleId].customOpArgs[1] = arg2;
        ruletrees[ruler].allRuleSets[ruleSetId].evaluativeRules[lastRuleId].customOpArgs[2] = arg3;
        ruletrees[ruler].allRuleSets[ruleSetId].evaluativeRules[lastRuleId].customOpArgs[3] = arg4;
    }    

    /// @dev This method will add a new source to the mapping cache.
    /// @notice 
    function addSource(bytes32 srcName, bytes32 sts, address cntrtAddr, bytes32 methName, bytes32 setMethName) public onlyEngineOwner {

        sourceMap[srcName] = 
            WonkaLibrary.WonkaSource({
                sourceName: srcName,
                status: sts,
                contractAddress: cntrtAddr,
                methodName: methName,
                setMethodName: setMethName,
                isValue: true
        });
    }

    /// @dev This method will invoke the ruler's RuleTree in order to validate their stored record.  This method should be invoked via a call() and not a transaction().
    /// @notice This method will only return a boolean
    function executeRuleTree(address ruler) public onlyEngineOwnerOrTreeOwner(ruler) override returns (bool executeSuccess) {

        executeSuccess = true;

        require(ruletrees[ruler].allRuleSetList.length > 0, "Empty RT");

        // NOTE: Unnecessary and commented out in order to save deployment costs (in terms of gas)
        // require(ruletrees[ruler].rootRuleSetName != "", "The specified RuleTree has an invalid root.");

        // NOTE: USE WHEN DEBUGGING IS NEEDED
        emit WonkaLibrary.CallRuleTree(ruler);

        lastSenderAddressProvided = ruler;

        WonkaLibrary.WonkaRuleReport memory report = WonkaLibrary.WonkaRuleReport({
            ruleFailCount: 0,
            ruleSetIds: new bytes32[](ruletrees[ruler].totalRuleCount),
            ruleIds: new bytes32[](ruletrees[ruler].totalRuleCount)
        });

        executeWithReport(ruler, ruletrees[ruler].allRuleSets[ruletrees[ruler].rootRuleSetName], report);

        executeSuccess = lastTransactionSuccess = (report.ruleFailCount == 0);
    }

    /// @dev This method will invoke the ruler's RuleTree in order to validate their stored record.  This method should be invoked via a call() and not a transaction().
    /// @notice This method will return a disassembled RuleReport that can be reassembled, especially by using the Nethereum library
    function executeWithReport(address ruler) public onlyEngineOwnerOrTreeOwner(ruler) returns (uint fails, bytes32[] memory rsets, bytes32[] memory rules) {

        require(ruletrees[ruler].allRuleSetList.length > 0, "Empty RT");

        // NOTE: Unnecessary and commented out in order to save deployment costs (in terms of gas)
        // require(ruletrees[ruler].rootRuleSetName != "", "The specified RuleTree has an invalid root.");

        // NOTE: USE WHEN DEBUGGING IS NEEDED
        emit WonkaLibrary.CallRuleTree(ruler);

        lastSenderAddressProvided = ruler;

        WonkaLibrary.WonkaRuleReport memory report = WonkaLibrary.WonkaRuleReport({
            ruleFailCount: 0,
            ruleSetIds: new bytes32[](ruletrees[ruler].totalRuleCount),
            ruleIds: new bytes32[](ruletrees[ruler].totalRuleCount)
            });

        executeWithReport(ruler, ruletrees[ruler].allRuleSets[ruletrees[ruler].rootRuleSetName], report);

        return (report.ruleFailCount, report.ruleSetIds, report.ruleIds);       
    }

    /// @dev This method will invoke one RuleSet within a RuleTree when validating a stored record
    /// @notice This method will return a boolean that assists with traversing the RuleTree
    function executeWithReport(address ruler, WonkaLibrary.WonkaRuleSet storage targetRuleSet, WonkaLibrary.WonkaRuleReport memory ruleReport) private returns (bool executeSuccess) {
       
        executeSuccess = true;

        // NOTE: USE WHEN DEBUGGING IS NEEDED
        emit WonkaLibrary.CallRuleSet(ruler, targetRuleSet.ruleSetId);

        if (transStateInd[ruletrees[ruler].ruleTreeId]) {

            require(transStateMap[ruletrees[ruler].ruleTreeId].isTransactionConfirmed(), "No conf trx");

            require(transStateMap[ruletrees[ruler].ruleTreeId].isExecutor(ruler), "No exec perm");
        }

        bool tempResult = false;
        bool tempSetResult = true;
        bool useAndOp = targetRuleSet.andOp;
        bool failImmediately = targetRuleSet.failImmediately;
        bool severeFailure = targetRuleSet.severeFailure;

        // Now invoke the rules
        for (uint idx = 0; idx < targetRuleSet.evalRuleList.length; idx++) {
            
            WonkaLibrary.WonkaRule storage tempRule = targetRuleSet.evaluativeRules[targetRuleSet.evalRuleList[idx]];

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
            emit WonkaLibrary.RuleSetError(ruler, targetRuleSet.ruleSetId, severeFailure);
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
    /// @notice This method will return a boolean that assists with traversing the RuleTree
    function executeWithReport(address ruler, WonkaLibrary.WonkaRule storage targetRule, WonkaLibrary.WonkaRuleReport memory ruleReport) private returns (bool ruleResult) {

        ruleResult = true;

        uint testNumValue = 0;
        uint ruleNumValue = 0;

        string memory tempValue = getValueOnRecord(ruler, targetRule.targetAttr.attrName);
        bool almostOpInd  = false;

        // NOTE: USE WHEN DEBUGGING IS NEEDED
        emit WonkaLibrary.CallRule(ruler, targetRule.parentRuleSetId, targetRule.name, targetRule.ruleType);

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
            emit WonkaLibrary.CallRule(ruler, targetRule.parentRuleSetId, targetRule.name, targetRule.ruleType);

            ruleReport.ruleSetIds[ruleReport.ruleFailCount] = targetRule.parentRuleSetId;

            ruleReport.ruleIds[ruleReport.ruleFailCount] = targetRule.name;

            ruleReport.ruleFailCount += 1;
        }

    }

    /// @dev This method will return the indicator of whether or not the last execuction of the engine was a validation success
    function getLastTransactionSuccess() public view returns(bool) {

        return lastTransactionSuccess;
    }

    /// @dev This method will return the data that composes a particular Rule
    function getRuleProps(address ruler, bytes32 rsId, bool evalRuleFlag, uint ruleIdx) public view override returns (bytes32, uint, bytes32, string memory, bool, bytes32[] memory) {

        require(ruletrees[ruler].isValue == true, "No RT");

        WonkaLibrary.WonkaRule storage targetRule = (evalRuleFlag) ? ruletrees[ruler].allRuleSets[rsId].evaluativeRules[ruletrees[ruler].allRuleSets[rsId].evalRuleList[ruleIdx]] : ruletrees[ruler].allRuleSets[rsId].assertiveRules[ruletrees[ruler].allRuleSets[rsId].assertiveRuleList[ruleIdx]];
        
        return (targetRule.name, targetRule.ruleType, targetRule.targetAttr.attrName, targetRule.ruleValue, targetRule.notOpFlag, targetRule.customOpArgs);
    }

    /// @dev This method will return the ID of a RuleSet that is the child of a parent RuleSet
    function getRuleSetChildId(address ruler, bytes32 rsId, uint rsChildIdx) public view returns (bytes32) {

        require(ruletrees[ruler].isValue == true, "No RT");

        return ruletrees[ruler].allRuleSets[rsId].childRuleSetList[rsChildIdx];
    }

    /// @dev This method will return the data that composes a particular RuleSet
    function getRuleSetProps(address ruler, bytes32 rsId) public view override returns (string memory, bool, bool, uint, uint, uint) {

        require(ruletrees[ruler].isValue == true, "No RT");

        return (ruletrees[ruler].allRuleSets[rsId].description, ruletrees[ruler].allRuleSets[rsId].severeFailure, ruletrees[ruler].allRuleSets[rsId].andOp, ruletrees[ruler].allRuleSets[rsId].evalRuleList.length, ruletrees[ruler].allRuleSets[rsId].assertiveRuleList.length, ruletrees[ruler].allRuleSets[rsId].childRuleSetList.length);
    }

    /// @dev This method will return the data that composes a particular RuleTree
    function getRuleTreeProps(address ruler) public view override returns (bytes32, string memory, bytes32) { 

        require(ruletrees[ruler].isValue == true, "No RT");

        return (ruletrees[ruler].ruleTreeId, ruletrees[ruler].description, ruletrees[ruler].rootRuleSetName);
    }

    /// @dev This method will return the value for an Attribute that is currently stored within the ruler's record
    /// @notice This method should only be used for debugging purposes.
    function getValueOnRecord(address ruler, bytes32 key) public override returns(string memory) { 

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
    /// @notice This method should only be used for debugging purposes.
    function getIsSourceMapped(bytes32 key) public view returns(bool) {

        return sourceMap[key].isValue;
    }

    /// @dev This method will return the current number of Attributes in the cache
    /// @notice This method should only be used for debugging purposes.
    function getNumberOfAttributes() public view returns(uint) {

        return attributes.length;
    }

	/// @dev This method will indicate whether or not the Orchestration mode has been enabled
    /// @notice This method should only be used for debugging purposes.
    function getOrchestrationMode() public view returns(bool) {

        return orchestrationMode;
    }	

    /// @dev This method will indicate whether or not the provided address/account has a RuleTree associated with it
    /// @notice This method should only be used for debugging purposes.
    function hasRuleTree(address ruler) public view returns(bool) {

        return (ruletrees[ruler].isValue == true);
    }

    function removeRuleTree(address /*_owner*/) public pure override returns (bool) {
        // NOTE: Does nothing currently
        return true;
    }    

    /// @dev This method will set the flag as to whether or not the engine should run in Orchestration mode (i.e., use the sourceMap)
    function setOrchestrationMode(bool orchMode, bytes32 defSource) public onlyEngineOwner { 

        orchestrationMode = orchMode;

        defaultTargetSource = defSource;
    }

    /// @dev This method will set the transaction state to be used by a RuleTree
    function setTransactionState(address ruler, address transStateAddr) public {

        transStateInd[ruletrees[ruler].ruleTreeId] = true;
        transStateMap[ruletrees[ruler].ruleTreeId] = TransactionStateInterface(transStateAddr);
    }

    /// @dev This method will set an Attribute value on the record associated with the provided address/account
    /// @notice We do not currently check here to see if the value qualifies according to the Attribute's definition
    function setValueOnRecord(address ruler, bytes32 key, string memory value) public override returns(string memory) { 

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
            else {
                return "";
            }
        }
    }

    /***********************
     *   SUPPORT METHODS   *
     ***********************/

    /// @dev This method will calculate the value for a Rule according to its type (Add, Subtract, etc.) and its domain values
    /// @notice 
    function calculateValue(address ruler, WonkaLibrary.WonkaRule storage targetRule) private returns (uint calcValue){  

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

    /// @dev This method will assist by returning the correct value, either a literal static value or one obtained through retrieval
    function determineDomainValue(address ruler, uint domainIdx, WonkaLibrary.WonkaRule storage targetRule) private returns (string memory retValue) {

        bytes32 keyName = targetRule.customOpArgs[domainIdx];

        if (attrMap[keyName].isValue)
            retValue = getValueOnRecord(ruler, keyName);
        else
            retValue = keyName.bytes32ToString();
    }  

    /// @dev This method will parse a delimited string and insert them into the Domain map of a Rule
    /// @notice 
    function splitStrIntoMap(string memory str, string memory delimiter, WonkaLibrary.WonkaRule storage targetRule, bool isOpRule) private {  

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