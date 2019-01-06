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
    /// <summary>
    /// 
    /// This test will create an instance of the .NET implementation of the rules engine and initialize a 
    /// RuleTree with the rules mentioned in the file 'VATCalculationExample.xml'.  It will then populate a 
    /// record with test data and then apply the RuleTree against the record, for the purpose of validating
    /// a logical record's contents and then calculating the VAT of a commercial sale.  It will then populate
    /// a record with test data, serialize the data to the Ethereum blockchain, and then also serialize the 
    /// RuleTree to the blockchain.  It will also execute the rules engine on the Ethereum blockchain and 
    /// examine the report that is returned.
    ///
    /// This test will showcase the Orchestration and Custom Operator (i.e., CU) functionality.
    /// In this way, during the application of a RuleTree, the rules engine (i.e., WonkaEngine contract) on the blockchain
    /// can interact with other contracts in order to do more complex operations.  In the rules markup of the test below,
    /// the CU used is "INVOKE_VAT_LOOKUP", which is a contract method that accepts multiple arguments and then returns a String.
    ///
    /// Finally, this test will showcase the Registry and Grove functionality, which is reusability of the RuleTrees.    
    /// With the Registry, a user can discover a RuleTree that belongs to someone else on the blockchain and use it 
    /// for themselves.  With the Grove functionality, we can order RuleTrees into a collection and have them applied
    /// to data in a specified order.
    ///
    /// NOTE: Like the other tests, the Rules Engine and the Registry must be deployed by Solidity scripts before
    ///       running this test.
    ///
    /// NOTE: This test does execute the Ethereum implementation of the rules engine.  However, it will use classes
    ///       from the CQS namespace, which encapsulates all of the Wonka functionality (serialization to the blockchain,
    ///       execution of the engine, etc.).    
    ///
    /// </summary>
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

        private Dictionary<string, WonkaBreSource> moAttrSourceMap = null;
        private Dictionary<string, WonkaBreSource> moCustomOpMap   = null;

        private WonkaEth.Orchestration.Init.OrchestrationInitData moOrchInitData      = null; 
        private WonkaEth.Init.WonkaEthRegistryInitialization      moWonkaRegistryInit = null;

        // This constructor will be called in the case that we wish to initialize the framework
        // with configuration files locally (embedded resources, local filesystem, etc.)
        public WonkaCQSOrchTest()
        {
            moAttrSourceMap = new Dictionary<string, WonkaBreSource>();
            moCustomOpMap   = new Dictionary<string, WonkaBreSource>();

            var TmpAssembly = Assembly.GetExecutingAssembly();

            // Using the metadata source, we create an instance of a defined data domain
            WonkaRefEnvironment RefEnv =
                WonkaRefEnvironment.CreateInstance(false, moMetadataSource);

            // Read the XML markup that lists the business rules (i.e., the RuleTree)
            using (var RulesReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.VATCalculationExample.xml")))
            {
                msRulesContents = RulesReader.ReadToEnd();
            }

            // Read the configuration file that contains all the initialization details regarding the rules engine 
            // (like addresses of contracts, senders, passwords, etc.)
            using (var XmlReader = new System.IO.StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.VATCalculationExample.init.xml")))
            {
                string sInitXml = XmlReader.ReadToEnd();

                // We deserialize/parse the contents of the config file
                System.Xml.Serialization.XmlSerializer WonkaEthSerializer =
                    new System.Xml.Serialization.XmlSerializer(typeof(WonkaEth.Init.WonkaEthInitialization),
                                                               new System.Xml.Serialization.XmlRootAttribute("WonkaEthInitialization"));

                WonkaEth.Init.WonkaEthInitialization WonkaInit =
                    WonkaEthSerializer.Deserialize(new System.IO.StringReader(sInitXml)) as WonkaEth.Init.WonkaEthInitialization;

                // Here, any embeddeded resources mentioned in the config file (instead of simple file URLs) are accessed here
                WonkaInit.RetrieveEmbeddedResources(TmpAssembly);

                // The initialization data is transformed into a structure used by the WonkaEth namespace
                moOrchInitData = WonkaInit.TransformIntoOrchestrationInit(moMetadataSource);

                System.Console.WriteLine("Number of custom operators: (" + WonkaInit.CustomOperatorList.Length + ").");
            }

            // Read the configuration file that contains all the initialization details regarding the rules registry
            // (like Ruletree info, Grove info, etc.) - this information will allow us to add our RuleTree to the 
            // Registry so that it can be discovered by users and so it can be added to a Grove (where it can be executed
            // as a member of a collection)
            using (var XmlReader = new System.IO.StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.WonkaRegistry.init.xml")))
            {
                string sInitRegistryXml = XmlReader.ReadToEnd();

                // We deserialize/parse the contents of the config file
                System.Xml.Serialization.XmlSerializer WonkaEthSerializer =
                    new System.Xml.Serialization.XmlSerializer(typeof(WonkaEth.Init.WonkaEthRegistryInitialization),
                                                               new System.Xml.Serialization.XmlRootAttribute("WonkaEthRegistryInitialization"));

                moWonkaRegistryInit =
                    WonkaEthSerializer.Deserialize(new System.IO.StringReader(sInitRegistryXml)) as WonkaEth.Init.WonkaEthRegistryInitialization;

                // Here, any embeddeded resources mentioned in the config file (instead of simple file URLs) are accessed here                
                moWonkaRegistryInit.RetrieveEmbeddedResources(TmpAssembly);
            }

            // Here, we save all data from the config files to member properties
            // This region and the usage of member properties isn't necessary, but it's useful for debugging
            #region Set Class Member Variables
            msSenderAddress = moOrchInitData.BlockchainEngine.SenderAddress;
            msPassword      = moOrchInitData.BlockchainEngine.Password;

            if (moOrchInitData.BlockchainEngine.ContractAddress == null)
                msWonkaContractAddress = DeployWonkaContract();
            else
                msWonkaContractAddress = moOrchInitData.BlockchainEngine.ContractAddress;

            if (moOrchInitData.DefaultBlockchainDataSource.ContractAddress == null)
                msOrchContractAddress = DeployOrchestrationContract();
            else
                msOrchContractAddress = moOrchInitData.DefaultBlockchainDataSource.ContractAddress;

            msAbiWonka        = moOrchInitData.BlockchainEngine.ContractABI;
            msAbiOrchContract = moOrchInitData.DefaultBlockchainDataSource.ContractABI;

            moDefaultSource =
                new WonkaBreSource(moOrchInitData.DefaultBlockchainDataSource.SourceId,
                                   moOrchInitData.DefaultBlockchainDataSource.SenderAddress,
                                   moOrchInitData.DefaultBlockchainDataSource.Password,
                                   moOrchInitData.DefaultBlockchainDataSource.ContractAddress,
                                   moOrchInitData.DefaultBlockchainDataSource.ContractABI,
                                   moOrchInitData.DefaultBlockchainDataSource.MethodName,
                                   moOrchInitData.DefaultBlockchainDataSource.SetterMethodName,
                                   RetrieveValueMethod);

            foreach (WonkaRefAttr TempAttr in RefEnv.AttrCache)
            {
                moAttrSourceMap[TempAttr.AttrName] = moDefaultSource;
            }

            moCustomOpMap = moOrchInitData.BlockchainCustomOpFunctions;
            #endregion

            // We initialize the proxy that will be used to communicate with the Registry on the blockchain
            WonkaEth.Contracts.WonkaRuleTreeRegistry WonkaRegistry =
                WonkaEth.Contracts.WonkaRuleTreeRegistry.CreateInstance(moWonkaRegistryInit.BlockchainRegistry.ContractSender, 
                                                                        moWonkaRegistryInit.BlockchainRegistry.ContractPassword,
                                                                        moWonkaRegistryInit.BlockchainRegistry.ContractAddress, 
                                                                        moWonkaRegistryInit.BlockchainRegistry.ContractABI);

            // Here, the data domain is serialized to the blockchain for use by the RuleTree(s)
            RefEnv.Serialize(msSenderAddress, msPassword, msWonkaContractAddress, msAbiWonka);
        }

        // This constructor will be called in the case that we wish to initialize the framework
        // with provided parameters
        public WonkaCQSOrchTest(string psSenderAddress, string psPassword, string psWonkaContractAddress, string psOrchContractAddress)
        {
            var TmpAssembly = Assembly.GetExecutingAssembly();

            moAttrSourceMap = new Dictionary<string, WonkaBreSource>();
            moCustomOpMap   = new Dictionary<string, WonkaBreSource>();

            // Read the ABI of the Ethereum contract for the Wonka rules engine
            using (var AbiReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.WonkaEngine.abi")))
            {
                msAbiWonka = AbiReader.ReadToEnd();
            }

            // Read the ABI of the Ethereum contract that will demonstrate both our Custom Operator functionality and our Orchestration functionality
            using (var AbiReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.OrchTest.abi")))
            {
                msAbiOrchContract = AbiReader.ReadToEnd();
            }

            // Read the XML markup that lists the business rules
            using (var RulesReader = new StreamReader(TmpAssembly.GetManifestResourceStream("WonkaSystem.TestData.VATCalculationExample.xml")))
            {
                msRulesContents = RulesReader.ReadToEnd();
            }

            // Using the metadata source, we create an instance of a defined data domain
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

            // Serialize the data domain to the blockchain
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

            // Here a mapping is created, where each Attribute points to a specific contract and its "accessor" methods
            // - the class that contains this information (contract, accessors, etc.) is of the WonkaBreSource type            
            foreach (WonkaRefAttr TempAttr in RefEnv.AttrCache)
            {
                moAttrSourceMap[TempAttr.AttrName] = moDefaultSource;
            }

            Dictionary<string, WonkaBreSource> CustomOpSourceMap = new Dictionary<string, WonkaBreSource>();

            // Here a mapping is created, where each Custom Operator points to a specific contract and its "implementation" method
            // - the class that contains this information (contract, accessors, etc.) is of the WonkaBreSource type            
            WonkaBreSource CustomOpSource =
                new WonkaBreSource(CONST_CUSTOM_OP_MARKUP_ID,
                                   msSenderAddress,
                                   msPassword,
                                   psOrchContractAddress,
                                   msAbiOrchContract,
                                   LookupVATDenominator,
                                   CONST_CUSTOM_OP_CONTRACT_METHOD);

            moCustomOpMap[CONST_CUSTOM_OP_MARKUP_ID] = CustomOpSource;
        }

        // This constructor will be called in the case that we wish to initialize the framework
        // with configuration files that will be accessed through IPFS
        public WonkaCQSOrchTest(StringBuilder psPeerKeyId, string psRulesMarkupFile, string psRulesInitFile, string psRegistryInitFile)
        {
            moAttrSourceMap = new Dictionary<string, WonkaBreSource>();
            moCustomOpMap   = new Dictionary<string, WonkaBreSource>();

            var TmpAssembly = Assembly.GetExecutingAssembly();

            WonkaRefEnvironment            RefEnv  = WonkaRefEnvironment.CreateInstance(false, moMetadataSource);
            WonkaIpfs.WonkaIpfsEnvironment IpfsEnv = WonkaIpfs.WonkaIpfsEnvironment.CreateInstance();

            // Read the XML markup that lists the business rules
            msRulesContents = IpfsEnv.GetFile(psPeerKeyId.ToString(), psRulesMarkupFile);

            // Read the configuration file that contains all the initialization details regarding the rules engine 
            // (like addresses of contracts, senders, passwords, etc.)
            string sInitXml = IpfsEnv.GetFile(psPeerKeyId.ToString(), psRulesInitFile);
            if (!String.IsNullOrEmpty(sInitXml))
            {
                System.Xml.Serialization.XmlSerializer WonkaEthSerializer =
                    new System.Xml.Serialization.XmlSerializer(typeof(WonkaEth.Init.WonkaEthInitialization),
                                                               new System.Xml.Serialization.XmlRootAttribute("WonkaEthInitialization"));

                WonkaEth.Init.WonkaEthInitialization WonkaInit =
                    WonkaEthSerializer.Deserialize(new System.IO.StringReader(sInitXml)) as WonkaEth.Init.WonkaEthInitialization;

                WonkaInit.RetrieveEmbeddedResources(TmpAssembly);

                moOrchInitData = WonkaInit.TransformIntoOrchestrationInit(moMetadataSource);

                System.Console.WriteLine("Number of custom operators: (" + WonkaInit.CustomOperatorList.Length + ").");
            }

            // Read the configuration file that contains all the initialization details regarding the rules registry
            // (like Ruletree info, Grove info, etc.)            
            string sInitRegistryXml = IpfsEnv.GetFile(psPeerKeyId.ToString(), psRegistryInitFile);
            if (!String.IsNullOrEmpty(sInitRegistryXml))
            {
                System.Xml.Serialization.XmlSerializer WonkaEthSerializer =
                    new System.Xml.Serialization.XmlSerializer(typeof(WonkaEth.Init.WonkaEthRegistryInitialization),
                                                               new System.Xml.Serialization.XmlRootAttribute("WonkaEthRegistryInitialization"));

                moWonkaRegistryInit =
                    WonkaEthSerializer.Deserialize(new System.IO.StringReader(sInitRegistryXml)) as WonkaEth.Init.WonkaEthRegistryInitialization;

                moWonkaRegistryInit.RetrieveEmbeddedResources(TmpAssembly);
            }

            #region Set Class Member Variables
            msSenderAddress = moOrchInitData.BlockchainEngine.SenderAddress;
            msPassword      = moOrchInitData.BlockchainEngine.Password;

            if (moOrchInitData.BlockchainEngine.ContractAddress == null)
                msWonkaContractAddress = DeployWonkaContract();
            else
                msWonkaContractAddress = moOrchInitData.BlockchainEngine.ContractAddress;

            if (moOrchInitData.DefaultBlockchainDataSource.ContractAddress == null)
                msOrchContractAddress = DeployOrchestrationContract();
            else
                msOrchContractAddress = moOrchInitData.DefaultBlockchainDataSource.ContractAddress;

            msAbiWonka        = moOrchInitData.BlockchainEngine.ContractABI;
            msAbiOrchContract = moOrchInitData.DefaultBlockchainDataSource.ContractABI;

            moDefaultSource =
                new WonkaBreSource(moOrchInitData.DefaultBlockchainDataSource.SourceId,
                                   moOrchInitData.DefaultBlockchainDataSource.SenderAddress,
                                   moOrchInitData.DefaultBlockchainDataSource.Password,
                                   moOrchInitData.DefaultBlockchainDataSource.ContractAddress,
                                   moOrchInitData.DefaultBlockchainDataSource.ContractABI,
                                   moOrchInitData.DefaultBlockchainDataSource.MethodName,
                                   moOrchInitData.DefaultBlockchainDataSource.SetterMethodName,
                                   RetrieveValueMethod);

            // Here a mapping is created, where each Attribute points to a specific contract and its "accessor" methods
            // - the class that contains this information (contract, accessors, etc.) is of the WonkaBreSource type
            foreach (WonkaRefAttr TempAttr in RefEnv.AttrCache)
            {
                moAttrSourceMap[TempAttr.AttrName] = moDefaultSource;
            }

            // Here a mapping is created, where each Custom Operator points to a specific contract and its "implementation" method
            // - the class that contains this information (contract, accessors, etc.) is of the WonkaBreSource type    
            moCustomOpMap = moOrchInitData.BlockchainCustomOpFunctions;
            
            #endregion

            WonkaEth.Contracts.WonkaRuleTreeRegistry WonkaRegistry =
                WonkaEth.Contracts.WonkaRuleTreeRegistry.CreateInstance(moWonkaRegistryInit.BlockchainRegistry.ContractSender, 
                                                                        moWonkaRegistryInit.BlockchainRegistry.ContractPassword,
                                                                        moWonkaRegistryInit.BlockchainRegistry.ContractAddress, 
                                                                        moWonkaRegistryInit.BlockchainRegistry.ContractABI);


            RefEnv.Serialize(msSenderAddress, msPassword, msWonkaContractAddress, msAbiWonka);
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

        public void Execute()
        {
            WonkaRefEnvironment RefEnv = WonkaRefEnvironment.GetInstance();

            // Now we assemble the data record for processing - in our VAT Calculation example, parts of the 
            // logical record can exist within contract(s) within the blockchain (which has been specified 
            // via Orchestration metadata), like a logistics or supply contract - these properties below 
            // (NewSaleEAN, NewSaleItemType, CountryOfSale) would be supplied by a client, like an 
            // eCommerce site, and be persisted to the blockchain so we may apply the RuleTree to the logical record
            CQS.Contracts.SalesTrxCreateCommand SalesTrxCommand = new CQS.Contracts.SalesTrxCreateCommand();

            SalesTrxCommand.NewSaleEAN      = 9781234567890;
            SalesTrxCommand.NewSaleItemType = "Widget";
            SalesTrxCommand.CountryOfSale   = "UK";

            WonkaEth.Orchestration.Init.OrchestrationInitData InitData = GenerateInitData();

            #region Invoking the RuleTree for the first time as a single entity

            // The engine's proxy for the blockchain is instantiated here, which will be responsible for serializing
            // and executing the RuleTree within the engine
            CQS.Generation.SalesTransactionGenerator TrxGenerator =
                   new CQS.Generation.SalesTransactionGenerator(SalesTrxCommand, new StringBuilder(msRulesContents), InitData);

            // Here, we invoke the Rules engine on the blockchain, which will calculate the VAT for a sale and then
            // retrieve all values of this logical record (including the VAT) and assemble them within 'SalesTrxCommand'
            bool bValid = TrxGenerator.GenerateSalesTransaction(SalesTrxCommand);

            if (!bValid)
                throw new Exception("Oh heavens to Betsy! Something bad happened!");

            // Since the purpose of this example was to showcase the calculated VAT, we examine them here 
            // (while interactively debugging in Visual Studio)
            string sNewSellTaxAmt    = Convert.ToString(SalesTrxCommand.NewSellTaxAmt);
            string sNewVATAmtForHMRC = Convert.ToString(SalesTrxCommand.NewVATAmtForHMRC);

            #endregion

            #region Invoking the RuleTree as a registered entity and as a member of a Grove

            // Here, we attempt to call the same RuleTree as above, but we are going to invoke the execution of
            // its Grove "NewSaleGroup" - since it is the sole member of the Grove, it will still be the only RuleTree
            // applied to the record - in this scenario, we pretend that we know nothing about the RuleTree or the Grove, 
            // effectively treating it as a black box and only looking to retrieve the VAT
            WonkaEth.Contracts.WonkaRuleGrove NewSaleGrove = new WonkaEth.Contracts.WonkaRuleGrove("NewSaleGroup");
            NewSaleGrove.PopulateFromRegistry(this.msAbiWonka);

            // The engine's lightweight proxy for the blockchain is instantiated here
            WonkaEth.Orchestration.WonkaOrchestratorProxy<CQS.Contracts.SalesTrxCreateCommand> TrxGeneratorProxy = 
                new WonkaEth.Orchestration.WonkaOrchestratorProxy<CQS.Contracts.SalesTrxCreateCommand>(SalesTrxCommand, InitData);

            // We reset the values here and in the blockchain (by serializing)
            SalesTrxCommand.NewSellTaxAmt    = 0;
            SalesTrxCommand.NewVATAmtForHMRC = 0;

            TrxGeneratorProxy.SerializeRecordToBlockchain(SalesTrxCommand);

            // NOTE: Only useful when debugging
            // TrxGeneratorProxy.DeserializeRecordFromBlockchain(SalesTrxCommand);

            Dictionary<string, WonkaEth.Contracts.IOrchestrate> GroveMembers = new Dictionary<string, WonkaEth.Contracts.IOrchestrate>();
            GroveMembers[NewSaleGrove.OrderedRuleTrees[0].RuleTreeId] = TrxGenerator;

            // With their provided proxies for each RuleTree, we can now execute the Grove (or, in this case, our sole RuleTree)
            NewSaleGrove.Orchestrate(SalesTrxCommand, GroveMembers);

            // Again, since the purpose of this example was to showcase the calculated VAT, we examine them here 
            // (while interactively debugging in Visual Studio)            
            sNewSellTaxAmt    = Convert.ToString(SalesTrxCommand.NewSellTaxAmt);
            sNewVATAmtForHMRC = Convert.ToString(SalesTrxCommand.NewVATAmtForHMRC);

            #endregion

            // Now test exporting the RuleTree from the blockchain
            var RegistryItem = NewSaleGrove.OrderedRuleTrees[0];
            var ExportedXml  = RegistryItem.ExportXmlString();

            System.Console.WriteLine("DEBUG: The payload is: \n(\n" + ExportedXml + "\n)\n");
        }

        #region Methods Only Used for .NET Execution of the Rules Engine

        public WonkaEth.Orchestration.Init.OrchestrationInitData GenerateInitData()
        {
            WonkaEth.Orchestration.Init.OrchestrationInitData InitData = null;

            if (moOrchInitData != null)
                InitData = moOrchInitData;
            else
            {
                InitData = new WonkaEth.Orchestration.Init.OrchestrationInitData();

                InitData.BlockchainEngine = new WonkaBreSource("N", msSenderAddress, msPassword, msWonkaContractAddress, msAbiWonka, null, null, null);

                InitData.AttributesMetadataSource = new WonkaMetadataVATSource();

                InitData.DefaultBlockchainDataSource = moDefaultSource;
                InitData.BlockchainDataSources       = moAttrSourceMap;
                InitData.BlockchainCustomOpFunctions = moCustomOpMap;
            }

            return InitData;
        }

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
