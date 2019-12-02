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
            RuleTrees = new List<WonkaBizRulesEngine>();
        }

        public WonkaBizGrove(int pnGroveId, string psGroveDesc)
        {
            GroveId   = pnGroveId;
            GroveDesc = psGroveDesc;
            RuleTrees = new List<WonkaBizRulesEngine>();
        }

        public int GroveId { get; }

        public string GroveDesc { get; }

        public List<WonkaBizRulesEngine> RuleTrees { get; set; }
    }
}
