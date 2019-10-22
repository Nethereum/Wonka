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
                var sSenderAddress   = "0x5095101f2dd4b4c4df1bfbaeb1391e8db4d10554";
                var sPassword        = "e685eb6a1f71a07746ec6e65b0ae9864db14881759d8cd981100b689b7401a0f";
                var sContractAddress = "0xeaed9c1082998971e56ac31512b08e3457644666";

                // SimpleTest();

                // SimpleTest(sSenderAddress, sPassword, sContractAddress);

                // NoviceTest(sSenderAddress, sPassword, sContractAddress);

                NoviceOnlineChainTest();

                // CQSDemoTest(sSenderAddress, sPassword, sContractAddress);

                // string sOrchTestContractAddress = "0x5f2d3b580e45ea133d368c66fff30cdd211a9372";
                // SimpleOrchestrationTest(sSenderAddress, sPassword, sContractAddress, sOrchTestContractAddress);

                // string sOrchTestContractAddress = "0x2843bb92fbdfcad057c6eea8ac37a098ae3f4077";
                // SimpleCustomOpsTest(sSenderAddress, sPassword, sContractAddress, sOrchTestContractAddress);

                string sOrchTestContractAddress = "0x3edf86c3ee590f9693393b8289c533e6cb0d6efa";
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

        static void NoviceOnlineChainTest()
        {
            var OnlineChainTest = new WonkaNoviceOnlineChainTest("", false, true);

            OnlineChainTest.Execute();
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
            // WonkaCQSOrchTest CQSTest = new WonkaCQSOrchTest(psSenderAddress, psPassword, psContractAddress, psOrchestrationAddress);

            WonkaCQSOrchTest CQSTest = new WonkaCQSOrchTest();
            CQSTest.Execute();
        }
    }
}
