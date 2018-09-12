using System;
using System.IO;
using System.Text;

using WonkaEth.Orchestration;
using WonkaEth.Orchestration.Init;

using WonkaSystem.CQS.Contracts;

namespace WonkaSystem.CQS.Generation
{
    /**
     ** NOTE: Assumption at this point is that the RulesEngine contract has already been deployed to the blockchain
     **/
    public class SalesTransactionGenerator : AbstractWonkaOrchestrator<SalesTrxCreateCommand>
    {
        public SalesTransactionGenerator(SalesTrxCreateCommand command, StringBuilder psRulesContents, OrchestrationInitData poInitData) :
            base(command, psRulesContents, poInitData, "NewSaleGroup", 1)
        {
            // NOTE: Not necessary here
            // base.SerializeRulesEngineToBlockchain();
        }

        public bool GenerateSalesTransaction(SalesTrxCreateCommand poCommand)
        {
            bool bSimulationMode = true;

            // First, let's provide the input to the orchestration (i.e., sending the data to their proper contract destinations)
            base.SerializeRecordToBlockchain(poCommand);

            // Next, we simulate the execution on the blockchain in order to save on gas (i.e., no persistence) - if all seems well 
            // (i.e., all data points retrieved appear valid), we then perform the actual execution of the RuleTree on the blockchain, 
            // persistence and all
            bool bValid = base.Orchestrate(poCommand, bSimulationMode);
            if (bValid)
                bValid = base.Orchestrate(poCommand);

            // Then, we pull back the values from the contract(s) in order to assemble our record after the rules engine has executed 
            // and possibly set data points
            base.DeserializeRecordFromBlockchain(poCommand);

            // NOTE: Should anything else be done here to override the base method, like capturing events
            //       broadcast from the blockchain?

            return bValid;
        }

        public override bool ValidateCommand(SalesTrxCreateCommand poCommand)
        {
            // NOTE: Put additional validation logic here

            return true;
        }

    }
}