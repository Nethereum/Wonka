// SPDX-License-Identifier: MIT
pragma solidity ^0.6.8;

/// @title An Ethereum library that contains information about all the instances of the Wonka rules engines in a blockchain
/// @author Aaron Kendall
/// @notice 
/// @dev 
interface TransactionStateInterface
{
    function addConfirmation(address psOwner) external;

    function clearPendingTransaction() external;

    function getCurrentScore() external view returns (uint);

    function getMinScoreRequirement() external view returns (uint);
  
    function getOwnersConfirmed() external view returns (address[] memory);
  
    function getOwnersUnconfirmed() external view returns (address[] memory);
  
    function hasConfirmed(address owner) external view returns (bool);

    function isExecutor(address candidate) external view returns (bool);
  
    function isTransactionConfirmed() external view returns (bool);

    function removeExecutor(address executor) external;
  
    function removeOwner(address owner) external;
  
    function revokeAllConfirmations() external returns (bool);
  
    function revokeConfirmation(address owner) external returns (bool);

    function setExecutor(address executor) external;
  
    function setMinScoreRequirement(uint newMinReqScore) external;
  
    function setOwner(address owner, uint weight) external;
}