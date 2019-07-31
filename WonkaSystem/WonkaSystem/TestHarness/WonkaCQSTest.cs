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
    /// <summary>
    /// 
    /// This test will create an instance of the .NET implementation of the rules engine and initialize a 
    /// RuleTree with the rules mentioned in the file 'SimpleAccountCheck.xml'.  It will then populate a 
    /// record with test data and then apply the RuleTree against the record, for the purpose of validating
    /// the record's contents.  Unlike WonkaSimpleTest, this test will pull the existing record from a contract
    /// on the Ethereum blockchain.
    ///
    /// The rules of the XML markup can reference values from different records, like "O.Price" for the existing
    /// record's price and "N.Price" for the new record's price.
    ///
    /// NOTE: Like some other tests, the Rules Engine must be deployed by a Solidity script before running this test.
    ///
    /// NOTE: This test does execute the Ethereum implementation of the rules engine.  However, it will use classes
    ///       from the CQS namespace, which encapsulates all of the Wonka functionality (serialization to the blockchain,
    ///       execution of the engine, etc.).
    ///
    /// </summary>
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

            // Read the ABI of the Ethereum contract for the Wonka rules engine and holds our data record
            using (var AbiReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.WonkaEngine.abi")))
            {
                msAbiWonka = AbiReader.ReadToEnd();
            }

            // Read the XML markup that lists the business rules (i.e., the RuleTree)
            using (var RulesReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.SimpleAccountCheck.xml")))
            {
                msRulesContents = RulesReader.ReadToEnd();
            }

            // Using the metadata source, we create an instance of a defined data domain
            WonkaRefEnvironment RefEnv =
                WonkaRefEnvironment.CreateInstance(false, moMetadataSource);

            msSenderAddress = psSenderAddress;
            msPassword      = psPassword;

            if (psContractAddress == null)
                msContractAddress = DeployContract();
            else
                msContractAddress = psContractAddress;

            // Finally we serialize the data domain to the blockchain
            RefEnv.Serialize(msSenderAddress, msPassword, msSenderAddress, msContractAddress, msAbiWonka);
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

            // Now we assemble a predefined data record that will be the target of examination by the rules engine 
            // and to which we will apply the RuleTree
            CQS.Contracts.AccountUpdateCommand UpdateCommand = new CQS.Contracts.AccountUpdateCommand();
            UpdateCommand.AccountId   = "1234567890";
            UpdateCommand.AccountName = "JohnSmithFirstCheckingAccount";
            UpdateCommand.Status      = "OOS";
            UpdateCommand.UpdateValue = 100.00;
            UpdateCommand.Currency    = "USD";
            // UpdateCommand.AccountType = "Checking";
            UpdateCommand.AccountType = "CompletelyBogusTypeThatWillCauseAnError";

            // The engine's proxy for the blockchain is instantiated here, which will be responsible for serializing
            // and executing the RuleTree within the engine
            CQS.Validation.AccountUpdateValidator UpdateValidator =
                   new CQS.Validation.AccountUpdateValidator(UpdateCommand, new StringBuilder(msRulesContents));

            UpdateValidator.BlockchainEngineOwner = msSenderAddress;

            UpdateValidator.BlockchainEngine =
                new WonkaEth.Validation.WonkaBlockchainEngine()
                {
                    SenderAddress = msSenderAddress,
                    Password = msPassword,
                    ContractAddress = msContractAddress,
                    ContractABI = msAbiWonka
                };

            // Now execute the rules engine on the blockchain
            bool bValid = UpdateValidator.Validate(UpdateCommand);

            if (!bValid)
                throw new Exception("Oh heavens to Betsy! Something bad happened!");
        }
    }
}
