using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Wonka.Product;
using Wonka.MetaData;

using Wonka.BizRulesEngine.Readers;

namespace Wonka.BizRulesEngine.RuleTree.RuleTypes
{
    /// <summary>
    /// 
    /// This class represents a custom operator rule, which is an aggressive rule that will
    /// assign data returned from a delegate. For example, the following Wonka-Bre markup:
    /// 
    ///  <validate err="severe">
    ///     <criteria op = "AND" >
    ///         <eval>(N.AccountCurrValue) GT ('10.00')</eval>
    ///         <eval>(N.AccountCurrValue) LT ('100000000.00')</eval>
    ///         <eval>(N.AccountCurrValue) MY_CALCULATION ('AccountCurrValue,2,5')</eval>
    ///     </criteria >
    /// 
    /// Has two instances of the ArithmeticLimitRule that focus on the Attribute 'AccountCurrValue'.
    /// One instance evaluates whether the value of the incoming record if greater than 10.00, and 
    /// the other instance evaluates whether the incoming value is less than 100000000.0.  If both rules 
    /// are evaluated to TRUE, then the custom operator rule below them will be invoked, setting 
    /// the AccountCurrValue to the returned value from a delegate with three parameters: the current value of
    /// AccountCurrValue, 2, and 5.
    /// 
    /// NOTE: Since the WonkaProduct only records Attribute values as Strings, it will not validate
    /// the data type before assignment.
    /// 
    /// NOTE: If the Attribute has neither a 'O' or 'N' preceding it, it will be assumed to be 'N'.
    ///  
    /// </summary>
    public class CustomOperatorRule : WonkaBizRule
    {
        #region Constructors

        public CustomOperatorRule() 
            : base(-1, RULE_TYPE.RT_CUSTOM_OP)
        {
            Init(TARGET_RECORD.TRID_NONE, -1, null);
        }

        public CustomOperatorRule(int pnRuleID) 
            : base(pnRuleID, RULE_TYPE.RT_CUSTOM_OP)
        {
            Init(TARGET_RECORD.TRID_NONE, -1, null);
        }

        public CustomOperatorRule(int                                     pnRuleID, 
                                  TARGET_RECORD                           peTargetRecord, 
                                  int                                     pnTargetAttrId, 
                                  string                                  psCustomOpName,
                                  WonkaBizRulesXmlReader.ExecuteCustomOperator poCustomOpDelegate,
                                  WonkaBizSource                          poCustomOpContractSource) 
            : base(pnRuleID, RULE_TYPE.RT_CUSTOM_OP)
        {
            Init(peTargetRecord, pnTargetAttrId, null);

            CustomOpName           = psCustomOpName;
            CustomOpDelegate       = poCustomOpDelegate;
            CustomOpContractSource = poCustomOpContractSource; 
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// This method allows the caller to add a domain value to the set, whether it's a literal value or 
        /// a referenced Attribute in the Transactional record.
        /// 
        /// <param name="psDomainVal">The value to add to the set</param>
        /// <param name="pbIsLiteral">The indicator for whether the value is literal (i.e., '5') or an Attribute name (i.e., 'PubPrice')</param>
        /// <param name="peTargetRecord">The record with the Attribute value that we will add to the set (if the domain value is not literal)</param>
        /// <returns>None.</returns>
        /// </summary>
        public void AddDomainValue(string psDomainVal, bool pbIsLiteral, TARGET_RECORD peTargetRecord)
        {
            WonkaBizRuleValueProps oValueProps =
                new WonkaBizRuleValueProps() { IsLiteralValue = pbIsLiteral };

            if (pbIsLiteral)
            {
                /*
                 * NOTE: This is a bit of a hack...but since we must use commas to separate the domain values, we will require
                 *       that any rule value with an embedded comma must use "&#44;", and that value will be replaced with an 
                 *       actual comma here.  My apologies.
                 */
                if (!string.IsNullOrEmpty(psDomainVal) && psDomainVal.Contains("&#44;"))
                {
                    psDomainVal = psDomainVal.Replace("&#44;", ",");
                }

                DomainCache.Add(psDomainVal);
            }
            else
            {
                oValueProps.TargetRecord = peTargetRecord;

                oValueProps.AttributeInfo =
                    WonkaRefEnvironment.GetInstance().GetAttributeByAttrName(psDomainVal);

                HasAttrIdTargets = true;
            }

            CustomOpPropArgs.Add(psDomainVal);

            DomainValueProps[psDomainVal] = oValueProps;
        }

        public WonkaBizRuleValueProps GetDomainValueProps(string psValue)
        {
            WonkaBizRuleValueProps oValueProps = null;

            if (!string.IsNullOrEmpty(psValue) && DomainValueProps.ContainsKey(psValue))
            {
                oValueProps = DomainValueProps[psValue];
            }

            return oValueProps;
        }

        /// <summary>
        /// 
        /// This method will apply the assignment rule to either the transaction record or the current (i.e., database) 
        /// record, using the other record as a reference.
        /// 
        /// <param name="poTransactionRecord">The incoming record</param>
        /// <param name="poCurrentRecord">The current record (i.e., in the database)</param>
        /// <param name="poErrorMessage">The buffer that will contain an error message if the rule fails</param>
        /// <returns>Indicates whether or not the target product passed the rule successfully</returns>
        /// </summary>
        public override bool Execute(WonkaProduct  poTransactionRecord,
                                     WonkaProduct  poCurrentRecord,
                                     StringBuilder poErrorMessage)
        {
            bool bResult = false;

            WonkaProduct        TargetRecord = null;
            WonkaRefEnvironment WonkaRefEnv  = WonkaRefEnvironment.GetInstance();

            int nAttrId  = TargetAttribute.AttrId;
            int nGroupId = TargetAttribute.GroupId;

            if (RecordOfInterest == TARGET_RECORD.TRID_NEW_RECORD)
            {
                TargetRecord = poTransactionRecord;
            }
            else if (RecordOfInterest == TARGET_RECORD.TRID_OLD_RECORD)
            {
                TargetRecord = poCurrentRecord;
            }
            else
            {
                throw new Exception("ERROR!  The target record is none!");
            }

            string sTargetData =
                TargetRecord.GetPrimaryAttributeData(TargetAttribute.GroupId, TargetAttribute.AttrId);

            if (sTargetData == null)
            {
                sTargetData = string.Empty;
            }

            RefreshCache(poTransactionRecord, poCurrentRecord);

            WonkaPrdGroup TempProductGroup = null;

            if (RecordOfInterest == TARGET_RECORD.TRID_NEW_RECORD)
            {
                TempProductGroup = poTransactionRecord.GetProductGroup(nGroupId);
            }
            else
            {
                TempProductGroup = poCurrentRecord.GetProductGroup(nGroupId);
            }

            if ((CustomOpDelegate == null) && (CustomOpContractSource.CustomOpDelegate != null))
            {
                CustomOpDelegate = CustomOpContractSource.CustomOpDelegate;
            }

            if (CustomOpDelegate != null)
            {
                string[] CustomOpArgs = new string[4];

                for (int idx = 0; idx < 4; ++idx)
                {
                    if (idx < DomainCache.Count())
                    {
                        CustomOpArgs[idx] = DomainCache.ElementAt(idx);
                    }
                    else
                    {
                        CustomOpArgs[idx] = string.Empty;
                    }
                }

                TempProductGroup[0][nAttrId] = 
                    CustomOpDelegate(CustomOpArgs[0], CustomOpArgs[1], CustomOpArgs[2], CustomOpArgs[3]);

                if (poErrorMessage != null)
                {
                    poErrorMessage.Clear();
                    poErrorMessage.Append(GetVerboseError(TargetRecord));
                }
            }

            return bResult;
        }

        /// <summary>
        /// 
        /// This method will return a verbose error that explains why the rule failed for a particular product.
        /// 
        /// <param name="poTargetProduct">The record to which the current rule is applied</param>
        /// <returns>String with an error message that provides more detail about the rule failure</returns>
        /// </summary>
        public override string GetVerboseError(WonkaProduct poTargetProduct)
        {
            StringBuilder DomainListBuilder   = new StringBuilder();
            StringBuilder VerboseErrorBuilder = new StringBuilder();

            string sAttrName = TargetAttribute.AttrName;

            foreach (string sDomainVal in DomainCache)
            {
                if (DomainListBuilder.Length > 0)
                {
                    DomainListBuilder.Append(",");
                }

                DomainListBuilder.Append(sDomainVal);
            }

            VerboseErrorBuilder.Append(sAttrName);
            VerboseErrorBuilder.Append(" ");
            VerboseErrorBuilder.Append("could not invoke the delegate associated for the Custom Operator, with the arguments (");
            VerboseErrorBuilder.Append(DomainListBuilder.ToString());
            VerboseErrorBuilder.Append(").");

            if (VerboseErrorBuilder.Length > (CONST_MAX_VERBOSE_ERROR_SIZE + 3))
            {
                VerboseErrorBuilder.Remove(CONST_MAX_VERBOSE_ERROR_SIZE, (VerboseErrorBuilder.Length - CONST_MAX_VERBOSE_ERROR_SIZE));
                VerboseErrorBuilder.Append("...");
            }

            return VerboseErrorBuilder.ToString();
        }

        private void Init(TARGET_RECORD peTargetRecord, int pnTargetAttrId, string psAssignValue)
        {
            this.IsPassive        = false;
            this.HasAttrIdTargets = false;

            DomainCache      = new List<string>();
            DomainValueProps = new Dictionary<string, WonkaBizRuleValueProps>();
            CustomOpPropArgs = new List<string>();

            this.RecordOfInterest = peTargetRecord;

            if (pnTargetAttrId > 0)
            {
                this.TargetAttribute = WonkaRefEnvironment.GetInstance().GetAttributeByAttrId(pnTargetAttrId);
            }
        }

        /// <summary>
        /// 
        /// This method will refresh the set of data (if there are any mentioned Attributes).
        /// 
        /// <param name="poNewProduct">The incoming Product whose Attributes might be used to populate the Set</param>
        /// <param name="poOldProduct">The current Product whose Attributes might be used to populate the Set</param>
        /// <returns>Indicator of whether the set was refreshed successfully</returns>
        /// </summary>
        public bool RefreshCache(WonkaProduct poNewProduct, WonkaProduct poOldProduct)
        {
            return RefreshCache(poNewProduct, poOldProduct, this.DomainCache);
        }

        /// <summary>
        /// 
        /// This method will refresh the set of data if there are any mentioned Attributes.
        /// 
        /// <param name="poNewProduct">The incoming Product whose Attributes might be used to populate the Set</param>
        /// <param name="poOldProduct">The current Product whose Attributes might be used to populate the Set</param>
        /// <param name="poDomainCache">Our domain cache that will be refreshed</param>
        /// <returns>Indicator of whether the set was refreshed successfully</returns>
        /// </summary>
        public bool RefreshCache(WonkaProduct poNewProduct, WonkaProduct poOldProduct, List<string> poDomainCache)
        {
            bool bResult  = true;
            int  nAttrId  = 0;
            int  nGroupId = 0;

            WonkaBizRuleValueProps RuleValueProps = null;

            if (HasAttrIdTargets)
            {
                if (poNewProduct == null)
                {
                    throw new Exception("ERROR!  The new Product is null.");
                }

                if (poNewProduct == null)
                {
                    throw new Exception("ERROR!  The old Product is null.");
                }

                if (poDomainCache == null)
                {
                    throw new Exception("ERROR!  The provided Domain Cache is null.");
                }

                poDomainCache.Clear();
                foreach (string sDomainValue in DomainValueProps.Keys)
                {
                    RuleValueProps = DomainValueProps[sDomainValue];

                    if (RuleValueProps.IsLiteralValue)
                    {
                        poDomainCache.Add(sDomainValue);
                    }
                    else
                    {
                        nAttrId  = RuleValueProps.AttributeInfo.AttrId;
                        nGroupId = RuleValueProps.AttributeInfo.GroupId;

                        WonkaPrdGroup TempProductGroup = null;

                        if (RuleValueProps.TargetRecord == TARGET_RECORD.TRID_NEW_RECORD)
                        {
                            TempProductGroup = poNewProduct.GetProductGroup(nGroupId);
                        }
                        else
                        {
                            TempProductGroup = poOldProduct.GetProductGroup(nGroupId);
                        }

                        foreach (WonkaPrdGroupDataRow TempDataRow in TempProductGroup)
                        {
                            if (TempDataRow.Keys.Contains(nAttrId))
                            {
                                poDomainCache.Add(TempDataRow[nAttrId]);
                            }
                        }
                    }
                }
            }

            return bResult;
        }

        /// <summary>
        /// 
        /// This method will use the provided values to set the domain of this rule.  More importantly, 
        /// it will assist with the initialization of the rule by interpreting the provided values 
        /// to detect which provided values are literal and which of them are Attribute names (which 
        /// will be used to dynamically update the domain on every processed pair of records).
        /// 
        /// <param name="asDomainValues">The set of values that are the domain for this particular rule</param>
        /// <returns>None</returns>
        /// </summary>
        public void SetDomain(string[] asDomainValues)
        {
            DomainCache.Clear();
            DomainValueProps.Clear();

            foreach (string sTempDomainVal in asDomainValues)
            {
                int nLiteralValueStartIdx = sTempDomainVal.IndexOf("'");
                if (nLiteralValueStartIdx >= 0)
                {
                    int nLiteralValueEndIdx = sTempDomainVal.LastIndexOf("'");

                    if (nLiteralValueEndIdx > nLiteralValueStartIdx)
                    {
                        string sLiteralValue =
                            sTempDomainVal.Substring(nLiteralValueStartIdx + 1, nLiteralValueEndIdx - nLiteralValueStartIdx - 1);

                        AddDomainValue(sLiteralValue, true, TARGET_RECORD.TRID_NONE);
                    }
                }
                else
                {
                    char[] acAttrNameDelim = new char[1] { '.' };

                    string        sAttrName     = sTempDomainVal;
                    TARGET_RECORD eTargetRecord = TARGET_RECORD.TRID_NEW_RECORD;

                    if (sTempDomainVal.Contains(acAttrNameDelim[0]))
                    {
                        string[] asAttrNameParts = sTempDomainVal.Split(acAttrNameDelim);

                        if (asAttrNameParts.Length > 1)
                        {
                            string sTargetRecord = asAttrNameParts[0];

                            sAttrName = asAttrNameParts[1];

                            if (sTargetRecord == "O")
                            {
                                eTargetRecord = TARGET_RECORD.TRID_OLD_RECORD;
                            }
                        }
                    }
                    else
                    {
                        sAttrName = sTempDomainVal;
                    }

                    AddDomainValue(sAttrName, false, eTargetRecord);
                }
            }
        }

        #endregion

        #region Properties

        public bool HasAttrIdTargets { get; set; }

        public string CustomOpName { get; set; }

        public WonkaBizRulesXmlReader.ExecuteCustomOperator CustomOpDelegate { get; set; }

        public WonkaBizSource CustomOpContractSource { get; set; }

        public List<string> DomainCache { get; set; }

        public Dictionary<string, WonkaBizRuleValueProps> DomainValueProps { get; set; }

        public List<string> CustomOpPropArgs { get; set; }

        #endregion
    }
}

