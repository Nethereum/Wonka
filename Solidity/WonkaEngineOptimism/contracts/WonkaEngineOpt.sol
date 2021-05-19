// SPDX-License-Identifier: MIT
pragma solidity ^0.7.6;

pragma experimental ABIEncoderV2;

import "./ERC2746.sol";
import "./TransactionStateInterface.sol";
import "./WonkaLibrary.sol";
import "./WonkaEngineMetadata.sol";
import "./WonkaEngineRuleSets.sol";

/// @title An Ethereum contract that contains the functionality for a rules engine
/// @author Aaron Kendall
/// @notice 1.) Certain steps are required in order to use this engine correctly + 2.) Deployment of this contract to a blockchain is expensive (~8000000 gas) + 3.) Various require() statements are commented out to save deployment costs
/// @dev Even though you can create rule trees by calling this contract directly, it is generally recommended that you create them using the Nethereum library
contract WonkaEngineOpt is ERC2746 {

    using WonkaLibrary for *;

    string constant blankValue = "";

    address public rulesMaster;

    address lastSenderAddressProvided;
    bool    lastTransactionSuccess;

    // The cache of rule trees that are owned by owner 
    mapping(address => WonkaLibrary.WonkaRuleTree) private ruletrees;

    // The cache that indicates if a transaction state exist for a RuleTree
    mapping(bytes32 => bool) transStateInd;

    // The cache of transaction states assigned to RuleTrees
    mapping(bytes32 => TransactionStateInterface) transStateMap;

    WonkaEngineMetadata wonkaMetadata;
    WonkaEngineRuleSets wonkaRuleSets;

    /// @dev Constructor for the rules engine
    /// @notice Currently, the engine will create three dummy Attributes within the cache by default, but they will be removed later
    constructor(address _metadataAddr, address _rulesetsAddr) {

        lastTransactionSuccess = false;

        rulesMaster = msg.sender;

        wonkaMetadata = WonkaEngineMetadata(_metadataAddr);
        wonkaRuleSets = WonkaEngineRuleSets(_rulesetsAddr);
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

        wonkaMetadata.addAttribute(pAttrName, pMaxLen, pMaxNumVal, pDefVal, pIsStr, pIsNum);
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

        wonkaRuleSets.addRuleSet(ruler, rsName, desc, "", severeFailureFlag, useAndOperator, flagFailImmediately);

        transStateInd[ruletrees[ruler].ruleTreeId] = false;
    }

    /// @dev This method will add a new custom operator to the cache.
    /// @notice 
    function addCustomOp(bytes32 srcName, bytes32 sts, address cntrtAddr, bytes32 methName) public onlyEngineOwner {

        wonkaRuleSets.addCustomOp(srcName, sts, cntrtAddr, methName);
    }

    /// @dev This method will add a new RuleSet to the cache and to the indicated RuleTree.  Using flagFailImmediately is not recommended and will likely be deprecated in the near future.
    /// @notice Currently, a RuleSet can only belong to one RuleTree and be a child of one parent RuleSet, though there are plans to have a RuleSet capable of being shared among parents
    function addRuleSet(address ruler, bytes32 ruleSetName, string memory desc, bytes32 parentRSName, bool severeFailureFlag, bool useAndOperator, bool flagFailImmediately) public onlyEngineOwnerOrTreeOwner(ruler) override {

        ruletrees[ruler].allRuleSetList.push(ruleSetName);

        wonkaRuleSets.addRuleSet(ruler, ruleSetName, desc, parentRSName, severeFailureFlag, useAndOperator, flagFailImmediately);
    }

    /// @dev This method will add a new Rule to the indicated RuleSet
    /// @notice Currently, a Rule can only belong to one RuleSet
    function addRule(address ruler, bytes32 ruleSetId, bytes32 ruleName, bytes32 attrName, uint rType, string memory rVal, bool notFlag, bool passiveFlag) public onlyEngineOwnerOrTreeOwner(ruler) override {

        wonkaRuleSets.addRule(ruler, ruleSetId, ruleName, attrName, rType, rVal, notFlag, passiveFlag);

        ruletrees[ruler].totalRuleCount += 1; 
    }

    /// @dev This method will supply the args to the last rule added (of type Custom Operator)
    /// @notice Currently, a Rule can only belong to one RuleSet
    function addRuleCustomOpArgs(address ruler, bytes32 ruleSetId, bytes32 arg1, bytes32 arg2, bytes32 arg3, bytes32 arg4) public onlyEngineOwnerOrTreeOwner(ruler) {

        wonkaRuleSets.addRuleCustomOpArgs(ruler, ruleSetId, arg1, arg2, arg3, arg4);
    }    

    /// @dev This method will add a new source to the mapping cache.
    /// @notice 
    function addSource(bytes32 srcName, bytes32 sts, address cntrtAddr, bytes32 methName, bytes32 setMethName) public onlyEngineOwner {

        wonkaRuleSets.addSource(srcName, sts, cntrtAddr, methName, setMethName);
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

        wonkaRuleSets.executeWithReport(ruler, ruletrees[ruler].rootRuleSetName, report);

        executeSuccess = lastTransactionSuccess = (report.ruleFailCount == 0);
    }

    /// @dev This method will return the default source
    function getDefaultSource() public view returns(bytes32) {

        return wonkaRuleSets.getDefaultSource();
    }

    /// @dev This method will return the indicator of whether or not the last execuction of the engine was a validation success
    function getLastTransactionSuccess() public view returns(bool) {

        return lastTransactionSuccess;
    }

    /// @dev This method will return the data that composes a particular Rule
    function getRuleProps(address ruler, bytes32 rsId, bool evalRuleFlag, uint ruleIdx) public view override returns (bytes32, uint, bytes32, string memory, bool, bytes32[] memory) {

        require(ruletrees[ruler].isValue == true, "No RT");
        
        return wonkaRuleSets.getRuleProps(ruler, rsId, evalRuleFlag, ruleIdx);
    }

    /// @dev This method will return the ID of a RuleSet that is the child of a parent RuleSet
    function getRuleSetChildId(address ruler, bytes32 rsId, uint rsChildIdx) public view returns (bytes32) {

        require(ruletrees[ruler].isValue == true, "No RT");

        return wonkaRuleSets.getRuleSetChildId(ruler, rsId, rsChildIdx);
    }

    /// @dev This method will return the data that composes a particular RuleSet
    function getRuleSetProps(address ruler, bytes32 rsId) public view override returns (string memory, bool, bool, uint, uint, uint) {

        require(ruletrees[ruler].isValue == true, "No RT");

        return wonkaRuleSets.getRuleSetProps(ruler, rsId);
    }

    /// @dev This method will return the data that composes a particular RuleTree
    function getRuleTreeProps(address ruler) public view override returns (bytes32, string memory, bytes32, uint) { 

        require(ruletrees[ruler].isValue == true, "No RT");

        return (ruletrees[ruler].ruleTreeId, ruletrees[ruler].description, ruletrees[ruler].rootRuleSetName, ruletrees[ruler].totalRuleCount);
    }

	/// @dev This method will indicate whether or not a particular source exists
    /// @notice This method should only be used for debugging purposes.
    function getIsSourceMapped(bytes32 key) public view returns(bool) {

        return wonkaRuleSets.getIsSourceMapped(key);
    }

    /// @dev This method will return the current number of Attributes in the cache
    /// @notice This method should only be used for debugging purposes.
    function getNumberOfAttributes() public view returns(uint) {

        return wonkaMetadata.getNumberOfAttributes();
    }

	/// @dev This method will indicate whether or not the Orchestration mode has been enabled
    /// @notice This method should only be used for debugging purposes.
    function getOrchestrationMode() public view returns(bool) {

        return wonkaRuleSets.getOrchestrationMode();
    }	

    /// @dev This method will return the value for an Attribute that is currently stored within the ruler's record
    /// @notice This method should only be used for debugging purposes.
    function getValueOnRecord(address ruler, bytes32 key) public override returns(string memory) { 

        return wonkaRuleSets.getValueOnRecord(ruler, key);
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

        wonkaRuleSets.setOrchestrationMode(orchMode, defSource);
    }

    /// @dev This method will set the transaction state to be used by a RuleTree
    function setTransactionState(address ruler, address transStateAddr) public {

        transStateInd[ruletrees[ruler].ruleTreeId] = true;
        transStateMap[ruletrees[ruler].ruleTreeId] = TransactionStateInterface(transStateAddr);
    }

    /// @dev This method will set an Attribute value on the record associated with the provided address/account
    /// @notice We do not currently check here to see if the value qualifies according to the Attribute's definition
    function setValueOnRecord(address ruler, bytes32 key, string memory value) public override returns(string memory) { 

        return wonkaRuleSets.setValueOnRecord(ruler, key, value);
    }

}