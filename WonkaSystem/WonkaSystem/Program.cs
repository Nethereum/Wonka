﻿using System;
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
                var sSenderAddress   = "0xb482fbf39276a48fc608bbd90faa11f12178246a";
                var sPassword        = "eca20e969724c4d52493f37b90b628ee3540751c5d17be7efc2d6cb796c8d398";
                var sContractAddress = "0x0081fc1ed23fc179aa89c5ebc2ad35345a4689e2";

                // SimpleTest(sSenderAddress, sPassword, sContractAddress);

                // NoviceTest(sSenderAddress, sPassword, sContractAddress);

                // CQSDemoTest(sSenderAddress, sPassword, sContractAddress);

                string sOrchTestContractAddress = "0x945b91f25d809fce26c144244136fc11a50b8409";
                SimpleOrchestrationTest(sSenderAddress, sPassword, sContractAddress, sOrchTestContractAddress);
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

            SimpleOrchTest.Execute(psOrchTestContractAddress);
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
    }
}