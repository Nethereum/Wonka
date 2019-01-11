pragma solidity ^0.5.1;

/// @title An Ethereum library that contains information about all the instances of the Wonka rules engines in a blockchain
/// @author Aaron Kendall
/// @notice 
/// @dev 
contract TransactionStateInterface
{
  function addConfirmation(address psOwner) public;

  function clearPendingTransaction() public;

  function getCurrentScore() public view returns (uint);

  function getMinScoreRequirement() public view returns (uint);
  
  function getOwnersConfirmed() public view returns (address[] memory confirmed);
  
  function getOwnersUnconfirmed() public view returns (address[] memory unconfirmed);
  
  function hasConfirmed(address owner) public view returns (bool);
  
  function isTransactionConfirmed(address owner) public view returns (bool);
  
  function removeOwner(address owner) public;
  
  function revokeAllConfirmations() public view returns (bool);
  
  function revokeConfirmation(address owner) public returns (bool);
  
  function setMinScoreRequirement(uint newMinReqScore) public;
  
  function setOwner(address owner, uint8 weight) public;
}