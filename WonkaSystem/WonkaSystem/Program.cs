using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WonkaSystem.TestHarness;

namespace WonkaSystem
{
    /// <summary>
    /// 
    /// This class will serve as the test harness for this solution.
    /// 
    /// NOTE: At this point, the code to deploy the Ethereum contracts does not yet work.  So, this code will
    ///       only perform successfully if the contract has already been deployed using the Truffle project 
    ///       in the Solidity directory of the repo.
    ///
    /// NOTE: We are only issuing a call() now when we execute the rules engine,
    ///       since we are only looking to validate here.  However, there is a chance 
    ///       that sendTransaction() might be used in some cases for the future, because 
    ///       we might wish for the rules engine to alter the record.
    /// 
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var sSenderAddress   = "0x143be901d26e70b721c1bf21619d6639463f51aa";
                var sPassword        = "fa1f12037f1859e68b85962a379be3601436c0a8ad92cc8ed7e95d854c0c277e";
                var sContractAddress = "0x63bd21baa147317a11817dc80137eb948913fe53";

                // SimpleTest();

                // SimpleTest(sSenderAddress, sPassword, sContractAddress);

                // NoviceTest(sSenderAddress, sPassword, sContractAddress);

                // CQSDemoTest(sSenderAddress, sPassword, sContractAddress);

                // string sOrchTestContractAddress = "0x5f2d3b580e45ea133d368c66fff30cdd211a9372";
                // SimpleOrchestrationTest(sSenderAddress, sPassword, sContractAddress, sOrchTestContractAddress);

                // string sOrchTestContractAddress = "0x2843bb92fbdfcad057c6eea8ac37a098ae3f4077";
                // SimpleCustomOpsTest(sSenderAddress, sPassword, sContractAddress, sOrchTestContractAddress);

                string sOrchTestContractAddress = "0x2b0557f5a329a791c5280b608b3c7d0e772b7b3a";
                CQSOrchestrationDemoTest(sSenderAddress, sPassword, sContractAddress, sOrchTestContractAddress);
            }
            catch (WonkaEth.Validation.WonkaValidatorException ex)
            {
                string sErrMsg = ex.ToString();

                System.Console.WriteLine("ERROR!  (" + ex.RuleTreeReport.NumberOfRuleFailures + ") rules failed during execution.");
            }
            catch (Exception ex)
            {
                string sErrMsg = ex.ToString();

                System.Console.WriteLine(ex);
            }

            return;
        }

        static void SimpleOrchestrationTest(string psSenderAddress, string psPassword, string psContractAddress, string psOrchTestContractAddress)
        {
            WonkaSimpleOrchestrationTest SimpleOrchTest = new WonkaSimpleOrchestrationTest(psSenderAddress, psPassword, psContractAddress);

            bool bValidateWithinTransaction = true;

            SimpleOrchTest.Execute(psOrchTestContractAddress, bValidateWithinTransaction);
        }

        static void SimpleCustomOpsTest(string psSenderAddress, string psPassword, string psContractAddress, string psOrchTestContractAddress)
        {
            WonkaSimpleCustomOpsTest SimpleOrchTest = new WonkaSimpleCustomOpsTest(psSenderAddress, psPassword, psContractAddress);
            //WonkaSimpleCustomOpsTest SimpleOrchTest = new WonkaSimpleCustomOpsTest(psSenderAddress, psPassword, psContractAddress, false);

            bool bValidateWithinTransaction = true;

            SimpleOrchTest.Execute(psOrchTestContractAddress, bValidateWithinTransaction);
            // SimpleOrchTest.Execute();
        }

        static void SimpleTest()
        {
            WonkaSimpleTest SimpleTest = new WonkaSimpleTest();
            SimpleTest.Execute();
        }

        static void SimpleTest(string psSenderAddress, string psPassword, string psContractAddress)
        {
            WonkaSimpleNethereumTest SimpleNethTest = new WonkaSimpleNethereumTest(psSenderAddress, psPassword, psContractAddress);
            SimpleNethTest.Execute();
        }

        static void NoviceTest(string psSenderAddress, string psPassword, string psContractAddress)
        {
            bool bSerializeMDAndEngine = true;

            WonkaNoviceNethereumTest DeployNethTest =
                new WonkaNoviceNethereumTest(psSenderAddress, psPassword, psContractAddress, bSerializeMDAndEngine);

            // var isProductValid = DeployNethTest.Execute();
            var rulesReport = DeployNethTest.ExecuteWithReport();
        }

        static void CQSDemoTest(string psSenderAddress, string psPassword, string psContractAddress)
        {         
            WonkaCQSTest CQSTest = new WonkaCQSTest(psSenderAddress, psPassword, psContractAddress);
            CQSTest.Execute();
        }

        static void CQSOrchestrationDemoTest(string psSenderAddress, string psPassword, string psContractAddress, string psOrchestrationAddress)
        {
            WonkaCQSOrchTest CQSTest = new WonkaCQSOrchTest(psSenderAddress, psPassword, psContractAddress, psOrchestrationAddress);
            CQSTest.Execute();
        }
    }
}
