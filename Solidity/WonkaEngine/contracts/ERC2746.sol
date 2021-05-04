// SPDX-License-Identifier: MIT
pragma solidity ^0.7.6;

/**
    @title ERC-2746 Rules Engine Standard
    @dev See https://eips.ethereum.org/EIPS/eip-2746 (though this interface is a slight variation of the EIP)
 */
interface ERC2746 {

    /**
        @dev Should emit when a RuleTree is invoked.
        The `ruler` is the ID and owner of the RuleTree being invoked.  It is also likely msg.sender.
    */
    event CallRuleTree(
        address indexed ruler
    );

    /**
        @dev Should emit when a RuleSet is invoked.
        The `ruler` is the ID and owner of the RuleTree in which the RuleSet is stored.  It is also likely msg.sender.
        The 'ruleSetId' is the ID of the RuleSet being invoked.
    */
    event CallRuleSet(
        address indexed ruler,
        bytes32 indexed tmpRuleSetId
    );

    /**
        @dev Should emit when a Rule is invoked.
        The `ruler` is the ID and owner of the RuleTree in which the RuleSet is stored.  It is also likely msg.sender.
        The 'ruleSetId' is the ID of the RuleSet being invoked.
        The 'ruleId' is the ID of the Rule being invoked.
        The 'ruleType' is the type of the rule being invoked.        
    */
    event CallRule(
        address indexed ruler,
        bytes32 indexed ruleSetId,
        bytes32 indexed ruleId,
        uint ruleType
    );

    /**
        @dev Should emit when a RuleSet fails.
        The `ruler` is the ID and owner of the RuleTree in which the RuleSet is stored.  It is also likely msg.sender.
        The 'ruleSetId' is the ID of the RuleSet being invoked.
        The 'severeFailure' is the indicator of whether or not the RuleSet is a leaf with a 'severe' error flag.
    */
    event RuleSetError (
        address indexed ruler,
        bytes32 indexed ruleSetId,
        bool severeFailure
    );	

    /**
        @notice Adds a new Attribute to the data domain.
        @dev Caller should be the deployer/owner of the rules engine contract.  An Attribute value can be an optional alternative if it's not a string or numeric.
        @param _attrName    Name/ID of the Attribute
        @param _maxLen      Maximum length of the Attribute (if it is a string)
        @param _maxNumVal   Maximum numeric value of the Attribute (if it is numeric)
        @param _defaultVal  The default value for the Attribute
        @param _isString    Indicator of whether or not the Attribute is a string
        @param _isNumeric   Indicator of whether or not the Attribute is numeric
    */    
    function addAttribute(bytes32 _attrName, uint _maxLen, uint _maxNumVal, string calldata _defaultVal, bool _isString, bool _isNumeric) external;

    /**
        @notice Adds a new RuleTree.
        @param _owner          Owner/ID of the RuleTree
        @param _ruleTreeName   Name of the RuleTree
        @param _desc           Verbose description of the RuleTree's purpose
    */
    function addRuleTree(address _owner, bytes32 _ruleTreeName, string calldata _desc, bool _severeFailureFlag, bool _useAndOperator, bool _flagFailImmediately) external;

    /**
        @notice Adds a new RuleSet onto the hierarchy of a RuleTree.
        @dev RuleSets can have child RuleSets, but they will only be called if the parent's Rules execute to create boolean 'true'.
        @param _owner           Owner/ID of the RuleTree
        @param _ruleSetName     ID/Name of the RuleSet
        @param _desc            Verbose description of the RuleSet
        @param _parentRSName    ID/Name of the parent RuleSet, to which this will be added as a child
        @param _severalFailFlag Indicator of whether or not the RuleSet's execution (as failure) will result in a failure of the RuleTree.  (This flag only applies to leaves in the RuleTree.)
        @param _useAndOp        Indicator of whether or not the rules in the RuleSet will execute with 'AND' between them.  (Otherwise, it will be 'OR'.)
        @param _failQuickFlag   Indicator of whether or not the RuleSet's execution (as failure) should immediately stop the RuleTree.
    */    
    function addRuleSet(address _owner, bytes32 _ruleSetName, string calldata _desc, bytes32 _parentRSName, bool _severalFailFlag, bool _useAndOp, bool _failQuickFlag) external;

    /**
        @notice Adds a new Rule into a RuleSet.
        @dev Rule types can be implemented as any type of action (greater than, less than, etc.)
        @param _owner           Owner/ID of the RuleTree
        @param _ruleSetName     ID/Name of the RuleSet to which the Rule will be added
        @param _ruleName        ID/Name of the Rule being added
        @param _attrName        ID/Name of the Attribute upon which the Rule is invoked
        @param _ruleType        ID of the type of Rule
        @param _rightHandValue  The registered value to be used by the Rule when performing its action upon the Attribute
        @param _notFlag         Indicator of whether or not the NOT operator should be performed on this Rule.
    */    
    function addRule(address _owner, bytes32 _ruleSetName, bytes32 _ruleName, bytes32 _attrName, uint _ruleType, string calldata _rightHandValue, bool _notFlag, bool _passiveFlag) external;

    /**
        @notice Executes a RuleTree.
        @param _owner           Owner/ID of the RuleTree
    */
    function executeRuleTree(address _owner) external returns (bool);

    /**
        @notice Retrieves the properties of a Rule.
        @param _owner           Owner/ID of the RuleTree
        @param _ruleSetName     ID/Name of the RuleSet where the Rule resides
        @param _ruleIdx         Index of the rule in the RuleSet's listing 
        @return bytes32         ID/Name of Rule
        @return uint            Type of Rule
        @return bytes32         Target Attribute of Rule
        @return string          Value mentioned in Rule
        @return bool            Flag for NOT operator in Rule
        @return bytes32[]       Values that should be provided in delegated call (if Rule is custom operator)
    */
    function getRuleProps(address _owner, bytes32 _ruleSetName, bool _evalRuleFlag, uint _ruleIdx) external returns (bytes32, uint, bytes32, string memory, bool, bytes32[] memory);

    /**
        @notice Retrieves the properties of a RuleSet
        @param _owner        Owner/ID of the RuleTree
        @param _ruleSetName  ID/Name of the RuleSet
        @return string       Verbose description of the RuleSet
        @return bool         Flag that indicates whether this RuleSet's failure (if a leaf) will cause the RuleTree to fail
        @return bool         Flag that indicates whether this RuleSet uses the AND operator when executing rules collectively
        @return uint         Indicates the number of rules hosted by this RuleSet
        @return uint         The length of the list of RuleSets that are children of this RuleSet
    */
    function getRuleSetProps(address _owner, bytes32 _ruleSetName) external returns (string memory, bool, bool, uint, uint, uint);

    /**
        @notice Retrieves the properties of a RuleSet
        @param _owner        Owner/ID of the RuleTree
        @return bytes32      Name of the RuleTree
        @return string       Verbose description of the RuleTree
        @return bytes32      ID/Name of the RuleSet that serves as the root node for the RuleTree
    */
    function getRuleTreeProps(address _owner) external returns (bytes32, string memory, bytes32);

    /**
        @notice Retrieves the value of a field on the current logical record
        @param _owner        Owner/ID of the RuleTree
        @param _key          Name of the field
        @return string       Value of the field
    */
    function getValueOnRecord(address _owner, bytes32 _key) external returns(string memory);

    /**
        @notice Removes a RuleTree.
        @param _owner           Owner/ID of the RuleTree
    */
    function removeRuleTree(address _owner) external returns (bool);    

    /**
        @notice Sets the value of a field on the current logical record
        @param _owner        Owner/ID of the RuleTree
        @param _key          Name of the field
        @param _value        Value of the field
    */
    function setValueOnRecord(address _owner, bytes32 _key, string calldata _value) external returns(string memory);

}