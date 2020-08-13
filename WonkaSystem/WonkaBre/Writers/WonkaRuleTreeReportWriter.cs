using System;
using System.Text;

using Wonka.BizRulesEngine.Reporting;

namespace Wonka.BizRulesEngine.Writers
{
	public class WonkaRuleTreeReportWriter
    {
        private IRuleTreeReport miReport = null;

		public WonkaRuleTreeReportWriter(IRuleTreeReport piReport)
		{
            miReport = piReport;
        }

        ///
        /// <summary>
        /// 
        /// This method will write the XML (i.e., Wonka rules markup) of a IRuleTreeReport.
		///
		/// NOTE: UNDER CONSTRUCTION
		///
        /// <returns>Returns the XML payload that represents a IRuleTreeReport</returns>
        /// </summary>
        public string ExportXmlString()
        {
            string sIndentSpaces = "    ";

            StringBuilder sbExportXmlString = new StringBuilder("<?xml version=\"1.0\"?>\n<RuleTreeReport>\n");            

            sbExportXmlString.Append(sIndentSpaces).Append("<RuleSetFailCount>").Append(miReport.GetRuleSetSevereFailureCount()).Append("</RuleSetFailCount>\n");

            sbExportXmlString.Append(sIndentSpaces).Append("<RuleSetWarnCount>").Append(miReport.GetRuleSetWarningCount()).Append("</RuleSetWarnCount>\n");

            sbExportXmlString.Append(sIndentSpaces).Append("<RuleTreeStartTime>").Append(miReport.GetRuleTreeStartTime().ToString("yyyy-MM-dd HH:mm:ss:fffffff")).Append("</RuleTreeStartTime>\n");

            sbExportXmlString.Append(sIndentSpaces).Append("<RuleTreeEndTime>").Append(miReport.GetRuleTreeEndTime().ToString("yyyy-MM-dd HH:mm:ss:fffffff")).Append("</RuleTreeEndTime>\n");

            sbExportXmlString.Append(sIndentSpaces).Append("<WasExecutedOnChain>").Append(miReport.WasExecutedOnChain()).Append("</WasExecutedOnChain>\n");

            sbExportXmlString.Append(sIndentSpaces).Append("<WasExecutedSuccessfully>").Append(miReport.WasExecutedSuccessfully()).Append("</WasExecutedSuccessfully>\n");

            if (!String.IsNullOrEmpty(miReport.GetErrorString()))
                sbExportXmlString.Append(sIndentSpaces).Append("<ErrorString>").Append(miReport.GetErrorString()).Append("</ErrorString>\n");

            if (miReport.WasExecutedOnChain() && (miReport.GetGasUsed() > 0))
                sbExportXmlString.Append(sIndentSpaces).Append("<GasUsed>").Append(miReport.GetGasUsed()).Append("</GasUsed>\n");

            sbExportXmlString.Append("</RuleTreeReport>\n");

            return sbExportXmlString.ToString();
        }

	}
}