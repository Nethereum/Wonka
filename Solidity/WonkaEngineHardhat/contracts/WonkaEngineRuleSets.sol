// SPDX-License-Identifier: MIT
pragma solidity ^0.8.9;

pragma experimental ABIEncoderV2;

import "./WonkaLibrary.sol";
import "./WonkaEngineMetadata.sol";

/// @title An Ethereum contract that contains the functionality for a rules engine
/// @author Aaron Kendall
/// @notice 1.) Certain steps are required in order to use this engine correctly + 2.) Deployment of this contract to a blockchain is expensive (~8000000 gas) + 3.) Various require() statements are commented out to save deployment costs
/// @dev Even though you can create rule trees by calling this contract directly, it is generally recommended that you create them using the Nethereum library
contract WonkaEngineRuleSets {

    using WonkaLibrary for *;

    // An enum for the type of rules currently supported
    enum RuleTypes { IsEqual, IsLessThan, IsGreaterThan, Populated, InDomain, Assign, OpAdd, OpSub, OpMult, OpDiv, CustomOp, MAX_TYPE }
    RuleTypes constant defaultType = RuleTypes.IsEqual;

    string constant blankValue = "";

    uint constant CONST_CUSTOM_OP_ARGS = 4;

    address public rulesMaster;
    uint    public ruleCounter;
    uint    public lastRuleId;

    bool    orchestrationMode;
    bytes32 defaultTargetSource;

    WonkaEngineMetadata metadata;

    // The cache map for rulesets
    mapping(address => mapping(bytes32 => WonkaLibrary.WonkaRuleSet)) private ruleTreesMap;

    // The cache list for rulesets
    mapping(address => bytes32[]) private ruleTreesList;

    // The cache of records that are owned by "rulers" and that are validated when invoking a rule tree
    mapping(address => mapping(bytes32 => string)) currentRecords;

    // The cache of available sources for retrieving and setting attribute values found on other contracts
    mapping(bytes32 => WonkaLibrary.WonkaSource) sourceMap;

    // The cache of available sources for calling 'op' methods (i.e., that contain special logic to implement a custom operator)
    mapping(bytes32 => WonkaLibrary.WonkaSource) opMap;

    // For the function splitStr(...)
    // Currently unsure how the function will perform in a multithreaded scenario
    bytes splitTempStr; // temporarily holds the string part until a space is received

    /// @dev Constructor for the rules engine
    /// @notice Currently, the engine will create three dummy Attributes within the cache by default, but they will be removed later
    constructor(address _metadataAddr) {

        orchestrationMode = false;

        rulesMaster = msg.sender;
        ruleCounter = lastRuleId = 1;

        metadata = WonkaEngineMetadata(_metadataAddr);
    }

    modifier onlyEngineOwner() {
        
        // NOTE: Should be altered later
        // require(msg.sender == rulesMaster, "No exec perm");

        // Do not forget the "_;"! It will
        // be replaced by the actual function
        // body when the modifier is used.
        _;
    }

    modifier onlyEngineOwnerOrTreeOwner(address _RTOwner) {

        // NOTE: To be addressed later
        // require((msg.sender == rulesMaster) || (msg.sender == _RTOwner), "No exec perm");

        // NOTE: To be addressed later
        // require(ruletrees[_RTOwner].isValue == true, "No RT");

        // Do not forget the "_;"! It will
        // be replaced by the actual function
        // body when the modifier is used.
        _;
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
    function addRuleSet(address ruler, bytes32 ruleSetName, string memory desc, bytes32 parentRSName, bool severeFailureFlag, bool useAndOperator, bool flagFailImmediately) public onlyEngineOwnerOrTreeOwner(ruler) {

        if (parentRSName != "") {
            require(ruleTreesMap[ruler][parentRSName].isValue == true, "No parent RS");
        }

        require(ruleTreesMap[ruler][ruleSetName].isValue == false, "The specified RuleSet with the provided ID already exists.");

        ruleTreesList[ruler].push(ruleSetName);

        WonkaLibrary.WonkaRuleSet storage NewRuleSet = ruleTreesMap[ruler][ruleSetName];

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
            ruleTreesMap[ruler][parentRSName].childRuleSetList.push(ruleSetName);
            ruleTreesMap[ruler][parentRSName].isLeaf = false;
        }
    }

    /// @dev This method will add a new Rule to the indicated RuleSet
    /// @notice Currently, a Rule can only belong to one RuleSet
    function addRule(address ruler, bytes32 ruleSetId, bytes32 ruleName, bytes32 attrName, uint rType, string memory rVal, bool notFlag, bool passiveFlag) public onlyEngineOwnerOrTreeOwner(ruler) {

        require(ruleTreesMap[ruler][ruleSetId].isValue == true, "No RS");

        require(metadata.getAttribute(attrName).isValue, "No Attr (A)");

        require(rType < uint(RuleTypes.MAX_TYPE), "No RuleType");

        uint currRuleId = lastRuleId = ruleCounter;

        ruleCounter = ruleCounter + 1;

        if (passiveFlag) {
            ruleTreesMap[ruler][ruleSetId].evalRuleList.push(currRuleId);

            ruleTreesMap[ruler][ruleSetId].evaluativeRules[currRuleId].ruleId = currRuleId;
            ruleTreesMap[ruler][ruleSetId].evaluativeRules[currRuleId].name = ruleName;
            ruleTreesMap[ruler][ruleSetId].evaluativeRules[currRuleId].ruleType = rType;

            ruleTreesMap[ruler][ruleSetId].evaluativeRules[currRuleId].targetAttrName = attrName;

            ruleTreesMap[ruler][ruleSetId].evaluativeRules[currRuleId].ruleValue = rVal;
            ruleTreesMap[ruler][ruleSetId].evaluativeRules[currRuleId].ruleDomainKeys = new string[](0);   
            ruleTreesMap[ruler][ruleSetId].evaluativeRules[currRuleId].customOpArgs = new bytes32[](CONST_CUSTOM_OP_ARGS);
            ruleTreesMap[ruler][ruleSetId].evaluativeRules[currRuleId].parentRuleSetId = ruleSetId;
            ruleTreesMap[ruler][ruleSetId].evaluativeRules[currRuleId].notOpFlag = notFlag;
            ruleTreesMap[ruler][ruleSetId].evaluativeRules[currRuleId].isPassiveFlag = passiveFlag;
            
            bool isOpRule = ((uint(RuleTypes.OpAdd) == rType) || (uint(RuleTypes.OpSub) == rType) || (uint(RuleTypes.OpMult) == rType) || (uint(RuleTypes.OpDiv) == rType) || (uint(RuleTypes.CustomOp) == rType));

            if ( (uint(RuleTypes.InDomain) == rType) || isOpRule)  {
                splitStrIntoMap(rVal, ",", ruleTreesMap[ruler][ruleSetId].evaluativeRules[currRuleId], isOpRule);
            }

        } else {
            ruleTreesMap[ruler][ruleSetId].assertiveRuleList.push(currRuleId);

            WonkaLibrary.WonkaRule storage NewRule = ruleTreesMap[ruler][ruleSetId].assertiveRules[currRuleId];

            NewRule.ruleId = currRuleId;
            NewRule.name = ruleName;
            NewRule.ruleType = rType;
            NewRule.targetAttrName = attrName;
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

        require(ruleTreesMap[ruler][ruleSetId].isValue == true, "No RS");

        require(ruleTreesMap[ruler][ruleSetId].evaluativeRules[lastRuleId].ruleType == uint(RuleTypes.CustomOp), "LR not CO");

        ruleTreesMap[ruler][ruleSetId].evaluativeRules[lastRuleId].customOpArgs[0] = arg1;
        ruleTreesMap[ruler][ruleSetId].evaluativeRules[lastRuleId].customOpArgs[1] = arg2;
        ruleTreesMap[ruler][ruleSetId].evaluativeRules[lastRuleId].customOpArgs[2] = arg3;
        ruleTreesMap[ruler][ruleSetId].evaluativeRules[lastRuleId].customOpArgs[3] = arg4;
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

    /// @dev This method will invoke one RuleSet within a RuleTree when validating a stored record
    /// @notice This method will return a boolean that assists with traversing the RuleTree
    function executeWithReport(address ruler, bytes32 rsId, WonkaLibrary.WonkaRuleReport memory ruleReport) public returns (bool executeSuccess) {

        return executeWithReport(ruler, ruleTreesMap[ruler][rsId], ruleReport);
    }

    /// @dev This method will invoke one RuleSet within a RuleTree when validating a stored record
    /// @notice This method will return a boolean that assists with traversing the RuleTree
    function executeWithReport(address ruler, WonkaLibrary.WonkaRuleSet storage targetRuleSet, WonkaLibrary.WonkaRuleReport memory ruleReport) private returns (bool executeSuccess) {
       
        executeSuccess = true;

        emit WonkaLibrary.CallRuleSet(ruler, targetRuleSet.ruleSetId);

        /*
         * NOTE: Should be supported?
        if (transStateInd[ruletrees[ruler].ruleTreeId]) {

            require(transStateMap[ruletrees[ruler].ruleTreeId].isTransactionConfirmed(), "No conf trx");

            require(transStateMap[ruletrees[ruler].ruleTreeId].isExecutor(ruler), "No exec perm");
        }
        */

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
                tempResult = executeWithReport(ruler, ruleTreesMap[ruler][targetRuleSet.childRuleSetList[rsIdx]], ruleReport);
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

        string memory tempValue = getValueOnRecord(ruler, targetRule.targetAttrName);         

        bool almostOpInd  = false;

        emit WonkaLibrary.CallRule(ruler, targetRule.parentRuleSetId, targetRule.name, targetRule.ruleType);

        if (metadata.getAttribute(targetRule.targetAttrName).isNumeric) {

            testNumValue = tempValue.parseInt(0);
            ruleNumValue = targetRule.ruleValue.parseInt(0);

            // NOTE: Too expensive to deploy?
            // if (keccak256(abi.encodePacked(targetRule.ruleValue)) != keccak256(abi.encodePacked("NOW"))) {

            // This indicates that we are doing a timestamp comparison with the value for NOW (and maybe looking for a window of one day ahead)
            if (metadata.getAttribute(targetRule.targetAttrName).isString && metadata.getAttribute(targetRule.targetAttrName).isNumeric && (ruleNumValue <= 1)) {

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

            if (metadata.getAttribute(targetRule.targetAttrName).isNumeric) {
                ruleResult = (testNumValue == ruleNumValue);
            } else {
                ruleResult = (keccak256(abi.encodePacked(tempValue)) == keccak256(abi.encodePacked(targetRule.ruleValue)));
            }

        } else if (uint(RuleTypes.IsLessThan) == targetRule.ruleType) {

            if (metadata.getAttribute(targetRule.targetAttrName).isNumeric)
                ruleResult = (testNumValue < ruleNumValue);

        } else if (uint(RuleTypes.IsGreaterThan) == targetRule.ruleType) {

            if (metadata.getAttribute(targetRule.targetAttrName).isNumeric)
                ruleResult = (testNumValue > ruleNumValue);
        }
        else if (uint(RuleTypes.Populated) == targetRule.ruleType) {

            ruleResult = (keccak256(abi.encodePacked(tempValue)) != keccak256(abi.encodePacked("")));

        } else if (uint(RuleTypes.InDomain) == targetRule.ruleType) {

            ruleResult = (keccak256(abi.encodePacked(targetRule.ruleValueDomain[tempValue])) == keccak256(abi.encodePacked("Y")));

        } else if (uint(RuleTypes.Assign) == targetRule.ruleType) {

            setValueOnRecord(ruler, metadata.getAttribute(targetRule.targetAttrName).attrName, targetRule.ruleValue);

        } else if ( (uint(RuleTypes.OpAdd) == targetRule.ruleType) ||
                    (uint(RuleTypes.OpSub) == targetRule.ruleType) || 
                    (uint(RuleTypes.OpMult) == targetRule.ruleType) ||
                    (uint(RuleTypes.OpDiv) == targetRule.ruleType) ) {

            uint calculatedValue = calculateValue(ruler, targetRule);

            string memory convertedValue = calculatedValue.uintToBytes().bytes32ToString();

            setValueOnRecord(ruler, metadata.getAttribute(targetRule.targetAttrName).attrName, convertedValue);

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

            setValueOnRecord(ruler, metadata.getAttribute(targetRule.targetAttrName).attrName, customOpResult);
        }

        if (!ruleResult && ruleTreesMap[ruler][targetRule.parentRuleSetId].isLeaf) {            

            emit WonkaLibrary.CallRule(ruler, targetRule.parentRuleSetId, targetRule.name, targetRule.ruleType);

            ruleReport.ruleSetIds[ruleReport.ruleFailCount] = targetRule.parentRuleSetId;

            ruleReport.ruleIds[ruleReport.ruleFailCount] = targetRule.name;

            ruleReport.ruleFailCount += 1;
        }

    }

    /// @dev This method will return the default source
    function getDefaultSource() public view returns (bytes32) {

        return defaultTargetSource;
    }

    /// @dev This method will return the data that composes a particular Rule
    function getRuleProps(address ruler, bytes32 rsId, bool evalRuleFlag, uint ruleIdx) public view returns (bytes32, uint, bytes32, string memory, bool, bytes32[] memory) {

        WonkaLibrary.WonkaRule storage targetRule = (evalRuleFlag) ? ruleTreesMap[ruler][rsId].evaluativeRules[ruleTreesMap[ruler][rsId].evalRuleList[ruleIdx]] : ruleTreesMap[ruler][rsId].assertiveRules[ruleTreesMap[ruler][rsId].assertiveRuleList[ruleIdx]];
        
        return (targetRule.name, targetRule.ruleType, metadata.getAttribute(targetRule.targetAttrName).attrName, targetRule.ruleValue, targetRule.notOpFlag, targetRule.customOpArgs);
    }

    /// @dev This method will return the ID of a RuleSet that is the child of a parent RuleSet
    function getRuleSetChildId(address ruler, bytes32 rsId, uint rsChildIdx) public view returns (bytes32) {

        return ruleTreesMap[ruler][rsId].childRuleSetList[rsChildIdx];
    }

    /// @dev This method will return the data that composes a particular RuleSet
    function getRuleSetProps(address ruler, bytes32 rsId) public view returns (string memory, bool, bool, uint, uint, uint) {

        return (ruleTreesMap[ruler][rsId].description, ruleTreesMap[ruler][rsId].severeFailure, ruleTreesMap[ruler][rsId].andOp, ruleTreesMap[ruler][rsId].evalRuleList.length, ruleTreesMap[ruler][rsId].assertiveRuleList.length, ruleTreesMap[ruler][rsId].childRuleSetList.length);
    }

    /// @dev This method will return the value for an Attribute that is currently stored within the ruler's record
    /// @notice This method should only be used for debugging purposes.
    function getValueOnRecord(address ruler, bytes32 key) public returns(string memory) { 

        // NOTE: Likely to retire this check
        // require(ruletrees[ruler].isValue, "The provided user does not own anything on this instance of the contract.");

        require(metadata.getAttribute(key).isValue, "No Attr (G)");

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
            else {
                return blankValue;
            }
        }
    }

	/// @dev This method will indicate whether or not a particular source exists
    /// @notice This method should only be used for debugging purposes.
    function getIsSourceMapped(bytes32 key) public view returns(bool) {

        return sourceMap[key].isValue;
    }

	/// @dev This method will indicate whether or not the Orchestration mode has been enabled
    /// @notice This method should only be used for debugging purposes.
    function getOrchestrationMode() public view returns(bool) {

        return orchestrationMode;
    }	

    /// @dev This method will set the flag as to whether or not the engine should run in Orchestration mode (i.e., use the sourceMap)
    function setOrchestrationMode(bool orchMode, bytes32 defSource) public onlyEngineOwner { 

        orchestrationMode = orchMode;

        defaultTargetSource = defSource;
    }

    /// @dev This method will set an Attribute value on the record associated with the provided address/account
    /// @notice We do not currently check here to see if the value qualifies according to the Attribute's definition
    function setValueOnRecord(address ruler, bytes32 key, string memory value) public returns(string memory) { 

        // NOTE: Likely to retire this check
        // require(ruletrees[ruler].isValue, "The provided user does not own anything on this instance of the contract.");

        require(metadata.getAttribute(key).isValue, "No Attr (S)");
        
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

            if (metadata.getAttribute(keyName).isValue)
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

        retValue = "";

        bytes32 keyName = targetRule.customOpArgs[domainIdx];

        if (metadata.getAttribute(keyName).isValue)
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