using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Wonka.BizRulesEngine.Readers;
using Wonka.BizRulesEngine.RuleTree;
using Wonka.BizRulesEngine.RuleTree.RuleTypes;

namespace Wonka.BizRulesEngine.Writers
{
	public class WonkaBizRulesXmlWriter
	{
        private WonkaBizRulesEngine moRulesEngine = null;

		public WonkaBizRulesXmlWriter(WonkaBizRulesEngine poRulesEngine)
		{
            moRulesEngine = poRulesEngine;
        }

        ///
        /// <summary>
        /// 
        /// This method will write the XML (i.e., Wonka rules markup) of a RuleTree.
		///
        /// NOTE: Currently, we use a StringBuilder class to build the XML Document.  In the future, we should transition to
        /// using a XmlDocument and a XmlWriter.
        ///  
        /// <param name="poEngine">The rules engine which holds the RuleTree of interest</param>
        /// <returns>Returns the XML payload that represents a RuleTree</returns>
        /// </summary>
        public string ExportXmlString()
        {
            StringBuilder sbExportXmlString = new StringBuilder("<?xml version=\"1.0\"?>\n<RuleTree>\n");

            sbExportXmlString.Append(ExportXmlString(moRulesEngine.RuleTreeRoot, 0));

            sbExportXmlString.Append("</RuleTree>\n");

            string sExportXml = sbExportXmlString.ToString();
            
            return sExportXml;
        }

        ///
        /// <summary>
        /// 
        /// This method will write the XML (i.e., Wonka rules markup) of a RuleSet.
        /// 
        /// NOTE: Currently, we use a StringBuilder class to build the XML Document.  In the future, we should transition to
        /// using a XmlDocument and a XmlWriter.
        ///
        /// <param name="poRuleSet">The rules engine which holds the RuleTree of interest</param>
        /// <param name="pnStepLevel">The level of the RuleTree</param>
        /// <returns>Returns the XML payload that represents a RuleSet</returns>
        /// </summary>
        private string ExportXmlString(WonkaBizRuleSet poRuleSet, uint pnStepLevel)
        {
            var RSNodeTag   = WonkaBizRulesXmlReader.CONST_RS_FLOW_TAG;
            var RSNodeDesc  = WonkaBizRulesXmlReader.CONST_RS_FLOW_DESC_ATTR;
            var RSLeafTag   = WonkaBizRulesXmlReader.CONST_RS_VALID_TAG;
            var RSLeafMode  = WonkaBizRulesXmlReader.CONST_RS_VALID_ERR_ATTR;
            var RuleCollTag = WonkaBizRulesXmlReader.CONST_RULES_TAG;
            var LogicOp     = WonkaBizRulesXmlReader.CONST_RULES_OP_ATTR;

            StringBuilder sbExportXmlString = new StringBuilder();
            StringBuilder sbTabSpaces       = new StringBuilder();
            StringBuilder sbCritSpaces      = new StringBuilder();
            StringBuilder sbRuleSpaces      = new StringBuilder();

            for (uint x = 0; x < pnStepLevel; x++)
                sbTabSpaces.Append("    ");

            sbCritSpaces.Append(sbTabSpaces.ToString()).Append("    ");
            sbRuleSpaces.Append(sbCritSpaces.ToString()).Append("    ");

            if (!poRuleSet.Description.StartsWith("Root", StringComparison.CurrentCultureIgnoreCase))
            {
                if (poRuleSet.ChildRuleSets.Count > 0)
                {
                    sbExportXmlString.Append(sbTabSpaces.ToString()).Append("<" + RSNodeTag + " " + RSNodeDesc + "=\"" + poRuleSet.Description + "\" >\n");
                }
                else
                {
                    string sMode =
                        (poRuleSet.ErrorSeverity == RULE_SET_ERR_LVL.ERR_LVL_SEVERE) ? WonkaBizRulesXmlReader.CONST_RS_VALID_ERR_SEVERE : WonkaBizRulesXmlReader.CONST_RS_VALID_ERR_WARNING;

                    sbExportXmlString.Append(sbTabSpaces.ToString()).Append("<" + RSLeafTag + " " + RSLeafMode + "=\"" + sMode + "\" >\n");
                }

                sbExportXmlString.Append(sbCritSpaces.ToString());
                sbExportXmlString.Append("<" + RuleCollTag + " " + LogicOp + "=\"" + ((poRuleSet.RulesEvalOperator == RULE_OP.OP_AND) ? "AND" : "OR") + "\" >\n");

                if (poRuleSet.EvaluativeRules.Count > 0)
                {
                    StringBuilder sbRulesBody = new StringBuilder();
                    sbRulesBody.Append(sbTabSpaces.ToString()).Append(sbTabSpaces.ToString()).Append(sbTabSpaces.ToString());

                    for (int idx = 0; idx < poRuleSet.EvaluativeRules.Count; idx++)
                    {
                        sbExportXmlString.Append(ExportXmlString(poRuleSet.EvaluativeRules[idx], sbRuleSpaces));
                    }
                }

                if (poRuleSet.AssertiveRules.Count > 0)
                {
                    StringBuilder sbRulesBody = new StringBuilder();
                    sbRulesBody.Append(sbTabSpaces.ToString()).Append(sbTabSpaces.ToString()).Append(sbTabSpaces.ToString());

                    for (int idx = 0; idx < poRuleSet.AssertiveRules.Count; idx++)
                    {
                        sbExportXmlString.Append(ExportXmlString(poRuleSet.AssertiveRules[idx], sbRuleSpaces));
                    }
                }

                sbExportXmlString.Append(sbCritSpaces.ToString());
                sbExportXmlString.Append("</" + RuleCollTag + ">\n");
            }

            // Now invoke the rulesets
            for (int childIdx = 0; childIdx < poRuleSet.ChildRuleSets.Count; childIdx++)
            {
                sbExportXmlString.Append(ExportXmlString(poRuleSet.ChildRuleSets[childIdx], pnStepLevel + 1));
            }

            if (!poRuleSet.Description.StartsWith("Root", StringComparison.CurrentCultureIgnoreCase))
            {
                if (poRuleSet.ChildRuleSets.Count > 0)
                    sbExportXmlString.Append(sbTabSpaces.ToString()).Append("</" + RSNodeTag + ">\n");
                else
                    sbExportXmlString.Append(sbTabSpaces.ToString()).Append("</" + RSLeafTag + ">\n");
            }

            return sbExportXmlString.ToString();
        }

        ///
        /// <summary>
        /// 
        /// This method will write the XML (i.e., Wonka rules markup) of a Rule.
        /// 
        /// NOTE: Currently, we use a StringBuilder class to build the XML Document.  In the future, we should transition to
        /// using a XmlDocument and a XmlWriter.
        /// 
        /// <returns>Returns the XML payload that represents a Rule</returns>
        /// </summary>
        public string ExportXmlString(WonkaBizRule poRule, StringBuilder poSpaces)
        {
            string sOpName      = string.Empty;
            string sRuleValue   = "";
            string sDelim       = WonkaBizRulesXmlReader.CONST_RULE_TOKEN_VAL_DELIM;
            string sSingleQuote = "'";

            string sRuleTagFormat = 
                "{0}<" + WonkaBizRulesXmlReader.CONST_RULE_TAG + " " + WonkaBizRulesXmlReader.CONST_RULE_ID_ATTR + "=\"{1}\">(N.{2}) {3} {4}</eval>\n";

            if (poRule.RuleType == RULE_TYPE.RT_ARITH_LIMIT)
			{
                ArithmeticLimitRule ArithLimitRule = (ArithmeticLimitRule) poRule;

                if (ArithLimitRule.MinValue == Double.MinValue)
                {
                    sOpName    = !poRule.NotOperator ? WonkaBizRulesXmlReader.CONST_AL_LT : WonkaBizRulesXmlReader.CONST_AL_NOT_LT;
                    sRuleValue = Convert.ToString(ArithLimitRule.MaxValue);
                }
                else if (ArithLimitRule.MaxValue == Double.MaxValue)
                {
                    sOpName    = !poRule.NotOperator ? WonkaBizRulesXmlReader.CONST_AL_GT : WonkaBizRulesXmlReader.CONST_AL_NOT_GT;
                    sRuleValue = Convert.ToString(ArithLimitRule.MinValue);
                }
                else
                {
                    sOpName    = !poRule.NotOperator ? WonkaBizRulesXmlReader.CONST_AL_EQ : WonkaBizRulesXmlReader.CONST_AL_NOT_EQ;
                    sRuleValue = Convert.ToString(ArithLimitRule.MinValue);
                }
            }
            else if (poRule.RuleType == RULE_TYPE.RT_DATE_LIMIT)
            {
                DateLimitRule DtLimitRule = (DateLimitRule) poRule;

                if (DtLimitRule.MinValue == DateTime.MinValue)
                {
                    sOpName    = !poRule.NotOperator ? WonkaBizRulesXmlReader.CONST_DL_IA : WonkaBizRulesXmlReader.CONST_DL_NOT_IA;
                    sRuleValue = DtLimitRule.MaxValue.ToString("MM/dd/yyyy");
                }
                else if (DtLimitRule.MaxValue == DateTime.MaxValue)
                {
                    sOpName    = !poRule.NotOperator ? WonkaBizRulesXmlReader.CONST_DL_IB : WonkaBizRulesXmlReader.CONST_DL_NOT_IB;
                    sRuleValue = DtLimitRule.MinValue.ToString("MM/dd/yyyy");
                }
                else 
                {
                    sOpName    = WonkaBizRulesXmlReader.CONST_DL_AROUND;
                    sRuleValue = DtLimitRule.MinValue.ToString("MM/dd/yyyy");
                }
            }
            else if (poRule.RuleType == RULE_TYPE.RT_POPULATED)
            {
                sOpName =
					!poRule.NotOperator ? WonkaBizRulesXmlReader.CONST_BASIC_OP_POP : WonkaBizRulesXmlReader.CONST_BASIC_OP_NOT_POP;
			}
            else if (poRule.RuleType == RULE_TYPE.RT_DOMAIN)
            {
                StringBuilder DomainVals = new StringBuilder();

                DomainRule DmnRule = (DomainRule) poRule;

                sOpName =
                    !poRule.NotOperator ? WonkaBizRulesXmlReader.CONST_BASIC_OP_IN : WonkaBizRulesXmlReader.CONST_BASIC_OP_NOT_IN;

                sRuleValue = BuildDomainValues(DmnRule.DomainValueProps, sDelim, sSingleQuote);
			}
            else if (poRule.RuleType == RULE_TYPE.RT_ASSIGNMENT)
            {
                StringBuilder DomainVals = new StringBuilder();

                AssignmentRule AssignRule = (AssignmentRule) poRule;

                sOpName = WonkaBizRulesXmlReader.CONST_BASIC_OP_ASSIGN;

                var ValueProps = new Dictionary<string, WonkaBizRuleValueProps>();
                ValueProps[AssignRule.AssignValue] = AssignRule.AssignValueProps;
                    
                sRuleValue = BuildDomainValues(ValueProps, sDelim, sSingleQuote);
            }
            else if (poRule.RuleType == RULE_TYPE.RT_ARITHMETIC)
            {
                ArithmeticRule ArithRule = (ArithmeticRule) poRule;

                if (ArithRule.OpType == ARITH_OP_TYPE.AOT_SUM)
                    sOpName = WonkaBizRulesXmlReader.CONST_BASIC_OP_ASSIGN_SUM;
                else if (ArithRule.OpType == ARITH_OP_TYPE.AOT_DIFF)
                    sOpName = WonkaBizRulesXmlReader.CONST_BASIC_OP_ASSIGN_DIFF;
                else if (ArithRule.OpType == ARITH_OP_TYPE.AOT_PROD)
                    sOpName = WonkaBizRulesXmlReader.CONST_BASIC_OP_ASSIGN_PROD;
                else if (ArithRule.OpType == ARITH_OP_TYPE.AOT_QUOT)
                    sOpName = WonkaBizRulesXmlReader.CONST_BASIC_OP_ASSIGN_QUOT;

                sRuleValue = BuildDomainValues(ArithRule.DomainValueProps, sDelim, sSingleQuote);
            }
            else if (poRule.RuleType == RULE_TYPE.RT_CUSTOM_OP)
            {
                CustomOperatorRule CustomOpRule = (CustomOperatorRule) poRule;

                sOpName = CustomOpRule.CustomOpName;

                sRuleValue = BuildDomainValues(CustomOpRule.DomainValueProps, sDelim, sSingleQuote);
            }
            else
            {
                throw new WonkaBizRuleException("ERROR!  Unsupported Rule Type when writing out the Wonka RuleTree.");
            }

            if (!String.IsNullOrEmpty(sRuleValue))
                sRuleValue = "(" + sRuleValue + ")";

            return String.Format(sRuleTagFormat, poSpaces.ToString(), poRule.DescRuleId, poRule.TargetAttribute.AttrName, sOpName, sRuleValue);
        }

		private string BuildDomainValues(Dictionary<string, WonkaBizRuleValueProps> poValueProps, string sDelim = ",", string sLiteralValMarker = "'")
		{
            StringBuilder DomainVals = new StringBuilder();

            foreach (string sTmpValue in poValueProps.Keys)
            {
                var ValProps = poValueProps[sTmpValue];

                if (DomainVals.Length > 0)
                    DomainVals.Append(sDelim);

                if (ValProps.IsLiteralValue)
                    DomainVals.Append(sLiteralValMarker).Append(sTmpValue).Append(sLiteralValMarker);
                else
                    DomainVals.Append(sTmpValue);
            }

            return DomainVals.ToString();
		}
	}
}
