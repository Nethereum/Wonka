using System;
using System.Text;

using WonkaPrd;
using WonkaRef;

namespace Wonka.BizRulesEngine.RuleTree.RuleTypes
{
	/// <summary>
	/// 
	/// This class represents the type of validation rule where an Attribute of a Product is
	/// evaluated to detect if it's populated with a value.  For example, the following Wonka-Bre markup:
	/// 
	///  <if description="Minimum Required Fields for a Product Save">
	///      <criteria op = "AND">
	///          <eval>(N.AccountCurrValue) POPULATED</eval>
	///          <eval>(N.AccountStatus) POPULATED</eval>
	///          <eval>(N.AccountType) POPULATED</eval>
	///      </criteria>
	/// 
    /// Has three instances of the Populated rule.  Respectively, they detect whether the following Attributes
    /// of the incoming record are populated: AccountCurrValue, AccountStatus, and AccountType. 
	/// 
	/// </summary>
	public class PopulatedRule : WonkaBizRule
    {
        #region Constructors

        public PopulatedRule() : base(-1, RULE_TYPE.RT_POPULATED)
        {
            Init(TARGET_RECORD.TRID_NONE, -1);
        }

        public PopulatedRule(int pnRuleID, TARGET_RECORD peTargetRecord, int pnTargetAttrId) : base(pnRuleID, RULE_TYPE.RT_DOMAIN)
        {
            Init(peTargetRecord, pnTargetAttrId);
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// This method will apply the populated rule to either the transaction record or the current (i.e., database) record, 
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
                bResult = true;
            else
                bResult = false;

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
            string sAttrName = this.TargetAttribute.AttrName;                

            StringBuilder VerboseErrorBuilder = new StringBuilder();

            VerboseErrorBuilder.Append(sAttrName);
            VerboseErrorBuilder.Append(" ");
            VerboseErrorBuilder.Append(this.NotOperator ? "must not be populated" : "must be populated");

            if (VerboseErrorBuilder.Length > (CONST_MAX_VERBOSE_ERROR_SIZE + 3))
            {
                VerboseErrorBuilder.Remove(CONST_MAX_VERBOSE_ERROR_SIZE, (VerboseErrorBuilder.Length - CONST_MAX_VERBOSE_ERROR_SIZE));
                VerboseErrorBuilder.Append("...");
            }

            return VerboseErrorBuilder.ToString();
        }

        private void Init(TARGET_RECORD peTargetRecord, int pnTargetAttrId)
        {
            this.IsPassive = true;

            this.RecordOfInterest = peTargetRecord;

            if (pnTargetAttrId > 0)
                this.TargetAttribute  = WonkaRefEnvironment.GetInstance().GetAttributeByAttrId(pnTargetAttrId);
        }

        #endregion

        #region Properties
        #endregion
    }
}