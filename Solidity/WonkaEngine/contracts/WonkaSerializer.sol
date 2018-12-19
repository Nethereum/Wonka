pragma solidity ^0.5.1;

/// @title An Ethereum library that knows how to serialize a RuleTree into XML form (i.e., the Wonka rules markup)
/// @author Aaron Kendall
/// @notice 
/// @dev 
library WonkaSerializer {

    string constant CONST_RULE_TREE_ROOT_START = "<?xml version=\"1.0\"?>\n<RuleTree>\n";
    string constant CONST_RULE_TREE_ROOT_END   = "</RuleTree>";

    /// @dev This method will create the XML root node for the markup
    /// @author Aaron Kendall
    /// @notice 
    function writeRuleTreeRootStart(bytes32 ruleTreeId, string memory desc) public pure returns (string memory rootStart)  {

        // require(ruleTreeId != "", "Blank TreeID has been provided.");

        bytes32 dummyValBytes32 = "";
        string memory dummyValString = "";
        dummyValBytes32 = ruleTreeId;
        dummyValString = desc;

        rootStart = CONST_RULE_TREE_ROOT_START;
    }

    /// @dev This method will finish the XML root node for the markup
    /// @author Aaron Kendall
    /// @notice 
    function writeRuleTreeRootEnd() public pure returns (string memory rootEnd)  {

        rootEnd = CONST_RULE_TREE_ROOT_END;
    }

    /***********************
    *   SUPPORT METHODS   *
    ***********************/

 	/// @dev This method will concatenate the provided strings into one larger string
	/// @notice 
    function strConcat(string memory _a, string memory _b, string memory _c, string memory _d, string memory _e) private pure returns (string memory) {

        bytes memory _ba = bytes(_a);
        bytes memory _bb = bytes(_b);
        bytes memory _bc = bytes(_c);
        bytes memory _bd = bytes(_d);
        bytes memory _be = bytes(_e);
        string memory abcde = new string(_ba.length + _bb.length + _bc.length + _bd.length + _be.length);
        bytes memory babcde = bytes(abcde);
        
        uint k = 0;
        
        for (uint a = 0; a < _ba.length; a++) {
            babcde[k++] = _ba[a];
        }

        for (uint b = 0; b < _bb.length; b++) {
            babcde[k++] = _bb[b];
        }

        for (uint c = 0; c < _bc.length; c++) {
            babcde[k++] = _bc[c];
        } 

        for (uint d = 0; d < _bd.length; d++) {
            babcde[k++] = _bd[d];
        }

        for (uint e = 0; e < _be.length; e++) { 
            babcde[k++] = _be[e];
        }

        return string(babcde);
    }

 	/// @dev This method will concatenate the provided strings into one larger string
	/// @notice 
    function strConcat(string memory _a, string memory _b, string memory _c) private pure returns (string memory) {
        return strConcat(_a, _b, _c, "", "");
    }

 	/// @dev This method will concatenate the provided strings into one larger string
	/// @notice 
    function strConcat(string memory _a, string memory _b) private pure returns (string memory) {
        return strConcat(_a, _b, "", "", "");
    }

    /// @dev This method will convert a 'string' type to a 'bytes32' type
    /// @notice 
    function stringToBytes32(string memory source) private pure returns (bytes32 result) {
        bytes memory tempEmptyStringTest = bytes(source);
        if (tempEmptyStringTest.length == 0) {
            return 0x0;
        }

        assembly {
            result := mload(add(source, 32))
        }
        
    }

}