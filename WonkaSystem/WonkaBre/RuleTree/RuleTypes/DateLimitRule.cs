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
	/// This class represents the type of validation rule where an date Attribute of a Product is
	/// evaluated against a range of dates.  For example, the following Wonka-Bre markup:
	/// 
	///  <validate err="severe">
	///     <criteria op="AND" >
	///           <eval>(N.CreationDt) IS BEFORE ('01/01/1900')</eval>
	///     </criteria >
	/// 
	/// Has one instances of the DateLimitRule that focuses on the Attribute 'CreationDt'.
	/// It evaluates whether the value is before 01/01/1900.
	///  
	/// </summary>
	public class DateLimitRule : WonkaBreRule
    {
        #region CONSTANTS
        private const string CONST_WONKA_DATETIME_FORMAT = "MM/dd/yyyy";
        private const string CONST_CURRENT_DATETIME_IND  = "TODAY";
        #endregion

        #region Constructors
        public DateLimitRule() : base(-1, RULE_TYPE.RT_DATE_LIMIT)
        {
            Init(TARGET_RECORD.TRID_NONE, -1);
        }

        public DateLimitRule(int pnRuleId) : base(pnRuleId, RULE_TYPE.RT_ARITH_LIMIT)
        {
            Init(TARGET_RECORD.TRID_NONE, -1);
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// This method will apply the date limit rule to either the transaction record or the current (i.e., database) 
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
                    DateTime TargetDate = DateTime.Parse(sTargetData);

                    int nMinResult = DateTime.Compare(TargetDate, this.MinValue);
                    int nMaxResult = DateTime.Compare(TargetDate, this.MaxValue);

                    if ((nMinResult > 0) && (nMaxResult < 0))
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
            string sAttrName     = TargetAttribute.AttrName;

            if (MinValue == MaxValue)
            {
                sVerboseError =
                    String.Format("Rule ({0} {1} {2}) fails.",
                                  sAttrName,
                                  NotOperator ? "!=" : "==",
                                  MaxValue.ToString(CONST_WONKA_DATETIME_FORMAT));
            }
            else if (NotOperator)
            {
                sVerboseError =
                    String.Format("Rule (({0} < {1}) OR ({0} > {2})) fails.", 
                                  sAttrName, 
                                  MinValue.ToString(CONST_WONKA_DATETIME_FORMAT), 
                                  MaxValue.ToString(CONST_WONKA_DATETIME_FORMAT));
            }
            else
            {
                sVerboseError =
                    String.Format("Rule ({0} <= {1} <= {2}) fails.", 
                                  MinValue.ToString(CONST_WONKA_DATETIME_FORMAT), 
                                  sAttrName, 
                                  MaxValue.ToString(CONST_WONKA_DATETIME_FORMAT));
            }

            if (sVerboseError.Length > (CONST_MAX_VERBOSE_ERROR_SIZE + 3))
            {
                sVerboseError = sVerboseError.Substring(0, CONST_MAX_VERBOSE_ERROR_SIZE);

                sVerboseError += "...";
            }

            return sVerboseError;
        }

        private void Init(TARGET_RECORD peTargetRecord, int pnTargetAttrId)
        {
            this.MinValue      = DateTime.MinValue;
            this.MinValueProps = new WonkaBreRuleValueProps() { IsLiteralValue = true };

            this.MaxValue      = DateTime.MaxValue;
            this.MaxValueProps = new WonkaBreRuleValueProps() { IsLiteralValue = true };

            this.IsPassive        = true;
            this.RecordOfInterest = peTargetRecord;

            if (pnTargetAttrId > 0)
                this.TargetAttribute = WonkaRefEnvironment.GetInstance().GetAttributeByAttrId(pnTargetAttrId);
        }

        /// <summary>
        /// 
        /// If either the min and max values are set to be dynamic (i.e., subject to record values),
        /// this method will update the provided values for the min and max of this rule and use the
        /// appropriate target records to do so.
        /// 
        /// <param name="poTransactionRecord">The incoming (i.e., transaction) record</param>
        /// <param name="poCurrentRecord">The old (i.e., current) record</param>
        /// <returns>None</returns>
        /// </summary>
        private void RefreshMinAndMax(WonkaProduct poTransactionRecord, WonkaProduct poCurrentRecord)
        {
            string sTempAttrValue = null;

            if (!this.MinValueProps.IsLiteralValue)
            {
                int nAttrId  = this.MinValueProps.AttributeInfo.AttrId;
                int nGroupId = this.MinValueProps.AttributeInfo.GroupId;

                WonkaPrdGroup TempProductGroup = null;

                if (this.MinValueProps.TargetRecord == TARGET_RECORD.TRID_NEW_RECORD)
                    TempProductGroup = poTransactionRecord.GetProductGroup(nGroupId);
                else
                    TempProductGroup = poCurrentRecord.GetProductGroup(nGroupId);

                sTempAttrValue = TempProductGroup[0][nAttrId];

                if (!String.IsNullOrEmpty(sTempAttrValue))
                    this.MinValue = DateTime.ParseExact(sTempAttrValue, CONST_WONKA_DATETIME_FORMAT, null);
                else
                    this.MinValue = DateTime.MinValue;
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

                sTempAttrValue = TempProductGroup[0][nAttrId];

                if (!String.IsNullOrEmpty(sTempAttrValue))
                    this.MaxValue = DateTime.ParseExact(sTempAttrValue, CONST_WONKA_DATETIME_FORMAT, null);
                else
                    this.MaxValue = DateTime.MaxValue;
            }
        }

        /// <summary>
        /// 
        /// This method will use the provided values to set the min and max of this rule.  More importantly, 
        /// it will assist with the initialization of the rule by interpreting the provided values 
        /// to detect which values are literal and which of them are Attribute names (which 
        /// will be used to dynamically update the value(s) on every processed pair of records).
        /// 
        /// <param name="psRuleExpression">The date rule in verbose form</param>
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

                DateTime             DateTimeValue       = DateTime.Now;
                WonkaBreRuleValueProps AttributeValueProps = new WonkaBreRuleValueProps() { IsLiteralValue = false };

                try
                {
                    DateTimeValue = DateTime.ParseExact(sTempValue, CONST_WONKA_DATETIME_FORMAT, null);
                    bLiteralValue = true;
                }
                catch (Exception ex)
                {
                    if (sTempValue == CONST_CURRENT_DATETIME_IND)
                    {
                        DateTimeValue = DateTime.Now;
                        bLiteralValue = true;
                    }
                    else
                        AttributeValueProps = this.GetAttributeValueProps(sTempValue);
                }

                if (psRuleExpression.Contains(WonkaBreXmlReader.CONST_DL_IB) ||
                    psRuleExpression.Contains(WonkaBreXmlReader.CONST_DL_NOT_IB))
                {
                    this.MinValue = DateTime.MinValue;
                    this.MaxValue = DateTimeValue;

                    if (!bLiteralValue)
                        this.MaxValueProps = AttributeValueProps;

                    if (psRuleExpression.Contains(WonkaBreXmlReader.CONST_DL_NOT_IB))
                        this.NotOperator = true;
                }
                else if (psRuleExpression.Contains(WonkaBreXmlReader.CONST_DL_IA) ||
                         psRuleExpression.Contains(WonkaBreXmlReader.CONST_DL_NOT_IA))
                {
                    this.MinValue = DateTimeValue;
                    this.MaxValue = DateTime.MaxValue;

                    if (!bLiteralValue)
                        this.MinValueProps = AttributeValueProps;

                    if (psRuleExpression.Contains(WonkaBreXmlReader.CONST_DL_NOT_IA))
                        this.NotOperator = true;
                }
            }
        }
        #endregion

        #region Properties

        public DateTime MinValue { get; set; }

        public WonkaBreRuleValueProps MinValueProps { get; set; }

        public DateTime MaxValue { get; set; }

        public WonkaBreRuleValueProps MaxValueProps { get; set; }

        #endregion
    }
}
