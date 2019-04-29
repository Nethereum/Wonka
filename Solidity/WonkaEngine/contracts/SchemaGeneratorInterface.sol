pragma solidity ^0.5.0;

/// @title An interface for classes that want to replicate an existing schema within their internal storage
/// @author Aaron Kendall
/// @notice
/// @dev
interface SchemaGeneratorInterface
{
    /**
     ** Expecting ABI types like "uint256", "bytes32", "bool", and "string"
     **/
    function addSchemaCol(bytes32 abiName, bytes32 abiType, uint maxLength, uint minValue, uint maxValue) external returns (bool);
	
	function addFunction(bytes32 abiName, bytes32[] abiParamNames, bytes32[] abiParamTypes, bytes32[] abiReturnTypes) external returns (bool);
}
