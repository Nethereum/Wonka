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

        ruleTreesEnum.push(rsId);

        if (ruleTreeGrpId != "") {

            ruleTrees[rsId].ruleTreeGroveIds.push(ruleTreeGrpId);

            if (grpIdx < ruleTreesEnum.length) 
                ruleGroves[ruleTreeGrpId][rsId] = grpIdx;
        }
    }

    /// @dev This method will return an index from the registry
    /// @author Aaron Kendall
    /// @notice 
    function getRuleTreeIndex(bytes32 rsId) public view returns (bytes32, string, address, address, uint, uint, bytes32[]){

        // require(msg.sender == rulesMaster);

        require(ruleTrees[rsId].isValue == true);

        return (ruleTrees[rsId].ruleTreeId, ruleTrees[rsId].description, ruleTrees[rsId].hostContractAddress, ruleTrees[rsId].owner, ruleTrees[rsId].maxGasCost, ruleTrees[rsId].creationEpochTime, ruleTrees[rsId].usedAttributes);   
    }

    /// @dev This method will return all rule trees that belong to a specific group, in the order that they should be applied to a record
    /// @author Aaron Kendall
    /// @notice 
    function getGroupMembers(bytes32 rsGroupId) public view returns (bytes32[]) {

        // require(msg.sender == rulesMaster);

        require(ruleTreesEnum.length > 0);

        uint orderIdx = 0;

        bytes32[] memory groupMembers = new bytes32[](ruleTreesEnum.length);

        for (uint i = 0; i < ruleTreesEnum.length; ++i) {

            bytes32 tmpRsId = ruleTreesEnum[i];
 
            orderIdx = ruleGroves[rsGroupId][tmpRsId];

            groupMembers[orderIdx] = tmpRsId;
                
        }

        return groupMembers;
    }

    /// @dev This method will return the ordered position of the RuleTree 'rsId' within the group 'rsGroupId'
    /// @author Aaron Kendall
    /// @notice 
    function getGroupOrderPosition(bytes32 rsGroupId, bytes32 rsId) public view returns (uint) {

        // require(msg.sender == rulesMaster);

        require(ruleTreesEnum.length > 0);

        uint orderIdx = 999999;

        if (rsGroupId != "") {

            if (rsId != "") {

                orderIdx = ruleGroves[rsGroupId][rsId];
            }
        }

        return orderIdx;
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

        for (idx = 0; idx < rsIdList.length; ++idx) {
            ruleGroves[rsGroupId][rsIdList[idx]] = orderList[idx];
        }
    }

}