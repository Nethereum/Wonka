pragma solidity ^0.5.1;

import "./TransactionStateInterface.sol";

/// @title An example of a class that implements the interface defined by TransactionStateInterface
/// @author Aaron Kendall
/// @notice 
/// @dev 
contract WonkaTransactionState is TransactionStateInterface {

    uint constant CONST_MAX_OWNERS = 250;
    
    uint minReqScoreForApproval;

    address[] owners;

    mapping(address => bool) ownerConfirmations;

    mapping(address => uint) ownerWeights;

    mapping(address => bool) executors;
    
    /// @dev Constructor for the transaction state
    /// @author Aaron Kendall
    /// @notice 
    constructor() public {
        
        minReqScoreForApproval = 0;
    }    
  
    function addConfirmation(address owner) public {
        
        require(ownerWeights[owner] > 0, "The provided address does not belong to a registered owner.");
        
        ownerConfirmations[owner] = true;
    }

    function clearPendingTransaction() public {
        
        revokeAllConfirmations();

        // NOTE: In the case that we introduce othe state variables, they should be reset here
    }

    function getCurrentScore() public view returns (uint) {
        
        uint currScore = 0;
        
        for (uint16 idx = 0; idx < owners.length; ++idx) {
            if (ownerConfirmations[owners[idx]])
                currScore += ownerWeights[owners[idx]];
        }
        
        return currScore;
    }

    function getMinScoreRequirement() public view returns (uint) {
        
        return minReqScoreForApproval;
    }
  
    function getOwnersConfirmed() public view returns (address[] memory) {
        
        uint16 confirmedCount = 0;
        
        for (uint16 idx = 0; idx < owners.length; ++idx) {
            if (ownerConfirmations[owners[idx]])
                ++confirmedCount;
        }

        address[] memory confirmed = new address[](confirmedCount);
        
        for (uint16 idx2 = 0; idx2 < owners.length; ++idx2) {
            if (ownerConfirmations[owners[idx2]])
                confirmed[idx2] = owners[idx2];
        }
        
        return confirmed;
    }
  
    function getOwnersUnconfirmed() public view returns (address[] memory) {
        
        uint16 unconfirmedCount = 0;
        
        for (uint16 idx = 0; idx < owners.length; ++idx) {
            if (!ownerConfirmations[owners[idx]])
                ++unconfirmedCount;
        }

        address[] memory unconfirmed = new address[](unconfirmedCount);
        
        for (uint16 idx2 = 0; idx2 < owners.length; ++idx2) {
            if (!ownerConfirmations[owners[idx2]])
                unconfirmed[idx2] = owners[idx2];
        }
        
        return unconfirmed;
    }
  
    function hasConfirmed(address owner) public view returns (bool) {
        
        require(ownerWeights[owner] > 0, "The provided address does not belong to a registered owner.");
       
        return ownerConfirmations[owner];
    }

    function isExecutor(address candidate) public view returns (bool) {

        return executors[candidate];
    }
  
    function isTransactionConfirmed() public view returns (bool) {
        
        require(getMinScoreRequirement() > 0, "Minimum score has not yet been set.");
        
        require(owners.length > 0, "No owners have been provided.");
        
        return (getCurrentScore() >= minReqScoreForApproval);
    }

    function removeExecutor(address executor) public {

        executors[executor] = false;
    }
  
    function removeOwner(address owner) public {
        
        require(ownerWeights[owner] > 0, "The provided address does not belong to a registered owner.");
       
        ownerWeights[owner] = 0;
        revokeConfirmation(owner);
    }
  
    function revokeAllConfirmations() public returns (bool) {
        
        for (uint16 idx = 0; idx < owners.length; ++idx) {
            revokeConfirmation(owners[idx]);
        }

        return true;
    }
  
    function revokeConfirmation(address owner) public returns (bool) {
        
        require(ownerWeights[owner] > 0, "The provided address does not belong to a registered owner.");
        
        ownerConfirmations[owner] = false;
        return true;
    }
  
    function setMinScoreRequirement(uint newMinReqScore) public {
        
        minReqScoreForApproval = newMinReqScore;
    }

    function setExecutor(address executor) public {

        executors[executor] = true;
    }
  
    function setOwner(address owner, uint weight) public {
        
        require(owners.length < CONST_MAX_OWNERS, "The maximum number of owners has already been reached.");
        
        owners.push(owner);

        if (getMinScoreRequirement() < owners.length)
            setMinScoreRequirement(owners.length);
        
        ownerWeights[owner] = weight;
        
        revokeConfirmation(owner);
    }
}