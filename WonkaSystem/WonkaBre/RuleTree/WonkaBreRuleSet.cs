using System.Collections.Generic;

namespace WonkaBre.RuleTree
{
	/// <summary>
	/// 
	/// This class will be the class that acts as a collection of Rules.  (In fact,
	/// all instances of WonkaBreRule must belong to an instance of WonkaBreRuleSet.)  In 
	/// addition to instances of WonkaBreRule, it can contain WonkaBreRuleSets that are
	/// effectively its children.  For example, the following Wonka-Bre markup:
	/// 
	/// <if description="Me and My Enterprise Rules - Wherever I Go, He Goes!">
	///     <criteria op = "AND">
	///         <eval>(N.AccountType) == ('Checking')</eval>
	///         <eval>(N.AccountCurrency) != ('BTC')</eval>
	///     </criteria >
	/// 
	///     <!--description = "For all accounts that aren't BitCoin, the following is required" -->
	///     <validate err="severe">
	///         <criteria op="AND" >
    ///            <eval>(N.BankAccountName) POPULATED</eval>
	///            <eval>(O.AccountCurrValue) POPULATED</eval>
	///            <eval>(N.AccountCurrValue) POPULATED</eval>
	///            <eval>(O.AccountCurrValue) GT (10.00)</eval>
	///            <eval>(N.AccountCurrValue) GT (10.00)</eval>
	///            <eval>(N.AccountCurrValue) GT (O.AccountCurrValue)</eval>
	///         </criteria>
	/// 
	/// Has two RuleSets, in which RuleSet "Me and My..." contains the RuleSet described
	/// as "For all accounts...".  If all the rules within the "Me and My..." RuleSet pass
	/// when evaluating the target product, then its child RuleSet "For all accounts..." 
	/// will be used to evaluate the product.
	/// 
	/// NOTE: If the Attribute has neither a 'O' or 'N' preceding it, it will be assumed to be 
	/// 'N' (i.e., the new record).
	/// 
	/// NOTE: The "<if>" tag and "<validate>" tags (in Wonka-Bre XML) represent
	/// instances of WonkaBreRuleSet.  The "<if>" tag only acts as a placemarker for RuleSets and
	/// general traversal of the RuleTree, but a "<validate>" tag is a a leaf RuleSet who cannot
	/// have children and whose evaluation is vital and worthy of being reported.
	/// 
	/// </summary>
	public class WonkaBreRuleSet
    {
        #region Constructors
        public WonkaBreRuleSet()
        {
            Init();
        }

        public WonkaBreRuleSet(int pnRuleSetId)
        {
            Init();

            this.RuleSetId = pnRuleSetId;
        }

        #endregion

        #region Methods

        public void AddChildRuleSet(WonkaBreRuleSet poChildRuleSet)
        {
            poChildRuleSet.ParentRuleSetId = this.RuleSetId;

            ChildRuleSets.Add(poChildRuleSet);
        }

        public void AddRule(WonkaBreRule poNewRule)
        {
            if (poNewRule.IsPassive)
                EvaluativeRules.Add(poNewRule);
            else
                AssertiveRules.Add(poNewRule);
        }

        private void Init()
        {
            RuleSetId       = -1;
            ParentRuleSetId = -1;

            CustomId          = null;
            CustomFailureMsg  = null;

            ErrorSeverity     = RULE_SET_ERR_LVL.ERR_LVL_NONE;
            RulesEvalOperator = RULE_OP.OP_AND;

            EvaluativeRules = new List<WonkaBreRule>();
            AssertiveRules  = new List<WonkaBreRule>();
            ChildRuleSets   = new List<WonkaBreRuleSet>();
        }

        #endregion

        #region Properties

        public int RuleSetId { get; set; }

        public string Description { get; set; }

        public int ParentRuleSetId { get; set; }

        public RULE_OP RulesEvalOperator { get; set; }

        public RULE_SET_ERR_LVL ErrorSeverity { get; set; }

        public string CustomId { get; set; }

        public string CustomFailureMsg { get; set; }

        public List<WonkaBreRule> EvaluativeRules { get; set; }

        public List<WonkaBreRule> AssertiveRules { get; set; }

        public List<WonkaBreRuleSet> ChildRuleSets { get; set; }

        #endregion
    }
}
