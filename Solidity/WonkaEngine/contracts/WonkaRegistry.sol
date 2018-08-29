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

        bytes32[] ruleTreeGroveIds;

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

    // The cache of all rule trees 
    mapping(bytes32 => WonkaRuleTreeIndex) private ruleTrees;
    bytes32[] private ruleTreesEnum;

    // The grove names, their members, and the order in which they should be applied to a record
    mapping(bytes32 => mapping(bytes32 => uint)) private ruleGroves;

    /// @dev Constructor for the RuleTree registry
    /// @author Aaron Kendall
    /// @notice 
    constructor() public {
        // NOTE: Initialize members here
    }

    /// @dev This method will add a new index to the registry
    /// @author Aaron Kendall
    /// @notice 
    function addRuleTreeIndex(address ruler, bytes32 rsId, string desc, bytes32 ruleTreeGrpId, uint grpIdx, address host, uint minCost, uint maxCost, address[] associates, bytes32[] attributes, bytes32[] ops, uint createTime) public {

        // require(msg.sender == rulesMaster);

        require(ruleTrees[rsId].isValue != true);

        ruleTrees[rsId] = WonkaRuleTreeIndex({
            ruleTreeId: rsId,
            description: desc,
            ruleTreeGroveIds: new bytes32[](0),
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

        if (ruleTreeGrpId != "") {
            ruleTrees[rsId].ruleTreeGroveIds.push(ruleTreeGrpId);

            if (ruleGroves[ruleTreeGrpId][rsId] == 0) {

                if (grpIdx > 0) 
                    ruleGroves[ruleTreeGrpId][rsId] = grpIdx;
                else
                    ruleGroves[ruleTreeGrpId][rsId] = 999999;                    
            }
        }

        ruleTreesEnum.push(rsId);
    }

    /// @dev This method will return an index from the registry
    /// @author Aaron Kendall
    /// @notice 
    function getRuleTreeIndex(bytes32 rsId) public view returns (bytes32, string, address, address, uint, bytes32[], uint){

        // require(msg.sender == rulesMaster);

        require(ruleTrees[rsId].isValue == true);

        return (ruleTrees[rsId].ruleTreeId, ruleTrees[rsId].description, ruleTrees[rsId].hostContractAddress, ruleTrees[rsId].owner, ruleTrees[rsId].maxGasCost, ruleTrees[rsId].usedAttributes, ruleTrees[rsId].creationEpochTime);   
    }

    /// @dev This method will return all rule trees that belong to a specific group, in the order that they should be applied to a record
    /// @author Aaron Kendall
    /// @notice 
    function getGroupMembers(bytes32 rsGroupId) public view returns (bytes32[]) {

        // require(msg.sender == rulesMaster);

        bytes32[] memory groupMembers;

        uint maxIdx = ruleTreesEnum.length;

        for (uint i = 0; i < ruleTreesEnum.length; ++i) {

            bytes32 tmpRsId = ruleTreesEnum[i];

            if (ruleGroves[rsGroupId][tmpRsId] != 0) {

                uint orderIdx = ruleGroves[rsGroupId][tmpRsId];

                if (orderIdx > 0)
                    groupMembers[orderIdx] = tmpRsId;
                else
                    groupMembers[maxIdx] = tmpRsId;
            }
                
        }

        return groupMembers;
    }

    /// @dev This method will reorder the members of a rule grove
    /// @author Aaron Kendall
    /// @notice 
    function resetGroupOrder(bytes32 rsGroupId, bytes32[] rsIdList, uint[] orderList) public {

        // require(msg.sender == rulesMaster);

        require(rsIdList.length > 0);

        require(orderList.length > 0);

        require(rsIdList.length == orderList.length);

        uint idx = 0;

        // NOTE: Ensure that each ruletree mentioned is already a member of the group
        for (idx = 0; idx < rsIdList.length; ++idx) {
            require(ruleGroves[rsGroupId][rsIdList[idx]] > 0);
        }

        for (idx = 0; idx < rsIdList.length; ++idx) {
            ruleGroves[rsGroupId][rsIdList[idx]] = orderList[idx];
        }
    }

}