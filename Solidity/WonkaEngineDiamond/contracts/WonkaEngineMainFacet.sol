// SPDX-License-Identifier: MIT
pragma solidity ^0.6.8;

pragma experimental ABIEncoderV2;

import "./DiamondStorageContract.sol";
import "./DiamondHeaders.sol";
import "./DiamondFacet.sol";

import "./TransactionStateInterface.sol";

/// @title An Ethereum library that contains the functionality for a rules engine
/// @author Aaron Kendall
/// @notice 1.) Certain steps are required in order to use this engine correctly + 2.) Deployment of this contract to a blockchain is expensive (~8000000 gas) + 3.) Various require() statements are commented out to save deployment costs
/// @dev Even though you can create rule trees by calling this contract directly, it is generally recommended that you create them using the Nethereum library
contract WonkaEngineMainFacet is DiamondFacet {

    /**
     ** NOTE: These methods need to be altered for the storage
     **
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

        bool maxLenTrun = (pMaxLen > 0);

        attributes.push(WonkaAttr({
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

        bool almostOpInd  = false;
        uint testNumValue = 0;
        uint ruleNumValue = 0;

        string memory tempValue = getValueOnRecord(ruler, targetRule.targetAttr.attrName);

        // NOTE: USE WHEN DEBUGGING IS NEEDED
        emit CallRule(ruler, targetRule.parentRuleSetId, targetRule.name, targetRule.ruleType);

        if (targetRule.targetAttr.isNumeric) {

            testNumValue = parseInt(tempValue, 0);
            ruleNumValue = parseInt(targetRule.ruleValue, 0);

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

            string memory convertedValue = bytes32ToString(uintToBytes(calculatedValue));

            setValueOnRecord(ruler, targetRule.targetAttr.attrName, convertedValue);

        } else if (uint(RuleTypes.CustomOp) == targetRule.ruleType) {

            bytes32 customOpName = "";

            if (targetRule.ruleDomainKeys.length > 0)
                customOpName = stringToBytes32(targetRule.ruleDomainKeys[0]);

            bytes32[] memory argsDomain = new bytes32[](CONST_CUSTOM_OP_ARGS);

            for (uint idx = 0; idx < CONST_CUSTOM_OP_ARGS; ++idx) {
                if (idx < targetRule.customOpArgs.length)
                    argsDomain[idx] = stringToBytes32(determineDomainValue(ruler, idx, targetRule));
                else
                    argsDomain[idx] = "";                    
            }

            string memory customOpResult = invokeCustomOperator(opMap[customOpName].contractAddress, ruler, opMap[customOpName].methodName, argsDomain[0], argsDomain[1], argsDomain[2], argsDomain[3]);

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
                return invokeValueRetrieval(sourceMap[key].contractAddress, ruler, sourceMap[key].methodName, key);
            }
            else if (sourceMap[defaultTargetSource].isValue){
                return invokeValueRetrieval(sourceMap[defaultTargetSource].contractAddress, ruler, sourceMap[defaultTargetSource].methodName, key);
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

            bytes32 bytes32Value = stringToBytes32(value);

            if (sourceMap[key].isValue && (keccak256(abi.encodePacked(sourceMap[key].setMethodName)) != keccak256(abi.encodePacked("")))) {
                return invokeValueSetter(sourceMap[key].contractAddress, ruler, sourceMap[key].setMethodName, key, bytes32Value);
            }
            else if (sourceMap[defaultTargetSource].isValue && (keccak256(abi.encodePacked(sourceMap[defaultTargetSource].setMethodName)) != keccak256(abi.encodePacked("")))){                
                return invokeValueSetter(sourceMap[defaultTargetSource].contractAddress, ruler, sourceMap[defaultTargetSource].setMethodName, key, bytes32Value);
            }
        }
    }
    **/

}