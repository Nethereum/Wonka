using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Wonka.Product;
using Wonka.MetaData;

namespace Wonka.BizRulesEngine.RuleTree.RuleTypes
{
    /// <summary>
    /// 
    /// This class represents an arithmetic formula rule, which is an aggressive rule that will
    /// assign data based on an arithmetic operation. For example, the following Wonka-Bre markup:
    /// 
    /// <if description="Checks the type of the account">
    ///     <criteria>
    ///         <eval>(O.BankAccountID) EQ (1234567890)</eval>
    ///     </criteria>
    /// 
    ///     <validate err="severe" >
    ///         <criteria>
    ///             <eval>(N.CurrAccountValue) ASSIGN_SUM ('15.00', O.CurrAccountValue)</eval>
    ///         </criteria>
    /// 
    /// If the Attribute 'BankAccountID' (of the old record already preserved) has a value within the set {'1234567890'},
    /// we then move onto the Arithmetic rule inside the ruleset.  There, it will assign the the Attribute 'CurrAccountValue' 
    /// (of the new record) to a value that is the sum of 15.00 plus the 'CurrAccountValue' value of the old record.
    /// 
    /// NOTE: If the Attribute has neither a 'O' or 'N' preceding it, it will be assumed to be 'N'.
    ///  
    /// </summary>
    public class ArithmeticRule : WonkaBizRule
    {
        #region Constructors

        public ArithmeticRule() 
            : base(-1, RULE_TYPE.RT_ARITHMETIC)
        {
            Init(TARGET_RECORD.TRID_NONE, -1, ARITH_OP_TYPE.AOT_NONE);
        }

        public ArithmeticRule(int pnRuleID, ARITH_OP_TYPE poArithOpType) 
            : base(pnRuleID, RULE_TYPE.RT_ARITHMETIC)
        {
            Init(TARGET_RECORD.TRID_NONE, -1, poArithOpType);
        }

        public ArithmeticRule(int pnRuleID, TARGET_RECORD peTargetRecord, int pnTargetAttrId, ARITH_OP_TYPE poArithOpType, bool bSearchAllRows) 
            : base(pnRuleID, RULE_TYPE.RT_ARITHMETIC)
        {
            Init(peTargetRecord, pnTargetAttrId, poArithOpType);

            this.SearchAllDataRows = bSearchAllRows;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// This method allows the caller to add a value to the set for the arithmetic operation, whether it's a literal value or 
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
                DomainCache.Add(psDomainVal);
            }
            else
            {
                oValueProps.TargetRecord = peTargetRecord;

                oValueProps.AttributeInfo =
                    WonkaRefEnvironment.GetInstance().GetAttributeByAttrName(psDomainVal);

                HasAttrIdTargets = true;
            }

            DomainValueProps[psDomainVal] = oValueProps;
        }

        public WonkaBizRuleValueProps GetDomainValueProps(string psValue)
        {
            WonkaBizRuleValueProps oValueProps = null;

            if (!String.IsNullOrEmpty(psValue) && DomainValueProps.ContainsKey(psValue))
            {
                oValueProps = DomainValueProps[psValue];
            }

            return oValueProps;
        }

        private uint CalculateValue()
        {
            uint nTmpIdx = 0;
            uint nTmpVal = 0;
            uint nResult = 0;

            foreach (string sTempValue in DomainCache)
            {
                try
                {
                    nTmpVal = 
                        (uint) Convert.ToDouble(sTempValue, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                }
                catch (Exception ex)
                { 
                    nTmpVal = 0; 
                }

                if (nTmpIdx == 0)
                {
                    nResult = nTmpVal;
                }
                else if (this.OpType == ARITH_OP_TYPE.AOT_SUM)
                {
                    nResult += nTmpVal;
                }
                else if (this.OpType == ARITH_OP_TYPE.AOT_DIFF)
                {
                    nResult -= nTmpVal;
                }
                else if (this.OpType == ARITH_OP_TYPE.AOT_PROD)
                {
                    nResult *= nTmpVal;
                }
                else if (this.OpType == ARITH_OP_TYPE.AOT_QUOT)
                {
                    nResult /= nTmpVal;
                }

                ++nTmpIdx;
            }

            return nResult;
        }

        /// <summary>
        /// 
        /// This method will apply the arithmetic rule to either the transaction record or the current (i.e., database) record, 
        /// using the other record as a reference.
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
            WonkaRefEnvironment WonkaRefEnv    = WonkaRefEnvironment.GetInstance();

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

            TempProductGroup[0][nAttrId] = Convert.ToString(CalculateValue());

            if (poErrorMessage != null)
            {
                poErrorMessage.Clear();
                poErrorMessage.Append(GetVerboseError(TargetRecord));
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

            string sResultType = string.Empty;
            if (this.OpType == ARITH_OP_TYPE.AOT_SUM)
            {
                sResultType = "sum";
            }
            else if (this.OpType == ARITH_OP_TYPE.AOT_DIFF)
            {
                sResultType = "difference";
            }
            else if (this.OpType == ARITH_OP_TYPE.AOT_PROD)
            {
                sResultType = "product";
            }
            else if (this.OpType == ARITH_OP_TYPE.AOT_QUOT)
            {
                sResultType = "quotient";
            }

            VerboseErrorBuilder.Append(sAttrName);
            VerboseErrorBuilder.Append(" ");
            VerboseErrorBuilder.Append("could not calculate the arithmetic [" + sResultType + "] for (");
            VerboseErrorBuilder.Append(DomainListBuilder.ToString());
            VerboseErrorBuilder.Append(").");

            if (VerboseErrorBuilder.Length > (CONST_MAX_VERBOSE_ERROR_SIZE+3))
            {
                VerboseErrorBuilder.Remove(CONST_MAX_VERBOSE_ERROR_SIZE, (VerboseErrorBuilder.Length - CONST_MAX_VERBOSE_ERROR_SIZE));
                VerboseErrorBuilder.Append("...");
            }

            return VerboseErrorBuilder.ToString();
        }

        private void Init(TARGET_RECORD peTargetRecord, int pnTargetAttrId, ARITH_OP_TYPE poArithOpType)
        {
            this.IsPassive = false;
            this.OpType    = poArithOpType; 

            this.RecordOfInterest = peTargetRecord;

            if (pnTargetAttrId > 0)
            {
                this.TargetAttribute = WonkaRefEnvironment.GetInstance().GetAttributeByAttrId(pnTargetAttrId);
            }

            this.HasAttrIdTargets  = false;
            this.SearchAllDataRows = false;

            DomainCache      = new HashSet<string>();
            DomainValueProps = new Dictionary<string, WonkaBizRuleValueProps>();
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
        public bool RefreshCache(WonkaProduct poNewProduct, WonkaProduct poOldProduct, HashSet<string> poDomainCache)
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

        public ARITH_OP_TYPE OpType { get; set; }

        public bool HasAttrIdTargets { get; set; }

        public bool SearchAllDataRows { get; set; }

        public HashSet<string> DomainCache { get; set; }

        public Dictionary<string, WonkaBizRuleValueProps> DomainValueProps { get; set; }

        #endregion
    }
}

