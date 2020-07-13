// SPDX-License-Identifier: MIT
pragma solidity ^0.6.8;

library WonkaEngineStructs {

    /// @title Holds metadata which represents an Attribute (i.e., a unique point of data in a user's record)
    /// @author Aaron Kendall
    /// @notice Not all struct members are currently used
    struct WonkaAttr {
 
        uint attrId;

        bytes32 attrName;

        uint maxLength;

        bool maxLengthTruncate;

        uint maxNumValue;

        string defaultValue;

        bool isString;

        bool isDecimal;
        
        bool isNumeric;

        bool isValue;
    }

    /// @title A data structure that represents a Source (i.e., a provider of a record)
    /// @author Aaron Kendall
    /// @notice This structure isn't currently used
    struct WonkaSource {

        bytes32 sourceName;

        bytes32 status;

        // For retrieving an Attribute value
        bytes32 methodName;

        // For setting an Attribute value
        bytes32 setMethodName;
        
        address contractAddress;

        bool isValue;
    }

    /// @title Defines a rule (i.e., a logical unit for testing the validity of an Attribute value in a record)
    /// @author Aaron Kendall
    /// @notice 1.) Only one Attribute can be targeted now, but in the future, rules could be able to target multiple Attributes + 2.) A Rule can only be owned by one RuleSet
    /// @dev 
    struct WonkaRule {

        uint ruleId;

        bytes32 name;

        uint ruleType;

        WonkaAttr targetAttr;

        string ruleValue;

        mapping(string => string) ruleValueDomain;

        string[] ruleDomainKeys;

        bytes32[] customOpArgs;

        bytes32 parentRuleSetId;

        bool notOpFlag;

        bool isPassiveFlag;
    }

    /// @title Contains a list of all rules that have failed during a validation (and the rulesets to which they belong)
    /// @author Aaron Kendall
    /// @notice 
    /// @dev The arrays will be set to the total number of rules in a RuleTree, but 'ruleFailCount' will indicate how many of them are actually populated
    struct WonkaRuleReport {

        uint ruleFailCount;

        bytes32[] ruleSetIds;

        bytes32[] ruleIds;
    }

    /// @title Defines a ruleset (i.e., a logical grouping of rules)
    /// @author Aaron Kendall
    /// @notice A RuleSet can only be owned by one RuleTree
    /// @dev The collective evaluation of its rules will make a determination (such as how to navigate the RuleTree or whether or not the provided record is valid)  
    struct WonkaRuleSet {

        bytes32     ruleSetId;

        string      description;
        bytes32     parentRuleSetId;
        bool        severeFailure;
        // string      customFailureMsg;

        uint[]                           evalRuleList;
        mapping(uint => WonkaRule)       evaluativeRules;

        uint[]                           assertiveRuleList;
        mapping(uint => WonkaRule)       assertiveRules;

        bytes32[]                        childRuleSetList;

        bool        andOp;
        bool        failImmediately;
        bool        isLeaf;
        bool        isValue;
    }

    /// @title Defines a ruletree (i.e., a logical, hierarchical grouping of rulesets)
    /// @author Aaron Kendall
    /// @notice Currently, only one ruletree can be defined for any given address/account
    /// @dev The collective evaluation of its rulesets will determine whether or not the provided record is valid  
    struct WonkaRuleTree {

        bytes32     ruleTreeId;
        string      description;

        bytes32 rootRuleSetName;

        bytes32[]                        allRuleSetList;
        mapping(bytes32 => WonkaRuleSet) allRuleSets;

        uint totalRuleCount;

        bool isValue;
    }

}