using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wonka.BizRulesEngine.Reporting
{
    /// <summary>
    /// 
    /// This class will contain data about the evaluations of executed RuleTrees
    /// (i.e., instances of WonkaBizRulesEngine) within a Grove.
    /// 
    /// </summary>
    public class WonkaBizGroveReport
    {
        public WonkaBizGroveReport(WonkaBizGrove poGrove)
        {
            this.GroveOrigin        = poGrove;
            this.OverallGroveResult = ERR_CD.CD_SUCCESS;
        }

        #region Properties

        public ERR_CD OverallGroveResult { get; set; }

        public WonkaBizGrove GroveOrigin { get; }

        public Dictionary<string, IRuleTreeReport> RuleTreeReports { get; set; }

        #endregion

    }
}