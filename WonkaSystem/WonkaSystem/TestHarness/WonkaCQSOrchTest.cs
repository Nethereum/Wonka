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
using WonkaBre.RuleTree;
using WonkaPrd;
using WonkaRef;

using WonkaSystem.CQS;
using WonkaSystem.TestHarness;

namespace WonkaSystem.TestHarness
{
    public class WonkaCQSOrchTest
    {
        #region Constants

        private const string CONST_ORCH_CONTRACT_MARKUP_ID      = "N";
        private const string CONST_CUSTOM_OP_CONTRACT_MARKUP_ID = "N";
        private const string CONST_ORCH_CONTRACT_GET_METHOD     = "getAttrValueBytes32";
        private const string CONST_ORCH_CONTRACT_SET_METHOD     = "setAttrValueBytes32";
        private const string CONST_CUSTOM_OP_MARKUP_ID          = "INVOKE_VAT_LOOKUP";
        private const string CONST_CUSTOM_OP_CONTRACT_METHOD    = "lookupVATDenominator";

        #endregion

        private readonly string         msRulesContents;
        private readonly string         msAbiWonka;
        private readonly string         msAbiOrchContract;
        private readonly WonkaBreSource moDefaultSource;

        private IMetadataRetrievable moMetadataSource = new WonkaMetadataVATSource();

        private readonly string msSenderAddress        = "";
        private readonly string msPassword             = "";
        private readonly string msWonkaContractAddress = "";
        private readonly string msOrchContractAddress  = "";

        private Dictionary<string, WonkaBreSource> moSourceMap = null;

        public WonkaCQSOrchTest(string psSenderAddress, string psPassword, string psWonkaContractAddress, string psOrchContractAddress)
        {
            var TmpAssembly = Assembly.GetExecutingAssembly();

            using (var AbiReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.WonkaEngine.abi")))
            {
                msAbiWonka = AbiReader.ReadToEnd();
            }

            using (var AbiReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.OrchTest.abi")))
            {
                msAbiOrchContract = AbiReader.ReadToEnd();
            }

            using (var RulesReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.VATExample.xml")))
            {
                msRulesContents = RulesReader.ReadToEnd();
            }

            WonkaRefEnvironment RefEnv =
                WonkaRefEnvironment.CreateInstance(false, moMetadataSource);

            msSenderAddress = psSenderAddress;
            msPassword      = psPassword;

            moMetadataSource = new WonkaMetadataTestSource();

            if (psWonkaContractAddress == null)
                msWonkaContractAddress = DeployWonkaContract();
            else
                msWonkaContractAddress = psWonkaContractAddress;

            if (psOrchContractAddress == null)
                msOrchContractAddress = DeployOrchestrationContract();
            else
                msOrchContractAddress = psOrchContractAddress;

            RefEnv.Serialize(msSenderAddress, msPassword, msWonkaContractAddress, msAbiWonka);

            moDefaultSource =
                new WonkaBreSource(CONST_ORCH_CONTRACT_MARKUP_ID, 
                                   msSenderAddress, 
                                   msPassword, 
                                   psOrchContractAddress, 
                                   msAbiOrchContract, 
                                   CONST_ORCH_CONTRACT_GET_METHOD, 
                                   CONST_ORCH_CONTRACT_SET_METHOD, 
                                   RetrieveValueMethod);

            foreach (WonkaRefAttr TempAttr in RefEnv.AttrCache)
            {
                moSourceMap[TempAttr.AttrName] = moDefaultSource;
            }

            Dictionary<string, WonkaBreSource> CustomOpSourceMap = new Dictionary<string, WonkaBreSource>();

            WonkaBreSource CustomOpSource =
                new WonkaBreSource(CONST_CUSTOM_OP_MARKUP_ID, 
                                   msSenderAddress, 
                                   msPassword, 
                                   psOrchContractAddress, 
                                   msAbiOrchContract, 
                                   LookupVATDenominator, 
                                   CONST_CUSTOM_OP_CONTRACT_METHOD);

            CustomOpSourceMap[CONST_CUSTOM_OP_MARKUP_ID] = CustomOpSource;
        }

        public string DeployOrchestrationContract()
        {
            string sSenderAddress   = msSenderAddress;
            string sContractAddress = "blah";

            var account = new Account(msPassword);
            var web3    = new Nethereum.Web3.Web3(account);

            System.Numerics.BigInteger totalSupply = System.Numerics.BigInteger.Parse("10000000");

            /**
             ** NOTE: Deployment issues have not yet been resolved - more work needs to be done
             **
             // var unlockReceipt = web3.Personal.UnlockAccount.SendRequestAsync(sSenderAddress, msPassword, 120).Result;

             // base fee exceeds gas limit?
             // https://gitter.im/Nethereum/Nethereum?at=5a15318e540c78242d34505f
             // sContractAddress = web3.Eth.DeployContract.SendRequestAsync(msAbiWonka, msByteCodeWonka, sSenderAddress, new Nethereum.Hex.HexTypes.HexBigInteger(totalSupply)).Result;
             **        
             **/

            return sContractAddress;
        }

        public string DeployWonkaContract()
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

        // NOTE: This function will need to be altered
        public void Execute()
        {
            WonkaRefEnvironment RefEnv = WonkaRefEnvironment.GetInstance();

            CQS.Contracts.AccountUpdateCommand UpdateCommand = new CQS.Contracts.AccountUpdateCommand();

            /*
            UpdateCommand.AccountId   = "1234567890";
            UpdateCommand.AccountName = "JohnSmithFirstCheckingAccount";
            UpdateCommand.Status      = "OOS";
            UpdateCommand.UpdateValue = 100.00;
            UpdateCommand.Currency    = "USD";
            // UpdateCommand.AccountType = "Checking";
            UpdateCommand.AccountType = "CompletelyBogusTypeThatWillCauseAnError";
            */

            CQS.Validation.AccountUpdateValidator UpdateValidator =
                   new CQS.Validation.AccountUpdateValidator(UpdateCommand, new StringBuilder(msRulesContents));

            UpdateValidator.BlockchainEngine.SenderAddress   = msSenderAddress;
            UpdateValidator.BlockchainEngine.Password        = msPassword;
            UpdateValidator.BlockchainEngine.ContractAddress = msOrchContractAddress;
            UpdateValidator.BlockchainEngine.ContractABI     = msAbiWonka;

            bool bValid = UpdateValidator.Validate(UpdateCommand);

            if (!bValid)
                throw new Exception("Oh heavens to Betsy! Something bad happened!");
        }

        #region Methods Only Used for .NET Execution of the Rules Engine

        public Nethereum.Contracts.Contract GetContract(WonkaBre.RuleTree.WonkaBreSource TargetSource)
        {
            var account  = new Account(TargetSource.Password);
            var web3     = new Nethereum.Web3.Web3(account);
            var contract = web3.Eth.GetContract(TargetSource.ContractABI, TargetSource.ContractAddress);

            return contract;
        }

        public string LookupVATDenominator(string psSaleItemType, string psCountryOfSale, string psDummyVal1, string psDummyVal2)
        {
            if (psSaleItemType == "Widget" && psCountryOfSale == "UK")
                return "5";
            else
                return "1";
        }

        public string RetrieveValueMethod(WonkaBre.RuleTree.WonkaBreSource poTargetSource, string psAttrName)
        {
            var contract = GetContract(poTargetSource);

            var getRecordValueFunction = contract.GetFunction(poTargetSource.MethodName);

            var result = getRecordValueFunction.CallAsync<string>(psAttrName).Result;

            return result;
        }

        #endregion
    }
}
