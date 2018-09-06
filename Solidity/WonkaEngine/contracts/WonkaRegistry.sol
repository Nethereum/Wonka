pragma solidity ^0.4.24;

/// @title An Ethereum library that contains information about all the instances of the Wonka rules engines in a blockchain
/// @author Aaron Kendall
/// @notice 
/// @dev 
contract WonkaRegistry {

    /// @title Defines a rule grove
    /// @author Aaron Kendall
    /// @notice This class will provide information on a Grove (i.e., a collection of RuleTrees)
    struct WonkaRuleGrove {

        bytes32 ruleGroveId;

        string description;

        bytes32[] ruleTreeMembers;

        mapping(bytes32 => uint) memberPositions;

        address owner;

        uint creationEpochTime;
        
        bool isValue;
    }

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

    string constant CONST_DEFAULT_VALUE = "Default";

    // The cache of all rule groves
    mapping(bytes32 => WonkaRuleGrove) private ruleGroves;
    bytes32[] private ruleGrovesEnum;

    // The cache of all rule trees 
    mapping(bytes32 => WonkaRuleTreeIndex) private ruleTrees;
    bytes32[] private ruleTreesEnum;

    /// @dev Constructor for the RuleTree registry
    /// @author Aaron Kendall
    /// @notice 
    constructor() public {
        // NOTE: Initialize members here
    }

    /// @dev This method will add a new grove to the registry
    /// @author Aaron Kendall
    /// @notice 
    function addRuleGrove(bytes32 groveId, string desc, address groveOwner, uint createTime) public {

        require(groveId != "");

        require(ruleGroves[groveId].isValue != true);

        ruleGroves[groveId] = WonkaRuleGrove({
            ruleGroveId: groveId,
            description: desc,
            ruleTreeMembers: new bytes32[](0),
            owner: groveOwner,
            creationEpochTime: createTime,
            isValue: true
        });

        ruleGrovesEnum.push(groveId);
    }

    /// @dev This method will add a new tree index to the registry
    /// @author Aaron Kendall
    /// @notice 
    function addRuleTreeIndex(address ruler, bytes32 rsId, string desc, bytes32 ruleTreeGrpId, uint grpIdx, address host, uint minCost, uint maxCost, address[] associates, bytes32[] attributes, bytes32[] ops, uint createTime) public {

        // require(msg.sender == rulesMaster);

        require(rsId != "");

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

            if (ruleGroves[ruleTreeGrpId].isValue != true)
                addRuleGrove(ruleTreeGrpId, CONST_DEFAULT_VALUE, ruler, createTime);

            ruleTrees[rsId].ruleTreeGroveIds.push(ruleTreeGrpId);

            ruleGroves[ruleTreeGrpId].ruleTreeMembers.push(rsId);

            if ((grpIdx > 0) && (grpIdx <= ruleGroves[ruleTreeGrpId].ruleTreeMembers.length)) {
                ruleGroves[ruleTreeGrpId].memberPositions[rsId] = grpIdx;
            }
        }
    }

    /// @dev This method will add a RuleTree to an existing Grove
    /// @author Aaron Kendall
    /// @notice 
    function addRuleTreeToGrove(bytes32 groveId, bytes32 treeId) public {

        // require(msg.sender == rulesMaster);

        require(ruleTrees[treeId].isValue == true);

        require(ruleGroves[groveId].isValue != true);

        require(ruleGroves[groveId].memberPositions[treeId] == 0);

        ruleGroves[groveId].ruleTreeMembers.push(treeId);

        ruleGroves[groveId].memberPositions[treeId] = (ruleGroves[groveId].ruleTreeMembers.length + 1);
    }

    /// @dev This method will return an index from the registry
    /// @author Aaron Kendall
    /// @notice 
    function getRuleTreeIndex(bytes32 rsId) public view returns (bytes32 rtid, string rtdesc, address hostaddr, address owner, uint maxGasCost, uint createTime, bytes32[] attributes){

        // require(msg.sender == rulesMaster);

        require(ruleTrees[rsId].isValue == true);

        return (ruleTrees[rsId].ruleTreeId, ruleTrees[rsId].description, ruleTrees[rsId].hostContractAddress, ruleTrees[rsId].owner, ruleTrees[rsId].maxGasCost, ruleTrees[rsId].creationEpochTime, ruleTrees[rsId].usedAttributes);   
    }

    /// @dev This method will return all rule trees that belong to a specific group, in the order that they should be applied to a record
    /// @author Aaron Kendall
    /// @notice 
    function getGroupMembers(bytes32 groveId) public view returns (bytes32[]) {

        // require(msg.sender == rulesMaster);

        require(ruleGroves[groveId].isValue == true);

        uint orderIdx = 0;

        bytes32[] memory groupMembers = new bytes32[](ruleGroves[groveId].ruleTreeMembers.length);

        for (uint i = 0; i < ruleGroves[groveId].ruleTreeMembers.length; ++i) {

            bytes32 tmpRsId = ruleGroves[groveId].ruleTreeMembers[i];
 
            orderIdx = ruleGroves[groveId].memberPositions[tmpRsId];

            if ((orderIdx > 0) && (orderIdx <= ruleGroves[groveId].ruleTreeMembers.length))
                groupMembers[orderIdx-1] = tmpRsId;
        }

        return groupMembers;
        // return ruleGroves[groveId].ruleTreeMembers;
    }

    /// @dev This method will return the ordered position of the RuleTree 'rsId' within the group 'rsGroupId'
    /// @author Aaron Kendall
    /// @notice 
    function getGroupOrderPosition(bytes32 groveId, bytes32 rsId) public view returns (uint) {

        // require(msg.sender == rulesMaster);

        require(ruleGroves[groveId].isValue != true);

        uint orderIdx = 999999;

        if (groveId != "") {

            if (rsId != "") {
                orderIdx = ruleGroves[groveId].memberPositions[rsId];
            }
        }

        return orderIdx;
    }    

    /// @dev This method will indicate whether or not the RuleTree has been added to the registry
    /// @author Aaron Kendall
    /// @notice 
    function isRuleTreeRegistered(bytes32 rsId) public view returns (bool) {

        return (ruleTrees[rsId].isValue == true);
    }

    /// @dev This method will reorder the members of a rule grove
    /// @author Aaron Kendall
    /// @notice 
    function resetGroupOrder(bytes32 groveId, bytes32[] rsIdList, uint[] orderList) public {

        // require(msg.sender == rulesMaster);

        require(rsIdList.length > 0);

        require(orderList.length > 0);

        require(rsIdList.length == orderList.length);

        require(ruleGroves[groveId].ruleTreeMembers.length == rsIdList.length);

        uint idx = 0;

        uint grpIdx = 0;

        bytes32 tmpId = "";

        for (idx = 0; idx < rsIdList.length; ++idx) {

            tmpId = rsIdList[idx];

            ruleGroves[groveId].ruleTreeMembers[idx] = tmpId;

            grpIdx = ruleGroves[groveId].memberPositions[tmpId];

            if ((grpIdx > 0) && (grpIdx <= ruleGroves[groveId].ruleTreeMembers.length)) {
                ruleGroves[groveId].memberPositions[tmpId] = grpIdx;
            }
        }
    }

}