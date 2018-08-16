using System;
using System.IO;
using System.Text;

using WonkaBre;
using WonkaEth.Orchestration;
using WonkaBre.Reporting;
using WonkaPrd;
using WonkaRef;

using WonkaSystem.CQS.Contracts;

namespace WonkaSystem.CQS.Generation
{
    /**
     ** NOTE: Assumption at this point is that the contract has already been deployed to the blockchain
     **/
    public class SalesTransactionGenerator : AbstractWonkaOrchestrator<SalesTrxCreateCommand>
    {
        public SalesTransactionGenerator(SalesTrxCreateCommand command, StringBuilder psRulesContents, OrchestrationInitData poInitData) :
            base(command, psRulesContents, poInitData)
        {
            // NOTE: Not necessary here
            // base.SerializeRulesEngineToBlockchain();
        }

        public bool GenerateSalesTransaction(SalesTrxCreateCommand poCommand)
        {
            // First, let's provide the input to the orchestration (i.e., sending the data to their proper contract destinations)
            base.SerializeRecordToBlockchain(poCommand);

            bool bValid = base.Orchestrate(poCommand);

            // Then, we pull back the values from the contract(s) in order to assemble our record after the rules engine has executed 
            // and possibly set data points
            base.DeserializeRecordFromBlockchain(poCommand);

            // NOTE: Should anything else be done here to override the base method, like capturing events
            //       broadcast from the blockchain?

            return bValid;
        }
    }
}