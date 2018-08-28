pragma solidity ^0.4.24;

/// @title An Ethereum library that contains information about all the instances of the Wonka rules engines in a blockchain
/// @author Aaron Kendall
/// @notice 
/// @dev 
contract WonkaRegistry {

 	/// @title Defines a ruletree index
	/// @author Aaron Kendall
	/// @notice This class will provide information on any RuleTree in the 'tree-verse' (which contract owns it, who owns it, the cost associated with it, etc.)
    struct WonkaRuleTreeIndex {

        bytes32 ruleTreeId;

        string description;

        bytes32[] ruleTreeGroupIds;

        address hostContractAddress;

        // This property also doubles as the ID for the ruletree within an instance of the WonkaEngine
        address owner;

        uint minGasCost;

        uint maxGasCost;

        // These are other contracts that the RuleTree talks to, from within the host
        address[] contractAssociates;

        bytes32[] usedAttributes;

        bytes32[] usedCustomOperators;

        uint creationEpochTime;
        
        bool isValue;
    }

    // The cache of rule trees 
    mapping(bytes32 => WonkaRuleTreeIndex) private ruletrees;
    bytes32[] private ruleTreesEnum;

 	/// @dev Constructor for the RuleTree registry
	/// @author Aaron Kendall
	/// @notice 
    constructor() public {
        // NOTE: Initialize members here
    }

 	/// @dev This method will add a new index to the registry
	/// @author Aaron Kendall
	/// @notice 
    function addRuleTreeIndex(address ruler, bytes32 rsId, string desc, bytes32 ruleTreeGrpId, address host, uint minCost, uint maxCost, address[] associates, bytes32[] attributes, bytes32[] ops, uint createTime) public {

        // require(msg.sender == rulesMaster);

        require(ruletrees[rsId].isValue != true);

        ruletrees[rsId] = WonkaRuleTreeIndex({
            ruleTreeId: rsId,
            description: desc,
            ruleTreeGroupIds: new bytes32[](0),
            hostContractAddress: host,
            owner: ruler,
            minGasCost: minCost,
            maxGasCost: maxCost,
            contractAssociates: associates,
            usedAttributes: attributes,
            usedCustomOperators: ops,
            creationEpochTime: createTime,
            isValue: true
        });

        if (ruleTreeGrpId != "")
            ruletrees[rsId].ruleTreeGroupIds.push(ruleTreeGrpId);

        ruleTreesEnum.push(rsId);
    }

    /// @dev This method will return an index from the registry
	/// @author Aaron Kendall
	/// @notice 
    function getRuleTreeIndex(bytes32 rsId) public view returns (bytes32, string, bytes32[], address, address, uint, uint, address[], bytes32[], bytes32[], uint){

        // require(msg.sender == rulesMaster);

        require(ruletrees[rsId].isValue == true);

        return (ruletrees[rsId].ruleTreeId, ruletrees[rsId].description, ruletrees[rsId].ruleTreeGroupIds, ruletrees[rsId].hostContractAddress, ruletrees[rsId].owner, ruletrees[rsId].minGasCost, ruletrees[rsId].maxGasCost, ruletrees[rsId].contractAssociates, ruletrees[rsId].usedAttributes, ruletrees[rsId].usedCustomOperators, ruletrees[rsId].creationEpochTime);   
    }

}