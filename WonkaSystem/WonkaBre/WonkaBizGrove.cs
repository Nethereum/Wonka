using System;
using System.Collections.Generic;

namespace Wonka.BizRulesEngine
{
    public class WonkaBizGrove
    {
        public WonkaBizGrove()
        {
            GroveId   = 0;
            GroveDesc = String.Empty;

            RuleTreeMembers         = new List<WonkaBizRulesEngine>();
            ExecuteRuleTreesOnChain = new HashSet<WonkaBizRulesEngine>();
        }

        public WonkaBizGrove(int pnGroveId, string psGroveDesc)
        {
            GroveId   = pnGroveId;
            GroveDesc = psGroveDesc;

            RuleTreeMembers         = new List<WonkaBizRulesEngine>();
            ExecuteRuleTreesOnChain = new HashSet<WonkaBizRulesEngine>();
        }

        #region Methods

        public void AddRuleTree(WonkaBizRulesEngine poNewRuleTree, bool pbExecuteOnChain = true)
        {
            poNewRuleTree.GroveId    = GroveDesc;
            poNewRuleTree.GroveIndex = (uint) RuleTreeMembers.Count;

            RuleTreeMembers.Add(poNewRuleTree);

            if (pbExecuteOnChain)
                ExecuteRuleTreesOnChain.Add(poNewRuleTree);
        }

        public void SetRuleTreeToExecuteOnChain(WonkaBizRulesEngine poTargetRuleTree, bool pbExecuteOnChain = true)
        {
            if (RuleTreeMembers.Contains(poTargetRuleTree))
            {
                if (pbExecuteOnChain)
                    ExecuteRuleTreesOnChain.Add(poTargetRuleTree);
                else
                    ExecuteRuleTreesOnChain.Remove(poTargetRuleTree);
            }
            else
                AddRuleTree(poTargetRuleTree, pbExecuteOnChain);
        }

        #endregion

        public int GroveId { get; }

        public string GroveDesc { get; }

        public List<WonkaBizRulesEngine> RuleTreeMembers { get; }

        public HashSet<WonkaBizRulesEngine> ExecuteRuleTreesOnChain { get; }
    }
}
