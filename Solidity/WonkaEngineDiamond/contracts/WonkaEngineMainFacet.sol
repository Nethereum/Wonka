// SPDX-License-Identifier: MIT
pragma solidity ^0.6.8;

pragma experimental ABIEncoderV2;

import "./DiamondStorageContract.sol";
import "./DiamondHeaders.sol";
import "./DiamondFacet.sol";
import "./WonkaEngineStructs.sol";
import "./WonkaEngineSupportFacet.sol";
import "./WonkaEngineDiamond.sol";

import "./TransactionStateInterface.sol";

/// @title An Ethereum library that contains the functionality for a rules engine
/// @author Aaron Kendall
/// @notice 1.) Certain steps are required in order to use this engine correctly + 2.) Deployment of this contract to a blockchain is expensive (~8000000 gas) + 3.) Various require() statements are commented out to save deployment costs
/// @dev Even though you can create rule trees by calling this contract directly, it is generally recommended that you create them using the Nethereum library
contract WonkaEngineMainFacet is DiamondFacet {

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

    string constant blankValue = "";

    // An enum for the type of rules currently supported
    enum RuleTypes { IsEqual, IsLessThan, IsGreaterThan, Populated, InDomain, Assign, OpAdd, OpSub, OpMult, OpDiv, CustomOp, MAX_TYPE }
    RuleTypes constant defaultType = RuleTypes.IsEqual;

    bool lastTransactionSuccess;
    bool orchestrationMode;

    bytes32 defaultTargetSource;

    // The cache of records that are owned by "rulers" and that are validated when invoking a rule tree
    mapping(address => mapping(bytes32 => string)) public currentRecords;

    WonkaEngineDiamond diamond;

    /// @dev Constructor for the main facet
    /// @author Aaron Kendall
    constructor() public {
        lastTransactionSuccess = false;
        orchestrationMode = false; 
    }

    modifier onlyEngineOwner()
    {
        require(msg.sender == diamond.rulesMaster(), "The caller of this method does not have permission for this action.");

        // Do not forget the "_;"! It will
        // be replaced by the actual function
        // body when the modifier is used.
        _;
    }

    modifier onlyEngineOwnerOrTreeOwner(address _RTOwner) {

        require((msg.sender == diamond.rulesMaster()) || (msg.sender == _RTOwner), "The caller of this method does not have permission for this action.");

        require(diamond.hasRuleTree(_RTOwner) == true, "The specified RuleTree does not exist.");

        // Do not forget the "_;"! It will
        // be replaced by the actual function
        // body when the modifier is used.
        _;
    }

    /// @dev This method will set the address for the Wonka Diamond (proxy) 
    /// @author Aaron Kendall
    function setDiamondAddress(address payable _diamondAddress, address ruler) public onlyEngineOwnerOrTreeOwner(ruler) {

        require(msg.sender == diamond.rulesMaster(), "The caller of this method does not have permission for this action.");

        diamond = WonkaEngineDiamond(_diamondAddress);
    }    

    /**
     ** NOTE: These methods need to be altered for the storage
     **

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
     **/

    /// @dev This method will return the indicator of whether or not the last execuction of the engine was a validation success
    /// @author Aaron Kendall
    function getLastTransactionSuccess() public view returns(bool) {

        return lastTransactionSuccess;
    }

    /**
     **
     **
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
    */

    /// @dev This method will return the value for an Attribute that is currently stored within the ruler's record
    /// @author Aaron Kendall
    /// @notice This method should only be used for debugging purposes.
    function getValueOnRecord(address ruler, bytes32 key) public onlyEngineOwnerOrTreeOwner(ruler) returns(string memory) { 

        require (diamond.isAttribute(key), "The specified Attribute does not exist.");

        if (!orchestrationMode) {
            return (currentRecords[ruler])[key];
        }
        else {

            string memory recordValue = blankValue;

            DiamondStorage storage ds = diamondStorage();

            address supportFacetAddress = address(bytes20(ds.facets[WonkaEngineSupportFacet.invokeValueRetrieval.selector]));

            if (diamond.getSource(key).isValue) {                

                bytes memory IVRFunction = abi.encodeWithSelector(WonkaEngineSupportFacet.invokeValueRetrieval.selector, diamond.getSource(key).contractAddress, ruler, diamond.getSource(key).methodName, key);

                (bool success, bytes memory returnedData) = address(supportFacetAddress).delegatecall(IVRFunction);
                require(success);

                (recordValue) = abi.decode(returnedData, (string));
            }
            else if (diamond.getSource(defaultTargetSource).isValue){

                bytes memory IVRFunction = abi.encodeWithSelector(WonkaEngineSupportFacet.invokeValueRetrieval.selector, diamond.getSource(defaultTargetSource).contractAddress, ruler, diamond.getSource(defaultTargetSource).methodName, key);

                (bool success, bytes memory returnedData) = address(supportFacetAddress).delegatecall(IVRFunction);
                require(success);

                (recordValue) = abi.decode(returnedData, (string));
            }

            return recordValue;
        }
    }

	/// @dev This method will indicate whether or not a particular source exists
    /// @author Aaron Kendall
    /// @notice This method should only be used for debugging purposes.
    function getIsSourceMapped(bytes32 key) public view returns(bool) {

        return diamond.getSource(key).isValue;
    }

	/// @dev This method will indicate whether or not the Orchestration mode has been enabled
    /// @author Aaron Kendall
    /// @notice This method should only be used for debugging purposes.
    function getOrchestrationMode() public view returns(bool) {

        return orchestrationMode;
    }

    /// @dev This method will set the flag as to whether or not the engine should run in Orchestration mode (i.e., use the sourceMap)
    /// @author Aaron Kendall
    function setOrchestrationMode(bool orchMode, bytes32 defSource) public onlyEngineOwner { 

        orchestrationMode = orchMode;

        defaultTargetSource = defSource;
    }

    /// @dev This method will set an Attribute value on the record associated with the provided address/account
    /// @author Aaron Kendall
    /// @notice We do not currently check here to see if the value qualifies according to the Attribute's definition
    function setValueOnRecord(address ruler, bytes32 key, string memory value) public returns(string memory) { 

        require(diamond.hasRuleTree(ruler), "The provided user does not own anything on this instance of the contract.");

        require(diamond.isAttribute(key), "The specified Attribute does not exist.");
        
        if (!orchestrationMode) {
            (currentRecords[ruler])[key] = value;
            return (currentRecords[ruler])[key];
        }
        else {

            DiamondStorage storage ds = diamondStorage();

            address supportFacetAddress = address(bytes20(ds.facets[WonkaEngineSupportFacet.parseInt.selector]));

            bytes32 bytes32Value = "";
            bytes32 targetKey = "";
            string memory retValue = "";

            bytes memory STBFunction = abi.encodeWithSelector(WonkaEngineSupportFacet.stringToBytes32.selector, value);

            (bool successSTB, bytes memory returnedDataSTB) = address(supportFacetAddress).delegatecall(STBFunction);
            require(successSTB);
            (bytes32Value) = abi.decode(returnedDataSTB, (bytes32));

            if (diamond.getSource(key).isValue && (keccak256(abi.encodePacked(diamond.getSource(key).setMethodName)) != keccak256(abi.encodePacked("")))) {           
                targetKey = key;
            }
            else if (diamond.getSource(defaultTargetSource).isValue && (keccak256(abi.encodePacked(diamond.getSource(defaultTargetSource).setMethodName)) != keccak256(abi.encodePacked("")))) {                
                targetKey = defaultTargetSource;
            }

            if (targetKey != "") { 
                bytes memory IVSFunction = abi.encodeWithSelector(WonkaEngineSupportFacet.invokeValueSetter.selector, diamond.getSource(targetKey).contractAddress, ruler, diamond.getSource(targetKey).setMethodName, key, bytes32Value);

                (bool successIVS, bytes memory returnedDataIVS) = address(supportFacetAddress).delegatecall(IVSFunction);
                require(successIVS);
                (retValue) = abi.decode(returnedDataIVS, (string));
            }

            return retValue;
        }
    }

    // SUPPORT METHODS
    /// @dev This method will calculate the value for a Rule according to its type (Add, Subtract, etc.) and its domain values
    /// @notice 
    function calculateValue(address ruler, WonkaEngineStructs.WonkaRule storage targetRule, mapping(bytes32 => WonkaEngineStructs.WonkaAttr) storage attrMap) private returns (uint calcValue){  

        uint tmpValue   = 0;
        uint finalValue = 0;

        DiamondStorage storage ds = diamondStorage();

        address supportFacetAddress = address(bytes20(ds.facets[WonkaEngineSupportFacet.parseInt.selector]));

        for (uint i = 0; i < targetRule.ruleDomainKeys.length; i++) {

            bytes32 keyName = "";

            bytes memory STBFunction = abi.encodeWithSelector(WonkaEngineSupportFacet.stringToBytes32.selector, targetRule.ruleDomainKeys[i]);

            (bool successSTB, bytes memory returnedDataSTB) = address(supportFacetAddress).delegatecall(STBFunction);
            require(successSTB);

            (keyName) = abi.decode(returnedDataSTB, (bytes32));

            if (attrMap[keyName].isValue)
            {
                bytes memory PIFunction = abi.encodeWithSelector(WonkaEngineSupportFacet.parseInt.selector, getValueOnRecord(ruler, keyName), 0);

                (bool successPI, bytes memory returnedDataPI) = address(supportFacetAddress).delegatecall(PIFunction);
                require(successPI);

                (tmpValue) = abi.decode(returnedDataPI, (uint));
            }
            else
            {
                bytes memory PIFunction = abi.encodeWithSelector(WonkaEngineSupportFacet.parseInt.selector, targetRule.ruleDomainKeys[i], 0);

                (bool successPI, bytes memory returnedDataPI) = address(supportFacetAddress).delegatecall(PIFunction);
                require(successPI);

                (tmpValue) = abi.decode(returnedDataPI, (uint));
            }

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
    function determineDomainValue(address ruler, uint domainIdx, WonkaEngineStructs.WonkaRule storage targetRule) private returns (string memory retValue) {

        bytes32 keyName = targetRule.customOpArgs[domainIdx];

        if (diamond.isAttribute(keyName))
        {
            retValue = getValueOnRecord(ruler, keyName);
        }
        else
        {
            // NOTE: OLD WAY
            // retValue = bytes32ToString(keyName);

            // NOTE: ALT WAY
            // (bool success, bytes memory retData) = address(diamond).call(bytes4(bytes32(keccak256("bytes32ToString(bytes32)"))), keyName);
            // (uint a, uint[2] memory b, bytes memory c) = abi.decode(retData, (uint, uint[2], bytes))
            // (retValue) = abi.decode(retData, (string));

            DiamondStorage storage ds = diamondStorage();

            address supportFacetAddress = address(bytes20(ds.facets[WonkaEngineSupportFacet.bytes32ToString.selector]));

            bytes memory BTSFunction = abi.encodeWithSelector(WonkaEngineSupportFacet.bytes32ToString.selector, keyName);

            (bool success, bytes memory returnedData) = address(supportFacetAddress).delegatecall(BTSFunction);
            require(success);

            (retValue) = abi.decode(returnedData, (string));
        }
    }  

}