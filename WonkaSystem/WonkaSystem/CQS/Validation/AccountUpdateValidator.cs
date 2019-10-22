using System;
using System.IO;
using System.Text;

using Wonka.BizRulesEngine;
using WonkaEth.Validation;
using Wonka.BizRulesEngine.Reporting;
using WonkaPrd;
using WonkaRef;

using WonkaSystem.CQS.Contracts;

namespace WonkaSystem.CQS.Validation
{
    /**
     ** NOTE: Assumption at this point is that the contract has already been deployed to the blockchain
     **/
    public class AccountUpdateValidator : AbstractWonkaValidator<AccountUpdateCommand>
    {
        public AccountUpdateValidator(AccountUpdateCommand command, string psRulesFilepath, string psWeb3HttpUrl = null)
            : base(command, psRulesFilepath, psWeb3HttpUrl)
        {
        }

        public AccountUpdateValidator(AccountUpdateCommand command, StringBuilder psRulesContents, string psWeb3HttpUrl = null)
            : base(command, psRulesContents, psWeb3HttpUrl)
        {
        }

        public AccountUpdateValidator(AccountUpdateCommand command, FileInfo psRulesFile, string psWeb3HttpUrl = null)
            : base(command, psRulesFile.FullName, psWeb3HttpUrl)
        {
        }

        public override WonkaBreRuleTreeReport SimulateValidate(AccountUpdateCommand poCommand) 
        {
            /**
             ** NOTE: Since the Ethereum engine does not currently support record notation as specified in the markup 
             ** (where O.* indicates existing records in the blockchain, N.* indicates new records about to be fed into the blockchain, etc.),
             ** we don't need to actually get an old product from anywhere
             **/
            moRulesEngine.GetCurrentProductDelegate = GetEmptyProduct;

            WonkaProduct NewProduct = GetWonkaProductViaReflection(poCommand);

            WonkaBreRuleTreeReport Report = moRulesEngine.Validate(NewProduct);

            return Report;
        }

        public override bool Validate(AccountUpdateCommand poCommand) 
        {
            base.SerializeRulesEngineToBlockchain();

            base.SerializeRecordToBlockchain(poCommand);

            bool bValid = base.Validate(poCommand);

            // NOTE: Should anything else be done here to override the base method, like capturing events
            //       broadcast from the blockchain?

            return bValid;
        }
    }
}
