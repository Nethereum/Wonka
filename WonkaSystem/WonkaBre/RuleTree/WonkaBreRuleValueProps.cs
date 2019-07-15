namespace WonkaBre.RuleTree
{
	/// <summary>
	/// 
	/// This class is used by all classes that inherit from WonkaBreRule 
	/// (ArithmeticLimitRule, DomainRule, etc.).  It holds metadata about
	/// the values that are to be compared when executing rules, especially
	/// validation ones.
	/// 
	/// </summary>
	public class WonkaBreRuleValueProps
	{
		public WonkaBreRuleValueProps()
		{
			IsLiteralValue = true;

			TargetRecord = TARGET_RECORD.TRID_NEW_RECORD;

            OpType = STD_OP_TYPE.STD_OP_NONE;

            AttributeInfo = new WonkaRef.WonkaRefAttr();
		}

		public bool IsLiteralValue { get; set; }

        public STD_OP_TYPE OpType { get; set; }

        public TARGET_RECORD TargetRecord { get; set; }

		public WonkaRef.WonkaRefAttr AttributeInfo { get; set; }
	}
}
