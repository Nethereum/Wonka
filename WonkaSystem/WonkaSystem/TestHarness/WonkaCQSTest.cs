using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nethereum.Web3.Accounts;

using WonkaBre;
using WonkaEth.Extensions;
using WonkaPrd;
using WonkaRef;

using WonkaSystem.CQS;
using WonkaSystem.TestHarness;

namespace WonkaSystem.TestHarness
{
    public class WonkaCQSTest
    {
        private readonly string msRulesContents;
        private readonly string msAbiWonka;

        private IMetadataRetrievable moMetadataSource = new WonkaMetadataTestSource();

        private string msSenderAddress   = "";
        private string msPassword        = "";
        private string msContractAddress = "";

        public WonkaCQSTest(string psSenderAddress, string psPassword, string psContractAddress = null)
        {
            var TmpAssembly = Assembly.GetExecutingAssembly();

            using (var AbiReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.WonkaEngine.abi")))
            {
                msAbiWonka = AbiReader.ReadToEnd();
            }

            using (var RulesReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.SimpleAccountCheck.xml")))
            {
                msRulesContents = RulesReader.ReadToEnd();
            }

            WonkaRefEnvironment RefEnv =
                WonkaRefEnvironment.CreateInstance(false, moMetadataSource);

            msSenderAddress = psSenderAddress;
            msPassword      = psPassword;

            moMetadataSource = new WonkaMetadataTestSource();

            if (psContractAddress == null)
                msContractAddress = DeployContract();
            else
                msContractAddress = psContractAddress;

            RefEnv.Serialize(msSenderAddress, msPassword, msContractAddress, msAbiWonka);
        }

        public string DeployContract()
        {
            string sSenderAddress   = msSenderAddress;
            string sContractAddress = "blah";

            var account = new Account(msPassword);
            var web3    = new Nethereum.Web3.Web3(account);

            System.Numerics.BigInteger totalSupply = System.Numerics.BigInteger.Parse("10000000");

            /**
             ** NOTE: Deployment issues have not yet been resolved - more work needs to be done
             **
             // System.Exception: Too many arguments: 1 > 0
             // at Nethereum.ABI.FunctionEncoding.ParametersEncoder.EncodeParameters (Nethereum.ABI.Model.Parameter[] parameters, System.Object[] values) [0x00078] in <b4e1e3b6a7e947da9576619c2d31bafc>:0 
             // var receipt = 
             // web3.Eth.DeployContract.SendRequestAndWaitForReceiptAsync(msAbiWonka, msByteCodeWonka, sSenderAddress, new Nethereum.Hex.HexTypes.HexBigInteger(900000), null, totalSupply).Result;
             // sContractAddress = receipt.ContractAddress;

             // var unlockReceipt = web3.Personal.UnlockAccount.SendRequestAsync(sSenderAddress, msPassword, 120).Result;

             // base fee exceeds gas limit?
             // https://gitter.im/Nethereum/Nethereum?at=5a15318e540c78242d34505f
             // sContractAddress = web3.Eth.DeployContract.SendRequestAsync(msAbiWonka, msByteCodeWonka, sSenderAddress, new Nethereum.Hex.HexTypes.HexBigInteger(totalSupply)).Result;
             **        
             **/

            return sContractAddress;
        }

        public void Execute()
        {
            WonkaRefEnvironment RefEnv = WonkaRefEnvironment.GetInstance();

            CQS.Contracts.AccountUpdateCommand UpdateCommand = new CQS.Contracts.AccountUpdateCommand();
            UpdateCommand.AccountId   = "1234567890";
            UpdateCommand.AccountName = "JohnSmithFirstCheckingAccount";
            UpdateCommand.Status      = "OOS";
            UpdateCommand.UpdateValue = 100.00;
            UpdateCommand.Currency    = "USD";
            // UpdateCommand.AccountType = "Checking";
            UpdateCommand.AccountType = "CompletelyBogusTypeThatWillCauseAnError";

            CQS.Validation.AccountUpdateValidator UpdateValidator =
                   new CQS.Validation.AccountUpdateValidator(UpdateCommand, new StringBuilder(msRulesContents));

            UpdateValidator.BlockchainEngine.SenderAddress   = msSenderAddress;
            UpdateValidator.BlockchainEngine.Password        = msPassword;
            UpdateValidator.BlockchainEngine.ContractAddress = msContractAddress;
            UpdateValidator.BlockchainEngine.ContractABI     = msAbiWonka;

            bool bValid = UpdateValidator.Validate(UpdateCommand);

            if (!bValid)
                throw new Exception("Oh heavens to Betsy! Something bad happened!");
        }
    }
}
