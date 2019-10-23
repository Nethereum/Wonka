using System.Linq;
using System.Text;

using WonkaPrd;

namespace Wonka.BizRulesEngine.RuleTree
{
	/// <summary>
	/// 
	/// This class is the base class for all possible rules types 
	/// (ArithmeticLimitRule, DomainRule, etc.) that can be invoked
	/// during the execution of a RuleTree against the data inside
	/// an instance of WonkaProduct.  It serves as the base for 
	/// both categories: validation rules and assignment rules.
	/// 
	/// NOTE: The "<eval>" tag and body (in the BRE-XML) represents
	/// an instance of WonkaBreRule.
	/// 
	/// </summary>
	public abstract class WonkaBizRule
	{
		#region CONSTANTS

		public const int CONST_MAX_VERBOSE_ERROR_SIZE = 125;

		protected static char[] CONST_RULE_PROP_DELIM      = new char[] { ',' };
		protected static char[] CONST_RULE_ATTR_NAME_DELIM = new char[] { '.' };

		public const string CONST_GEN_PROP_OP = "OP";

		public const string CONST_AL_PROP_MIN = "MIN";
		public const string CONST_AL_PROP_MAX = "MAX";

		public const string CONST_AS_PROP_IDCOL = "IDCOLNAME";
		public const string CONST_AS_PROP_SQGEN = "SEQGEN";
		public const string CONST_AS_PROP_TBL   = "TABLENAME";
		public const string CONST_AS_PROP_COND  = "COND";

		public const string CONST_CX_PROP_CRID = "CRID";
		public const string CONST_CX_PROP_ARGN = "ARGNUM";
		public const string CONST_CX_PROP_ARG  = "ARG";

		public const string CONST_DF_PROP_VALUE = "VALUE";

		public const string CONST_DN_PROP_VALUE = "VALUE";

		public const string CONST_PI_PROP_PIVAL = "PINDVALUE";
		public const string CONST_PI_PROP_PAVAL = "PALTVALUE";
		public const string CONST_PI_PROP_CIVAL = "CINDVALUE";
		public const string CONST_PI_PROP_CAVAL = "CALTVALUE";

		public const string CONST_SP_PROP_CALL = "CALL";
		public const string CONST_SP_PROP_ARGN = "ARGNUM";
		public const string CONST_SP_PROP_ARG  = "ARG";

		public const string CONST_TL_PROP_VAL = "VALUE";

		#endregion

		public WonkaBizRule(int pnRuleId, RULE_TYPE peRuleType)
		{
			RuleId   = pnRuleId;
			RuleType = peRuleType;

			TargetAttribute = new WonkaRef.WonkaRefAttr();
			ParentRuleSetId = -1;

			NotOperator = false;
			IsPassive   = true;

            DescRuleId = "";

            RulesHostEngine = null;
        }

		#region Abstract Methods

		abstract public bool Execute(WonkaProduct  poTransactionRecord,
									 WonkaProduct  poExistingRecord,
									 StringBuilder poErrorMessage);

		abstract public string GetVerboseError(WonkaProduct poTargetProduct);

		#endregion

		#region Implemented Methods

		/// <summary>
		/// 
		/// This method will create the corresponding RuleValueProps that represents the Attribute described
		/// by the provided name.  For example:
		/// 
		/// N.AccountType
		/// 
		/// Would create a RuleValueProps that describes the Attribute 'AccountType' on the new (i.e., transaction)
		/// record.
		/// 
		/// <param name="psAttrName">The name (and possible record prefix) for the Attribute</param>
		/// <returns>The RuleValueProps that represents the provided Attribute name</returns>
		/// </summary>
		public WonkaBizRuleValueProps GetAttributeValueProps(string psAttrName)
		{
			WonkaBizRuleValueProps AttributeValueProps = new WonkaBizRuleValueProps() { IsLiteralValue = false };

			string sRecordOfInterest = "N";

			if (psAttrName.Contains(CONST_RULE_ATTR_NAME_DELIM[0]))
			{
				string[] acAttrNameParts = psAttrName.Split(CONST_RULE_ATTR_NAME_DELIM);

				if (acAttrNameParts.Length > 1)
				{
					sRecordOfInterest = acAttrNameParts[0];
					psAttrName = acAttrNameParts[1];
				}
			}

			if (sRecordOfInterest == "N")
                AttributeValueProps.TargetRecord = TARGET_RECORD.TRID_NEW_RECORD;
			else
				AttributeValueProps.TargetRecord = TARGET_RECORD.TRID_OLD_RECORD;

			AttributeValueProps.AttributeInfo =
				WonkaRef.WonkaRefEnvironment.GetInstance().GetAttributeByAttrName(psAttrName);

			return AttributeValueProps;
		}

		#endregion

		#region Properties
		public int RuleId { get; set; }

		public int ParentRuleSetId { get; set; }

        public string DescRuleId { get; set; }

		public TARGET_RECORD RecordOfInterest { get; set; }

		public WonkaRef.WonkaRefAttr TargetAttribute { get; set; }

		public RULE_TYPE RuleType { get; set; }

		public bool NotOperator { get; set; }

		public bool IsPassive { get; set; }

        public WonkaBizRulesEngine RulesHostEngine { get; set; }

        #endregion
    }
}
