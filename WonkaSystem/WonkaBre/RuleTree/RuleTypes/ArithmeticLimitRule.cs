using System;
using System.Linq;
using System.Text;

using WonkaBre.Readers;
using WonkaPrd;
using WonkaRef;

namespace WonkaBre.RuleTree.RuleTypes
{
	/// <summary>
	/// 
	/// This class represents the type of validation rule where an Attribute of a Product is
	/// evaluated against a range of numeric values.  For example, the following Wonka-Bre XML markup:
	/// 
	///  <validate err="severe">
	///     <criteria op = "AND" >
	///         <eval>(N.AccountCurrValue) GT (10.00)</eval>
	///         <eval>(N.AccountCurrValue) LT (100000000.00)</eval>
	///         <eval>(N.AccountCurrValue) GT (O.AccountCurrValue)</eval>
	///     </criteria >
	/// 
	/// Has three instances of the ArithmeticLimitRule that focus on the Attribute 'AccountCurrValue'.
	/// One instance evaluates whether the incoming value if greater than 10.00, another instance evaluates
    /// whether the incoming value is less than 100000000.0, and the last evaluates whether the incoming value
    /// is greater than the old value.
	/// 
	/// NOTE: All numeric values are represented as doubles.
	///  
	/// </summary>
	public class ArithmeticLimitRule : WonkaBreRule
    {
        #region Constructors
        public ArithmeticLimitRule() : base(-1, RULE_TYPE.RT_ARITH_LIMIT)
        {
            Init(0.0, 0.0, TARGET_RECORD.TRID_NONE, -1);
        }

        public ArithmeticLimitRule(int pnRuleId) : base(pnRuleId, RULE_TYPE.RT_ARITH_LIMIT)
        {
            Init(0.0, 0.0, TARGET_RECORD.TRID_NONE, -1);
        }           

        public ArithmeticLimitRule(int pnRuleId, TARGET_RECORD peTargetRecord, int pnTargetAttrId, double pnMinValue, double pnMaxValue) 
            : base(pnRuleId, RULE_TYPE.RT_ARITH_LIMIT)
        {
            Init(pnMinValue, pnMaxValue, peTargetRecord, pnTargetAttrId);
        }

        public ArithmeticLimitRule(int pnRuleId, TARGET_RECORD peTargetRecord, int pnTargetAttrId, double pnMinValue, double pnMaxValue, bool pbNotOp)
            : base(pnRuleId, RULE_TYPE.RT_ARITH_LIMIT)
        {
            Init(pnMinValue, pnMaxValue, peTargetRecord, pnTargetAttrId);

            NotOperator = pbNotOp;
        }
        #endregion

        #region Methods

        /// <summary>
        /// 
        /// This method will apply the arithmetic limit rule to either the incoming record or the current (i.e., database) 
        /// record, using the other record as a reference.
        /// 
        /// <param name="poTransactionRecord">The incoming record</param>
        /// <param name="poCurrentRecord">The current record (i.e., the already existing record)</param>
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

            if (RecordOfInterest == TARGET_RECORD.TRID_NEW_RECORD)
                TargetRecord = poTransactionRecord;
            else if (RecordOfInterest == TARGET_RECORD.TRID_OLD_RECORD)
                TargetRecord = poCurrentRecord;
            else
                throw new Exception("ERROR!  The target record is none!");

            string sTargetData =
                TargetRecord.GetPrimaryAttributeData(TargetAttribute.GroupId, TargetAttribute.AttrId);

            if (!String.IsNullOrEmpty(sTargetData))
            {
                RefreshMinAndMax(poTransactionRecord, poCurrentRecord);

                try
                {
                    double dTargetValue = Convert.ToDouble(sTargetData);

                    if ((this.MaxValue >= dTargetValue) && (dTargetValue >= this.MinValue))
                        bResult = true;
                    else
                        bResult = false;

                    if (poErrorMessage != null)
                    {
                        poErrorMessage.Clear();
                        poErrorMessage.Append(GetVerboseError(TargetRecord));
                    }
                }
                catch (Exception ex)
                {
                    bResult = false;

                    if (poErrorMessage != null)
                    {
                        poErrorMessage.Clear();
                        poErrorMessage.Append(ex.ToString());
                    }
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
            string sVerboseError = "";

            string sAttrName = TargetAttribute.AttrName;

            if (MinValue == MaxValue)
            {
                sVerboseError =
                    String.Format("Rule ({0} {1} {2}) fails.",
                                  sAttrName,
                                  NotOperator ? "!=" : "==",
                                  MaxValue);
            }
            else if (NotOperator)
            {
                sVerboseError =
                    String.Format("Rule (({0} < {1}) OR ({0} > {2})) fails.", sAttrName, MinValue, MaxValue);
            }
            else
            {
                sVerboseError =
                    String.Format("Rule ({0} <= {1} <= {2}) fails.", MinValue, sAttrName, MaxValue);
            }

            if (sVerboseError.Length > (CONST_MAX_VERBOSE_ERROR_SIZE + 3))
            {
                sVerboseError = sVerboseError.Substring(0, CONST_MAX_VERBOSE_ERROR_SIZE);

                sVerboseError += "...";
            }

            return sVerboseError;
        }

        private void Init(double pnMinValue, double pnMaxValue, TARGET_RECORD peTargetRecord, int pnTargetAttrId)
        {
            this.IsPassive = true;

            this.RecordOfInterest = peTargetRecord;

            if (pnTargetAttrId > 0)
                this.TargetAttribute  = WonkaRefEnvironment.GetInstance().GetAttributeByAttrId(pnTargetAttrId);

            this.MinValue      = pnMinValue;
            this.MinValueProps = new WonkaBreRuleValueProps() { IsLiteralValue = true };

            this.MaxValue      = pnMaxValue;
            this.MaxValueProps = new WonkaBreRuleValueProps() { IsLiteralValue = true };
        }

		/// <summary>
		/// 
		/// If either the min and max values are set to be dynamic (i.e., subject to record values),
		/// this method will update the provided values for the min and max of this rule and use the
		/// appropriate target records to do so.
		/// 
		/// NOTE: The code will only address the first row of a Group, in both the case of the old product and the new one.
		/// 
		/// <param name="poTransactionRecord">The incoming (i.e., transaction) record</param>
		/// <param name="poCurrentRecord">The old (i.e., current) record</param>
		/// <returns>None</returns>
		/// </summary>
	    private void RefreshMinAndMax(WonkaProduct poTransactionRecord, WonkaProduct poCurrentRecord)
        {
            if (!this.MinValueProps.IsLiteralValue)
            {
                int nAttrId  = this.MinValueProps.AttributeInfo.AttrId;
                int nGroupId = this.MinValueProps.AttributeInfo.GroupId;

                WonkaPrdGroup TempProductGroup = null;

                if (this.MinValueProps.TargetRecord == TARGET_RECORD.TRID_NEW_RECORD)
                    TempProductGroup = poTransactionRecord.GetProductGroup(nGroupId);
                else
                    TempProductGroup = poCurrentRecord.GetProductGroup(nGroupId);

                this.MinValue = Convert.ToDouble(TempProductGroup[0][nAttrId]);
            }

            if (!this.MaxValueProps.IsLiteralValue)
            {
                int nAttrId  = this.MaxValueProps.AttributeInfo.AttrId;
                int nGroupId = this.MaxValueProps.AttributeInfo.GroupId;

                WonkaPrdGroup TempProductGroup = null;

                if (this.MaxValueProps.TargetRecord == TARGET_RECORD.TRID_NEW_RECORD)
                    TempProductGroup = poTransactionRecord.GetProductGroup(nGroupId);
                else
                    TempProductGroup = poCurrentRecord.GetProductGroup(nGroupId);

                this.MaxValue = Convert.ToDouble(TempProductGroup[0][nAttrId]);
            }
        }

        /// <summary>
        /// 
        /// This method will use the provided values to set the min and max of this rule.  More importantly, 
        /// it will assist with the initialization of the rule by interpreting the provided values 
        /// to detect which values are literal and which of them are Attribute names (which 
        /// will be used to dynamically update the value(s) on every processed pair of records).
        /// 
        /// <param name="psRuleExpression">The arithmetic rule in verbose form</param>
        /// <param name="paValueSet">The value(s) that will determine the min and max</param>
        /// <returns>None</returns>
        /// </summary>
        public void SetMinAndMax(string psRuleExpression, string[] paValueSet)
        {
            if (paValueSet.Count() > 0)
            {
                // NOTE: Currently, if more than one value has been provided, we will only 
                //       take the first value into account
                bool   bLiteralValue = false;
                double dValue        = 0.0;
                string sTempValue    = paValueSet[0];

                WonkaBreRuleValueProps AttributeValueProps = new WonkaBreRuleValueProps();

                try
                {
                    dValue        = Convert.ToDouble(sTempValue);
                    bLiteralValue = true;
                }
                catch (Exception ex)
                {
                    dValue              = 0.0;
                    AttributeValueProps = this.GetAttributeValueProps(sTempValue);
                }

                if (psRuleExpression.Contains(WonkaBreXmlReader.CONST_AL_LT) ||
                    psRuleExpression.Contains(WonkaBreXmlReader.CONST_AL_NOT_LT))
                {
                    this.MinValue = Double.MinValue;
                    this.MaxValue = dValue - 0.001;

                    if (!bLiteralValue)
                        this.MaxValueProps = AttributeValueProps;

                    if (psRuleExpression.Contains(WonkaBreXmlReader.CONST_AL_NOT_LT))
                        this.NotOperator = true;
                }
                else if (psRuleExpression.Contains(WonkaBreXmlReader.CONST_AL_GT) ||
                         psRuleExpression.Contains(WonkaBreXmlReader.CONST_AL_NOT_GT))
                {
                    this.MinValue = dValue + 0.001;
                    this.MaxValue = Double.MaxValue;

                    if (!bLiteralValue)
                        this.MinValueProps = AttributeValueProps;

                    if (psRuleExpression.Contains(WonkaBreXmlReader.CONST_AL_NOT_GT))
                        this.NotOperator = true;
                }
                else if (psRuleExpression.Contains(WonkaBreXmlReader.CONST_AL_LE) ||
                         psRuleExpression.Contains(WonkaBreXmlReader.CONST_AL_NOT_LE))
                {
                    this.MinValue = Double.MinValue;
                    this.MaxValue = dValue;

                    if (!bLiteralValue)
                        this.MaxValueProps = AttributeValueProps;

                    if (psRuleExpression.Contains(WonkaBreXmlReader.CONST_AL_NOT_LE))
                        this.NotOperator = true;
                }
                else if (psRuleExpression.Contains(WonkaBreXmlReader.CONST_AL_GE) ||
                         psRuleExpression.Contains(WonkaBreXmlReader.CONST_AL_NOT_GE))
                {
                    this.MinValue = dValue;
                    this.MaxValue = Double.MaxValue;

                    if (!bLiteralValue)
                        this.MinValueProps = AttributeValueProps;

                    if (psRuleExpression.Contains(WonkaBreXmlReader.CONST_AL_NOT_GE))
                        this.NotOperator = true;
                }
                else if (psRuleExpression.Contains(WonkaBreXmlReader.CONST_AL_EQ) ||
                         psRuleExpression.Contains(WonkaBreXmlReader.CONST_AL_NOT_EQ))
                {
                    this.MinValue = dValue;
                    this.MaxValue = dValue;

                    if (!bLiteralValue)
                    {
                        this.MinValueProps = AttributeValueProps;
                        this.MaxValueProps = AttributeValueProps;
                    }

                    if (psRuleExpression.Contains(WonkaBreXmlReader.CONST_AL_NOT_EQ))
                        this.NotOperator = true;
                }
            }
        }
        #endregion

        #region Properties
        public double MinValue { get; set; }

        public WonkaBreRuleValueProps MinValueProps { get; set; }

        public double MaxValue { get; set; }

        public WonkaBreRuleValueProps MaxValueProps { get; set; }

        #endregion
    }
}

