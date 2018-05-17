using System;
using System.Linq;
using System.Text;

using WonkaPrd;
using WonkaRef;

namespace WonkaBre.RuleTree.RuleTypes
{
	/// <summary>
	/// 
	/// This class represents the an assignment rule, which is an aggressive rule that will
	/// assign data. For example, the following Wonka-Bre markup:
	/// 
	///  <validate err="severe">
	///     <criteria op = "AND" >
	///         <eval>(N.AccountCurrValue) GT (10.00)</eval>
	///         <eval>(N.AccountCurrValue) LT (100000000.00)</eval>
	///         <eval>(N.AccountStatus) ASSIGN ('ACT')</eval>
	///     </criteria >
	/// 
	/// Has two instances of the ArithmeticLimitRule that focus on the Attribute 'AccountCurrValue'.
	/// One instance evaluates whether the value of the incoming record if greater than 10.00, and 
	/// the other instance evaluates whether the incoming value is less than 100000000.0.  If both rules 
	/// are evaluated to TRUE, then the assignment rule below them will be invoked, setting 
	/// the AccountStatus on the incoming record to "ACT".
	/// 
	/// NOTE: Since the WonkaProduct only records Attribute values as Strings, it will not validate
	/// the data type before assignment.
	/// 
	/// NOTE: If the Attribute has neither a 'O' or 'N' preceding it, it will be assumed to be 'N'.
	///  
	/// </summary>
	public class AssignmentRule : WonkaBreRule
    {
        #region Constructors

        public AssignmentRule() : base(-1, RULE_TYPE.RT_ASSIGNMENT)
        {
            Init(TARGET_RECORD.TRID_NONE, -1, null);
        }

        public AssignmentRule(int pnRuleID) : base(pnRuleID, RULE_TYPE.RT_ASSIGNMENT)
        {
            Init(TARGET_RECORD.TRID_NONE, -1, null);
        }

        public AssignmentRule(int pnRuleID, TARGET_RECORD peTargetRecord, int pnTargetAttrId, string psAssignValue) : 
            base(pnRuleID, RULE_TYPE.RT_ASSIGNMENT)
        {
            Init(peTargetRecord, pnTargetAttrId, psAssignValue);

            this.AssignValueProps =
                new WonkaBreRuleValueProps()
                {
                    IsLiteralValue = true,
                    TargetRecord   = TARGET_RECORD.TRID_NONE,
                    AttributeInfo  = WonkaRefEnvironment.GetInstance().GetAttributeByAttrId(pnTargetAttrId)
                };
        }

        public AssignmentRule(int pnRuleID, TARGET_RECORD peTargetRecord, int pnTargetAttrId, TARGET_RECORD peAssignRecord, string psAssignAttrName) :
            base(pnRuleID, RULE_TYPE.RT_ASSIGNMENT)
        {
            Init(peTargetRecord, pnTargetAttrId, psAssignAttrName);

            this.AssignValueProps =
                new WonkaBreRuleValueProps()
                {
                    IsLiteralValue = false,
                    TargetRecord   = peAssignRecord,
                    AttributeInfo  = WonkaRefEnvironment.GetInstance().GetAttributeByAttrName(psAssignAttrName)
                };
        }

        #endregion

        #region Methods

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
            bool bResult      = false;
            bool bAssignValue = true;

            int nAttrId  = TargetAttribute.AttrId;
            int nGroupId = TargetAttribute.GroupId;

            WonkaProduct        TargetRecord = null;
            WonkaRefEnvironment WonkaRefEnv  = WonkaRefEnvironment.GetInstance();

            if (poTransactionRecord == null)
                throw new Exception("ERROR!  The new Product is null.");

            if (poCurrentRecord == null)
                throw new Exception("ERROR!  The old Product is null.");

            if (RecordOfInterest == TARGET_RECORD.TRID_NEW_RECORD)
                TargetRecord = poTransactionRecord;
            else if (RecordOfInterest == TARGET_RECORD.TRID_OLD_RECORD)
                TargetRecord = poCurrentRecord;
            else
                throw new Exception("ERROR!  The target record is none!");

            if (DefaultAssignment)
            {
                WonkaPrdGroup TempProductGroup = null;

                if (RecordOfInterest == TARGET_RECORD.TRID_NEW_RECORD)
                    TempProductGroup = poTransactionRecord.GetProductGroup(nGroupId);
                else
                    TempProductGroup = poCurrentRecord.GetProductGroup(nGroupId);

                string sCurrentValue = "";

                if (TempProductGroup.GetRowCount() > 0)
                {
                    if (TempProductGroup[0].ContainsKey(nAttrId))
                        sCurrentValue = TempProductGroup[0][nAttrId];
                }

                if (!String.IsNullOrEmpty(sCurrentValue))
                    bAssignValue = false;
            }

            if (bAssignValue)
            {
                RefreshAssignValue(poTransactionRecord, poCurrentRecord);

                WonkaPrdGroup TempProductGroup = null;

                if (RecordOfInterest == TARGET_RECORD.TRID_NEW_RECORD)
                    TempProductGroup = poTransactionRecord.GetProductGroup(nGroupId);
                else
                    TempProductGroup = poCurrentRecord.GetProductGroup(nGroupId);

                TempProductGroup[0][nAttrId] = AssignValue;
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
            return "";
        }

        private void Init(TARGET_RECORD peTargetRecord, int pnTargetAttrId, string psAssignValue)
        {
            this.IsPassive = false;

            this.RecordOfInterest = peTargetRecord;

            if (pnTargetAttrId > 0)
                this.TargetAttribute  = WonkaRefEnvironment.GetInstance().GetAttributeByAttrId(pnTargetAttrId);

            this.AssignValue      = psAssignValue;
            this.AssignValueProps = new WonkaBreRuleValueProps();
        }

        /// <summary>
        /// 
        /// This method will refresh the assignment value if it's not literal (i.e., it's an Attribute on a record).
        /// 
        /// <param name="poNewProduct">The incoming Product whose Attributes might be used to populate the Set</param>
        /// <param name="poOldProduct">The current Product whose Attributes might be used to populate the Set</param>
        /// <returns>Indicator of whether the assignment value was refreshed successfully</returns>
        /// </summary>
        private bool RefreshAssignValue(WonkaProduct poNewProduct, WonkaProduct poOldProduct)
        {
            bool bResult  = true;
            int  nAttrId  = 0;
            int  nGroupId = 0;

            if (!this.AssignValueProps.IsLiteralValue)
            {
                this.AssignValue = "";

                if (poNewProduct == null)
                    throw new Exception("ERROR!  The new Product is null.");

                if (poNewProduct == null)
                    throw new Exception("ERROR!  The old Product is null.");

                nAttrId  = AssignValueProps.AttributeInfo.AttrId;
                nGroupId = AssignValueProps.AttributeInfo.GroupId;

                WonkaPrdGroup TempProductGroup = null;

                if (AssignValueProps.TargetRecord == TARGET_RECORD.TRID_NEW_RECORD)
                    TempProductGroup = poNewProduct.GetProductGroup(nGroupId);
                else
                    TempProductGroup = poOldProduct.GetProductGroup(nGroupId);

                if (TempProductGroup.DataRowVector.Count > 0)
                {
                    if ( (TempProductGroup[0].Count > 0) && TempProductGroup[0].ContainsKey(nAttrId) )
                        this.AssignValue = TempProductGroup[0][nAttrId];
                }
            }

            return bResult;
        }

        /// <summary>
        /// 
        /// This method will use the provided value(s) to set the assign value of this rule.  More importantly, 
        /// it will assist with the initialization of the rule by interpreting the provided value(s) 
        /// to detect which provided values are literal and which of them are Attribute names (which 
        /// will be used to dynamically update the assign value using every processed pair of records).
        /// 
        /// <param name="asAssignValues">The value(s) used for the assignment</param>
        /// <returns>None</returns>
        /// </summary>
        public void SetAssignValue(string[] asAssignValues)
        {
            if (asAssignValues.Count() > 0)
            {
                string sTempDomainVal = asAssignValues[0];

                int nLiteralValueStartIdx = sTempDomainVal.IndexOf("'");
                if (nLiteralValueStartIdx >= 0)
                {
                    int nLiteralValueEndIdx = sTempDomainVal.LastIndexOf("'");

                    if (nLiteralValueEndIdx > nLiteralValueStartIdx)
                    {
                        string sLiteralValue =
                            sTempDomainVal.Substring(nLiteralValueStartIdx + 1, nLiteralValueEndIdx - nLiteralValueStartIdx - 1);

                        this.AssignValue = sLiteralValue;
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
                                eTargetRecord = TARGET_RECORD.TRID_OLD_RECORD;
                        }
                    }
                    else
                        sAttrName = sTempDomainVal;

                    this.AssignValue = sAttrName;

                    this.AssignValueProps.IsLiteralValue = false;
                    this.AssignValueProps.TargetRecord   = eTargetRecord;
                    this.AssignValueProps.AttributeInfo  = WonkaRefEnvironment.GetInstance().GetAttributeByAttrName(sAttrName);
                }
            }
        }

        #endregion

        #region Properties

        public string AssignValue { get; set; }

        public WonkaBreRuleValueProps AssignValueProps { get; set; }

        public bool DefaultAssignment = false;

        #endregion
    }
}
