using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using Wonka.BizRulesEngine.RuleTree;
using Wonka.BizRulesEngine.RuleTree.RuleTypes;
using WonkaRef;

namespace Wonka.BizRulesEngine.Readers
{
    /// <summary>
    /// 
    /// This class will parse a Wonka-Bre XML file, translating and loading its contents 
    /// into a RuleTree for our rules engine.
    /// 
    /// NOTE: In general, this first iteration of the parsing is basic and not entirely
    /// robust, especially with the case of parsing the rule expressions.
    /// 
    /// </summary>
    public class WonkaBizRulesXmlReader
    {
        #region Delegates

        public delegate string ExecuteCustomOperator(string psArg1, string psArg2, string psArg3, string psArg4);

        #endregion

        #region CONSTANTS

        public const string CONST_RS_FLOW_TAG       = "if";
        public const string CONST_RS_FLOW_DESC_ATTR = "description";

        public const string CONST_RS_VALID_TAG         = "validate";
        public const string CONST_RS_VALID_ERR_ATTR    = "err";
        public const string CONST_RS_VALID_ERR_WARNING = "warning";
        public const string CONST_RS_VALID_ERR_SEVERE  = "severe";

        public const string CONST_RS_FAIL_MSG_TAG  = "failure_message";
        public const string CONST_RS_CUSTOM_ID_TAG = "customId";

        public const string CONST_RULES_TAG     = "criteria";
        public const string CONST_RULES_OP_ATTR = "op";

        public const string CONST_RULE_TAG      = "eval";
        public const string CONST_RULE_ID_ATTR  = "id";

        public const string CONST_RULE_TOKEN_START_DELIM = "(";
        public const string CONST_RULE_TOKEN_END_DELIM   = ")";
        public const string CONST_RULE_TOKEN_VAL_DELIM   = ",";

        public const string CONST_BASIC_OP_NOT_POP     = "NOT POPULATED";
        public const string CONST_BASIC_OP_POP         = "POPULATED";
        public const string CONST_BASIC_OP_NOT_EQ      = "!=";
        public const string CONST_BASIC_OP_EQ          = "==";
        public const string CONST_BASIC_OP_NOT_IN      = "NOT IN";
        public const string CONST_BASIC_OP_IN          = "IN";
        public const string CONST_BASIC_OP_EXISTS_AS   = "EXISTS AS";
        public const string CONST_BASIC_OP_DEFAULT     = "DEFAULT";
        public const string CONST_BASIC_OP_ASSIGN_SUM  = "ASSIGN_SUM";
        public const string CONST_BASIC_OP_ASSIGN_DIFF = "ASSIGN_DIFF";
        public const string CONST_BASIC_OP_ASSIGN_PROD = "ASSIGN_PROD";
        public const string CONST_BASIC_OP_ASSIGN_QUOT = "ASSIGN_QUOT";
        public const string CONST_BASIC_OP_ASSIGN      = "ASSIGN";

        public const string CONST_AL_GT     = "GT";
        public const string CONST_AL_NOT_GT = "NOT GT";
        public const string CONST_AL_LT     = "LT";
        public const string CONST_AL_NOT_LT = "NOT LT";
        public const string CONST_AL_GE     = "GE";
        public const string CONST_AL_NOT_GE = "NOT GE";
        public const string CONST_AL_LE     = "LE";
        public const string CONST_AL_NOT_LE = "NOT LE";
        public const string CONST_AL_EQ     = "EQ";
        public const string CONST_AL_NOT_EQ = "NOT EQ";

        public const string CONST_DL_IB     = "IS BEFORE";
        public const string CONST_DL_NOT_IB = "IS NOT BEFORE";
        public const string CONST_DL_IA     = "IS AFTER";
        public const string CONST_DL_NOT_IA = "IS NOT AFTER";
        public const string CONST_DL_AROUND = "IS AROUND";
        public const string CONST_DL_ALMOST = "IS ALMOST";

        public const string CONST_RS_VALID_SUCCESS_MSG = "EQ";
        public const string CONST_RS_VALID_FAILURE_MSG = "NOT EQ";

        #endregion

        #region Constructors

        public WonkaBizRulesXmlReader(string psBreXmlFilepath, IMetadataRetrievable piMetadataSource = null, WonkaBizRulesEngine poRulesHostEngine = null)
        {
            if (string.IsNullOrEmpty(psBreXmlFilepath))
            {
                throw new WonkaBizRuleException(-1, -1, "ERROR!  The rules file provided is null.");
            }

            if (!File.Exists(psBreXmlFilepath))
            {
                throw new WonkaBizRuleException(-1, -1, "ERROR!  The rules file(" + psBreXmlFilepath + ") does not exist.");
            }

            this.BreXmlFilepath  = psBreXmlFilepath;
            this.BreXmlContents  = null;
            this.RulesHostEngine = poRulesHostEngine;

            this.Init(piMetadataSource);
        }

        public WonkaBizRulesXmlReader(StringBuilder psBreXml, IMetadataRetrievable piMetadataSource = null, WonkaBizRulesEngine poRulesHostEngine = null)
        {
            if ((psBreXml == null) || (psBreXml.Length <= 0))
            {
                throw new WonkaBizRuleException(-1, -1, "ERROR!  The rules file provided is null.");
            }

            this.BreXmlFilepath  = null;
            this.BreXmlContents  = psBreXml.ToString();
            this.RulesHostEngine = poRulesHostEngine;

            this.Init(piMetadataSource);
        }

        // NOTE: That says "po-Op-Source", but if you want to look at it as "poOp-Source", well, that's up to you,
        // and try not laugh yourself silly
        public void AddCustomOperator(string psCustomOpName, WonkaBizSource poOpSource = null)
        {
            if (this.BasicOps.Contains(psCustomOpName))
            {
                throw new Exception("ERROR!  Provided operator is already a basic operator within the rules engine.");
            }

            if (this.ArithmeticLimitOps.Contains(psCustomOpName))
            {
                throw new Exception("ERROR!  Provided operator is already an arithmetic limit operator within the rules engine.");
            }

            if (this.DateLimitOps.Contains(psCustomOpName))
            {
                throw new Exception("ERROR!  Provided operator is already a date limit operator within the rules engine.");
            }

            if (poOpSource != null)
            {
                this.CustomOpSources[psCustomOpName] = poOpSource;
            }
            else
            {
                this.CustomOpSources[psCustomOpName] = new WonkaBizSource("", "", "", "", "", "", "", null);
            }
        }

        private void Init(IMetadataRetrievable piMetadataSource)
        {
            this.RuleSetIdCounter  = 0;
            this.RuleIdCounter     = 0;
            this.ValSeqIdCounter   = 0;
            this.CustomOpSources   = new Dictionary<string, WonkaBizSource>();
            this.AllParsedRuleSets = new List<WonkaBizRuleSet>();

            this.BasicOps = new HashSet<string>();
            this.BasicOps.Add(CONST_BASIC_OP_NOT_POP);
            this.BasicOps.Add(CONST_BASIC_OP_POP);
            this.BasicOps.Add(CONST_BASIC_OP_NOT_EQ);
            this.BasicOps.Add(CONST_BASIC_OP_EQ);
            this.BasicOps.Add(CONST_BASIC_OP_NOT_IN);
            this.BasicOps.Add(CONST_BASIC_OP_IN);
            this.BasicOps.Add(CONST_BASIC_OP_EXISTS_AS);
            this.BasicOps.Add(CONST_BASIC_OP_DEFAULT);
            this.BasicOps.Add(CONST_BASIC_OP_ASSIGN_SUM);
            this.BasicOps.Add(CONST_BASIC_OP_ASSIGN_DIFF);
            this.BasicOps.Add(CONST_BASIC_OP_ASSIGN_PROD);
            this.BasicOps.Add(CONST_BASIC_OP_ASSIGN);

            this.ArithmeticLimitOps = new HashSet<string>();
            this.ArithmeticLimitOps.Add(CONST_AL_GT);
            this.ArithmeticLimitOps.Add(CONST_AL_NOT_GT);
            this.ArithmeticLimitOps.Add(CONST_AL_LT);
            this.ArithmeticLimitOps.Add(CONST_AL_NOT_LT);
            this.ArithmeticLimitOps.Add(CONST_AL_GE);
            this.ArithmeticLimitOps.Add(CONST_AL_NOT_GE);
            this.ArithmeticLimitOps.Add(CONST_AL_LE);
            this.ArithmeticLimitOps.Add(CONST_AL_NOT_LE);
            this.ArithmeticLimitOps.Add(CONST_AL_EQ);
            this.ArithmeticLimitOps.Add(CONST_AL_NOT_EQ);

            this.DateLimitOps = new HashSet<string>();
            this.DateLimitOps.Add(CONST_DL_IB);
            this.DateLimitOps.Add(CONST_DL_NOT_IB);
            this.DateLimitOps.Add(CONST_DL_IA);
            this.DateLimitOps.Add(CONST_DL_NOT_IA);
            this.DateLimitOps.Add(CONST_DL_ALMOST);

            // NOTE: Will be implemented later, with a defined plan
            // DateLimitOps.Add(CONST_DL_AROUND);

            if (piMetadataSource != null)
            {
                try
                {
                    WonkaRefEnvironment.GetInstance();
                }
                catch (Exception ex)
                {
                    WonkaRefEnvironment.CreateInstance(false, piMetadataSource);
                }
            }
        }

        #endregion

        #region Methods

        public WonkaBizRuleSet ParseRuleTree()
        {
            WonkaBizRuleSet newRootRuleSet = new WonkaBizRuleSet();

            newRootRuleSet.RuleSetId   = ++this.RuleSetIdCounter;
            newRootRuleSet.Description = "Root";

            this.RootRuleSet = newRootRuleSet;
            this.AllParsedRuleSets.Add(newRootRuleSet);

            XmlDocument xmlDoc = new XmlDocument();
            if (this.BreXmlFilepath != null)
            {
                xmlDoc.Load(this.BreXmlFilepath);
            }
            else
            {
                using (var ruleReader = new StringReader(this.BreXmlContents))
                {
                    xmlDoc.Load(ruleReader);
                }
            }

            XmlNode rootNode = xmlDoc.LastChild;

            XmlNodeList firstTierList = rootNode.ChildNodes;
            foreach (XmlNode firstTierNode in firstTierList)
            {
                if (firstTierNode.LocalName == CONST_RS_FLOW_TAG)
                {
                    WonkaBizRuleSet newChildRuleSet = this.ParseRuleSet(firstTierNode);

                    newChildRuleSet.ParentRuleSetId = newRootRuleSet.RuleSetId;

                    newRootRuleSet.AddChildRuleSet(newChildRuleSet);
                }
            }

            return newRootRuleSet;
        }

        private WonkaBizRuleSet ParseRuleSet(XmlNode poRuleSetXmlNode, bool pbLeafNode = false)
        {
            WonkaBizRuleSet currentRuleSet = new WonkaBizRuleSet(++(this.RuleSetIdCounter));

            AllParsedRuleSets.Add(currentRuleSet);

            var AttrDesc = poRuleSetXmlNode.Attributes.GetNamedItem(CONST_RS_FLOW_DESC_ATTR);
            if (AttrDesc != null)
                currentRuleSet.Description = AttrDesc.Value;

            XmlNodeList ChildNodeList = poRuleSetXmlNode.ChildNodes;
            foreach (XmlNode TempChildXmlNode in ChildNodeList)
            {
                if ( (TempChildXmlNode.LocalName == CONST_RS_FLOW_TAG) || 
                     (TempChildXmlNode.LocalName == CONST_RS_VALID_TAG) )
                {
                    bool bIsLeafNode = (TempChildXmlNode.LocalName == CONST_RS_VALID_TAG);

                    WonkaBizRuleSet NewChildRuleSet = ParseRuleSet(TempChildXmlNode, bIsLeafNode);

                    if (bIsLeafNode)
                    {
                        var AttrErrLevel = TempChildXmlNode.Attributes.GetNamedItem(CONST_RS_VALID_ERR_ATTR);
                        if (AttrErrLevel != null)
                        {
                            if (AttrErrLevel.Value.ToLower() == CONST_RS_VALID_ERR_WARNING)
                                NewChildRuleSet.ErrorSeverity = RULE_SET_ERR_LVL.ERR_LVL_WARNING;
                            else if (AttrErrLevel.Value.ToLower() == CONST_RS_VALID_ERR_SEVERE)
                                NewChildRuleSet.ErrorSeverity = RULE_SET_ERR_LVL.ERR_LVL_SEVERE;
                        }
                    }

                    NewChildRuleSet.ParentRuleSetId = currentRuleSet.RuleSetId;

                    currentRuleSet.AddChildRuleSet(NewChildRuleSet);
                }
                else if (TempChildXmlNode.LocalName == CONST_RULES_TAG)
                {
                    ParseRules(TempChildXmlNode, currentRuleSet);
                }
                else if (pbLeafNode && (TempChildXmlNode.LocalName == CONST_RS_FAIL_MSG_TAG))
                {
                    currentRuleSet.CustomFailureMsg = TempChildXmlNode.InnerText;
                }
                else if (pbLeafNode && (TempChildXmlNode.LocalName == CONST_RS_CUSTOM_ID_TAG))
                {
                    currentRuleSet.CustomId = TempChildXmlNode.InnerText;
                }
            }

            return currentRuleSet;
        }

        private void ParseRules(XmlNode poTargetXmlNode, WonkaBizRuleSet poTargetRuleSet)
        {
            var OpDesc = poTargetXmlNode.Attributes.GetNamedItem(CONST_RULES_OP_ATTR);
            if (OpDesc != null)
            {
                if (OpDesc.Value.ToLower() == "and")
                    poTargetRuleSet.RulesEvalOperator = RULE_OP.OP_AND;
                else if (OpDesc.Value.ToLower() == "or")
                    poTargetRuleSet.RulesEvalOperator = RULE_OP.OP_OR;
            }

            XmlNodeList ChildNodeList = poTargetXmlNode.ChildNodes;
            foreach (XmlNode TempChildXmlNode in ChildNodeList)
            {
                if (TempChildXmlNode.LocalName == CONST_RULE_TAG)
                    ParseSingleRule(TempChildXmlNode, poTargetRuleSet);
            }
        }

        private void ParseSingleRule(XmlNode poRuleXmlNode, WonkaBizRuleSet poTargetRuleSet)
        {
            int        nNewRuleId      = ++(this.RuleIdCounter);
            string     sRuleExpression = poRuleXmlNode.InnerText;
            WonkaBizRule NewRule         = null;

            if (this.CustomOpSources.Keys.Any(s => sRuleExpression.Contains(s)))
                NewRule = new CustomOperatorRule() { RuleId = nNewRuleId };
            else if (this.ArithmeticLimitOps.Any(s => sRuleExpression.Contains(s)))
                NewRule = new ArithmeticLimitRule() { RuleId = nNewRuleId };
            else if (this.DateLimitOps.Any(s => sRuleExpression.Contains(s)))
                NewRule = new DateLimitRule() { RuleId = nNewRuleId };
            else if (sRuleExpression.Contains("NOT POPULATED"))
                NewRule = new PopulatedRule() { RuleId = nNewRuleId, NotOperator = true };
            else if (sRuleExpression.Contains("POPULATED"))
                NewRule = new PopulatedRule() { RuleId = nNewRuleId, NotOperator = false };
            else if (sRuleExpression.Contains("!="))
                NewRule = new DomainRule() { RuleId = nNewRuleId, NotOperator = true };
            else if (sRuleExpression.Contains("=="))
                NewRule = new DomainRule() { RuleId = nNewRuleId, NotOperator = false };
            else if (sRuleExpression.Contains("NOT IN"))
                NewRule = new DomainRule() { RuleId = nNewRuleId, NotOperator = true };
            else if (sRuleExpression.Contains("IN"))
                NewRule = new DomainRule() { RuleId = nNewRuleId, NotOperator = false };
            else if (sRuleExpression.Contains("EXISTS AS"))
                NewRule = new DomainRule() { RuleId = nNewRuleId, NotOperator = false, SearchAllDataRows = true };
            else if (sRuleExpression.Contains("DEFAULT"))
                NewRule = new AssignmentRule() { RuleId = nNewRuleId, NotOperator = false, DefaultAssignment = true };
            else if (sRuleExpression.Contains("ASSIGN_SUM"))
                NewRule = new ArithmeticRule() { RuleId = nNewRuleId, NotOperator = false, OpType = ARITH_OP_TYPE.AOT_SUM };
            else if (sRuleExpression.Contains("ASSIGN_DIFF"))
                NewRule = new ArithmeticRule() { RuleId = nNewRuleId, NotOperator = false, OpType = ARITH_OP_TYPE.AOT_DIFF };
            else if (sRuleExpression.Contains("ASSIGN_PROD"))
                NewRule = new ArithmeticRule() { RuleId = nNewRuleId, NotOperator = false, OpType = ARITH_OP_TYPE.AOT_PROD };
            else if (sRuleExpression.Contains("ASSIGN_QUOT"))
                NewRule = new ArithmeticRule() { RuleId = nNewRuleId, NotOperator = false, OpType = ARITH_OP_TYPE.AOT_QUOT };
            else if (sRuleExpression.Contains("ASSIGN"))
                NewRule = new AssignmentRule() { RuleId = nNewRuleId, NotOperator = false };

            if (NewRule != null)
            {
                var RuleId = poRuleXmlNode.Attributes.GetNamedItem(CONST_RULE_ID_ATTR);
                if (RuleId != null)
                    NewRule.DescRuleId = RuleId.Value;

                NewRule.ParentRuleSetId = poTargetRuleSet.RuleSetId;

                SetTargetAttribute(NewRule, sRuleExpression);

                if (NewRule.RuleType != RULE_TYPE.RT_POPULATED)
                    SetRuleValues(NewRule, sRuleExpression);

                if (RulesHostEngine != null)
                {
                    NewRule.RulesHostEngine = RulesHostEngine;

                    if (RulesHostEngine.StdOpMap != null)
                    {
                        if ((NewRule is ArithmeticLimitRule) && RulesHostEngine.StdOpMap.ContainsKey(STD_OP_TYPE.STD_OP_BLOCK_NUM))
                            ((ArithmeticLimitRule)NewRule).BlockNumDelegate = RulesHostEngine.StdOpMap[STD_OP_TYPE.STD_OP_BLOCK_NUM];
                    }
                }
            }

            if (NewRule != null)
                poTargetRuleSet.AddRule(NewRule);
        }

        private void SetRuleValues(WonkaBizRule poTargetRule, string psRuleExpression)
        {
            char[] acRuleValuesDelim = new char[1] { ',' };

            int nValueStartIdx = psRuleExpression.LastIndexOf(CONST_RULE_TOKEN_START_DELIM);
            if (nValueStartIdx >= 0)
            {
                int nValueEndIdx =
                    psRuleExpression.IndexOf(CONST_RULE_TOKEN_END_DELIM, nValueStartIdx + 1);

                if (nValueEndIdx > 0)
                {
                    string sValues =
                        psRuleExpression.Substring(nValueStartIdx + 1, (nValueEndIdx - nValueStartIdx - 1));

                    string[] asValueSet = sValues.Split(acRuleValuesDelim);

                    if (poTargetRule.RuleType == RULE_TYPE.RT_DOMAIN)
                    {
                        DomainRule Rule = (DomainRule) poTargetRule;

                        Rule.SetDomain(asValueSet);
                    }
                    else if (poTargetRule.RuleType == RULE_TYPE.RT_ARITHMETIC)
                    {
                        ArithmeticRule Rule = (ArithmeticRule) poTargetRule;

                        Rule.SetDomain(asValueSet);
                    }
                    else if (poTargetRule.RuleType == RULE_TYPE.RT_ARITH_LIMIT)
                    {
                        ArithmeticLimitRule Rule = (ArithmeticLimitRule) poTargetRule;

                        Rule.SetMinAndMax(psRuleExpression, asValueSet);
                    }
                    else if (poTargetRule.RuleType == RULE_TYPE.RT_ASSIGNMENT)
                    {
                        AssignmentRule Rule = (AssignmentRule) poTargetRule;

                        Rule.SetAssignValue(asValueSet);
                    }
                    else if (poTargetRule.RuleType == RULE_TYPE.RT_DATE_LIMIT)
                    {
                        DateLimitRule Rule = (DateLimitRule) poTargetRule;

                        Rule.SetMinAndMax(psRuleExpression, asValueSet);
                    }
                    else if (poTargetRule.RuleType == RULE_TYPE.RT_CUSTOM_OP)
                    {
                        CustomOperatorRule Rule = (CustomOperatorRule) poTargetRule;

                        string sCustomOpKey = CustomOpSources.Keys.Where(s => psRuleExpression.Contains(s)).FirstOrDefault();

                        if (!String.IsNullOrEmpty(sCustomOpKey))
                        {                            
                            Rule.SetDomain(asValueSet);

                            Rule.CustomOpName           = sCustomOpKey;
                            Rule.CustomOpContractSource = CustomOpSources[sCustomOpKey];
                        }
                    }
                }
            }
        }

        private void SetTargetAttribute(WonkaBizRule poTargetRule, string psRuleExpression)
        {
            char[] acTargetAttributeDelim = new char[1] { '.' };

            int nAttrNameStartIdx = psRuleExpression.IndexOf(CONST_RULE_TOKEN_START_DELIM);
            if (nAttrNameStartIdx >= 0)
            {
                int nAttrNameEndIdx = 
                    psRuleExpression.IndexOf(CONST_RULE_TOKEN_END_DELIM, nAttrNameStartIdx + 1);

                if (nAttrNameEndIdx > 0)
                {
                    string sRecordOfInterest = "N";

                    string sAttrName =
                        psRuleExpression.Substring(nAttrNameStartIdx + 1, (nAttrNameEndIdx - nAttrNameStartIdx - 1));

                    if (sAttrName.Contains(acTargetAttributeDelim[0]))
                    {
                        string[] acAttrNameParts = sAttrName.Split(acTargetAttributeDelim);

                        if (acAttrNameParts.Length > 1)
                        {
                            sRecordOfInterest = acAttrNameParts[0];
                            sAttrName         = acAttrNameParts[1];
                        }
                    }

                    if (sRecordOfInterest == "N")
                        poTargetRule.RecordOfInterest = TARGET_RECORD.TRID_NEW_RECORD;
                    else
                        poTargetRule.RecordOfInterest = TARGET_RECORD.TRID_OLD_RECORD;

                    if (WonkaRefEnvironment.GetInstance().IsAttribute(sAttrName))
                    {
                        poTargetRule.TargetAttribute =
                            WonkaRefEnvironment.GetInstance().GetAttributeByAttrName(sAttrName);
                    }
                    else
                        throw new WonkaBizRuleException(-1, -1, "ERROR!  Attribute (" + sAttrName + ") does not exist.");
                }
            }
        }

        #endregion

        #region Properties

        public int ErrorCount { get; set; }

        private int RuleSetIdCounter { get; set; }

        private int RuleIdCounter { get; set; }

        private int ValSeqIdCounter { get; set; }

        private string BreXmlFilepath { get; set; }

        private string BreXmlContents { get; set; }

        private HashSet<string> BasicOps { get; set; }

        private HashSet<string> ArithmeticLimitOps { get; set; }

        private HashSet<string> DateLimitOps { get; set; }

        private Dictionary<string, WonkaBizSource> CustomOpSources { get; set; }

        public WonkaBizRuleSet RootRuleSet { get; set; }

        public List<WonkaBizRuleSet> AllParsedRuleSets { get; set; }

        public WonkaBizRulesEngine RulesHostEngine { get; set; }

        #endregion
    }
}
