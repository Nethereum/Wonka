pragma solidity ^0.5.1;

/// @title An Ethereum library that knows how to serialize a RuleTree into XML form (i.e., the Wonka rules markup)
/// @author Aaron Kendall
/// @notice 
/// @dev 
library WonkaSerializer {

    string constant CONST_CRITERIA_START       = "<criteria ";
    string constant CONST_RULE_START           = "<eval id=\"";
    string constant CONST_RULE_TREE_ROOT_START = "<?xml version=\"1.0\"?>\n<RuleTree>\n";
    string constant CONST_RULE_SET_NODE_START  = "<if ";
    string constant CONST_VALIDATE_START       = "<validate err=\"";

    string constant CONST_CRITERIA_END         = "</criteria>";
    string constant CONST_RULE_END             = "</eval>";
    string constant CONST_RULE_TREE_ROOT_END   = "</RuleTree>";
    string constant CONST_RULE_SET_NODE_END    = "</if>\n";
    string constant CONST_VALIDATE_END         = "</validate>";
    
    // An enum for the type of rules currently supported
    enum RuleTypes { IsEqual, IsLessThan, IsGreaterThan, Populated, InDomain, Assign, OpAdd, OpSub, OpMult, OpDiv, CustomOp, MAX_TYPE }

    /// @dev This method will create the starting XML markup for a Criteria node
    /// @author Aaron Kendall
    /// @notice 
    function writeCriteriaStart(bool andOp) public pure returns (string memory criteriaStart)  {        

        string memory operator = "";
        
        if (andOp)
            operator="AND";
        else
            operator="OR";
        
        criteriaStart = strConcat(CONST_CRITERIA_START, "op=\"", operator, "\"");
        criteriaStart = strConcat(criteriaStart, ">\n");
    }
    
    /// @dev This method will create the end XML markup for a Criteria node
    /// @author Aaron Kendall
    /// @notice 
    function writeCriteriaEnd() public pure returns (string memory criteriaEnd)  {        

        criteriaEnd = CONST_CRITERIA_END;
    }
    
    // <eval id="cmp2">(N.NewSalePrice) GT (0.00)</eval>
    
    /// @dev This method will create the starting XML markup for a Criteria node
    /// @author Aaron Kendall
    /// @notice 
    function writeRule(string memory ruleId, bytes32 attribute, uint8 ruleOpVal, string memory attrVal, bool isAttrValStr, bool notOp) public pure returns (string memory ruleBody)  {

        string memory ruleOpName = "";
        string memory attrName = bytes32ToString(attribute);
        
        if (ruleOpVal == uint8(RuleTypes.IsEqual))
            ruleOpName = "==";
        else if (ruleOpVal == uint8(RuleTypes.IsLessThan))
            ruleOpName = "LT";
        else if (ruleOpVal == uint8(RuleTypes.IsGreaterThan))
            ruleOpName = "GT";
        else if (ruleOpVal == uint8(RuleTypes.Populated))
            ruleOpName = "POPULATED";
        else if (ruleOpVal == uint8(RuleTypes.InDomain))
            ruleOpName = "IN";
        else if (ruleOpVal == uint8(RuleTypes.Assign))
            ruleOpName = "ASSIGN";
        else if (ruleOpVal == uint8(RuleTypes.OpAdd))
            ruleOpName = "ASSIGN_SUM";
        else if (ruleOpVal == uint8(RuleTypes.OpSub))
            ruleOpName = "ASSIGN_DIFF";
        else if (ruleOpVal == uint8(RuleTypes.OpMult))
            ruleOpName = "ASSIGN_PROD";
        else if (ruleOpVal == uint8(RuleTypes.OpDiv))
            ruleOpName = "ASSIGN_QUOT";
        else
            ruleOpName = "==";
            
        ruleBody = strConcat(CONST_RULE_START, ruleId, "\" (");
        ruleBody = strConcat(ruleBody, attrName, ") ");
        
        if (notOp)
            ruleBody = strConcat(ruleBody, "NOT ");
        
        ruleBody = strConcat(ruleBody, ruleOpName, " (");
        
        // If the rule is a binary operator, then we need to include the provided value
        if (ruleOpVal != uint8(RuleTypes.Populated)) {
    
            if (isAttrValStr)
                ruleBody = strConcat(ruleBody, "'", attrVal, "')", CONST_RULE_END);
            else
                ruleBody = strConcat(ruleBody, attrVal, ")", CONST_RULE_END);
        }
        
        return ruleBody;
    }

    /// @dev This method will create the starting XML markup for a Validate node
    /// @author Aaron Kendall
    /// @notice 
    function writeRuleSetLeafStart(bool severeMode) public pure returns (string memory validateStart)  {

        string memory mode = "";
        
        if (severeMode)
            mode ="severe";
        else
            mode = "warning";
        
        validateStart = strConcat(CONST_VALIDATE_START, mode, "\"");
        validateStart = strConcat(validateStart, ">\n");
    }
    
    /// @dev This method will create the end XML markup for a Validate node
    /// @author Aaron Kendall
    /// @notice 
    function writeRuleSetLeafEnd() public pure returns (string memory validateEnd)  {        

        validateEnd = CONST_VALIDATE_END;
    }

    /// @dev This method will create the starting XML markup for a RuleSet node
    /// @author Aaron Kendall
    /// @notice 
    function writeRuleSetNodeStart(string memory desc) public pure returns (string memory nodeStart)  {        

        nodeStart = strConcat(CONST_RULE_SET_NODE_START, "description=\"", desc, "\"");
        nodeStart = strConcat(nodeStart, ">\n");
    }

    /// @dev This method will create the ending XML markup for a RuleSet node
    /// @author Aaron Kendall
    /// @notice 
    function writeRuleSetNodeEnd() public pure returns (string memory nodeEnd)  {        

        nodeEnd = CONST_RULE_SET_NODE_END;
    }

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
    
    /// @dev This method will convert a bytes32 type to a String
    /// @notice 
    function bytes32ToString(bytes32 x) public pure returns (string memory) {

        bytes memory bytesString = new bytes(32);
        uint charCount = 0;
        for (uint j = 0; j < 32; j++) {
            byte char = byte(bytes32(uint(x) * 2 ** (8 * j)));
            if (char != 0) {
                bytesString[charCount] = char;
                charCount++;
            }
        }

        bytes memory bytesStringTrimmed = new bytes(charCount);
        for (uint k = 0; k < charCount; k++) {
            bytesStringTrimmed[k] = bytesString[k];
        }

        return string(bytesStringTrimmed);
    }

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
    function strConcat(string memory _a, string memory _b, string memory _c, string memory _d) private pure returns (string memory) {
        return strConcat(_a, _b, _c, _d, "");
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