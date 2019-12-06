using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Wonka.Product;
using Wonka.MetaData;

using Wonka.BizRulesEngine.Permissions;
using Wonka.BizRulesEngine.Readers;
using Wonka.BizRulesEngine.Reporting;
using Wonka.BizRulesEngine.RuleTree;
using Wonka.BizRulesEngine.RuleTree.RuleTypes;
using Wonka.BizRulesEngine.Triggers;

namespace Wonka.BizRulesEngine
{
    /// <summary>
    /// 
    /// This class is the main console that should be used by users of this library.  All functionality
    /// should be utilized through an instance of this class.
    /// 
    /// This console (and the rest of the classes in the library) encapsulate a business rules engine, 
    /// that will know how to read and then invoke a set or rules (i.e., a RuleTree).  For the most part,
    /// a RuleTree provides the ability to validate a set of data, but it can also be used to invoke 
    /// user-defined actions (call a custom function, etc.) within a certain context.
    /// 
    /// NOTE: There is only 1 RuleTree allowed per instance of this class.  This is unlike an instance of the
    /// Wonka rules engine contract on the Ethereum blockchain, which can execute multiple instances of RuleTrees 
    /// held in its storage.  Multiple instances of the contract are not created in order to save expenses 
    /// (especially in terms of saving gas).
    /// 
    /// </summary>
    public class WonkaBizRulesEngine
    {
        #region Delegates
        //public delegate bool ComplexEditsDelegate(WonkaProduct poNewProduct, WonkaProduct poOldProduct);
        public delegate WonkaProduct RetrieveOldRecordDelegate(Dictionary<string, string> KeyValues);
        public delegate string       RetrieveStdOpValDelegate(WonkaBizRulesEngine EngineId, string OptionalKeyVal);
        #endregion

        #region CONSTANTS

        public const int CONST_ISBN_LEN = 10;
        public const int CONST_EAN_LEN  = 13;

        public static readonly int[] CONST_EAN_WEIGHTS = { 1, 3, 1, 3, 1, 3, 1, 3, 1, 3, 1, 3, 1 };

        public const int CONST_MAX_STRING_LEN = 512;

        public const int CONST_MAX_PROPS = 32;
        public const int MAX_RULESET_NES = 32;

        public const int RULE_EXEC_SEVERE_FAIL = -1;
        public const int RULE_EXEC_SEVERE_FAIL_VAL = 666;

        public const int RULE_VALUE_MAX = 9;

        #endregion

        #region Constructors
        public WonkaBizRulesEngine(string psRulesFilepath, IMetadataRetrievable piMetadataSource = null, bool pbAddToRegistry = false)
        {
            if (String.IsNullOrEmpty(psRulesFilepath))
            {
                throw new WonkaBizRuleException("ERROR!  Provided rules file is null or empty!");
            }

            if (!File.Exists(psRulesFilepath))
            {
                throw new WonkaBizRuleException("ERROR!  Provided rules file(" + psRulesFilepath + ") does not exist on the filesystem.");
            }

            UsingOrchestrationMode = false;
            AddToRegistry          = pbAddToRegistry;

            RefEnvHandle = Init(piMetadataSource);

            WonkaBizRulesXmlReader BreXmlReader = new WonkaBizRulesXmlReader(psRulesFilepath, piMetadataSource, this);

            RuleTreeRoot = BreXmlReader.ParseRuleTree();
            AllRuleSets  = BreXmlReader.AllParsedRuleSets;
        }

        public WonkaBizRulesEngine(StringBuilder psRules, IMetadataRetrievable piMetadataSource = null, bool pbAddToRegistry = false)
        {
            if ((psRules == null) || (psRules.Length <= 0))
            {
                throw new WonkaBizRuleException("ERROR!  Provided rules are null or empty!");
            }

            UsingOrchestrationMode = false;
            AddToRegistry          = pbAddToRegistry;

            RefEnvHandle = Init(piMetadataSource);

            WonkaBizRulesXmlReader BreXmlReader = new WonkaBizRulesXmlReader(psRules, piMetadataSource, this);

            RuleTreeRoot = BreXmlReader.ParseRuleTree();
            AllRuleSets  = BreXmlReader.AllParsedRuleSets;
        }

        public WonkaBizRulesEngine(StringBuilder                      psRules, 
                                   Dictionary<string, WonkaBizSource> poSourceMap, 
                                   IMetadataRetrievable               piMetadataSource = null,
                                   bool                               pbAddToRegistry = false)
        {
            if ((psRules == null) || (psRules.Length <= 0))
            {
                throw new WonkaBizRuleException("ERROR!  Provided rules are null or empty!");
            }

            UsingOrchestrationMode = true;
            AddToRegistry          = pbAddToRegistry;

            RefEnvHandle = Init(piMetadataSource);

            WonkaBizRulesXmlReader BreXmlReader = new WonkaBizRulesXmlReader(psRules, piMetadataSource, this);

            RuleTreeRoot = BreXmlReader.ParseRuleTree();
            SourceMap    = poSourceMap;
            AllRuleSets  = BreXmlReader.AllParsedRuleSets;

            this.RetrieveCurrRecord = AssembleCurrentProduct;
        }

        public WonkaBizRulesEngine(StringBuilder                      psRules, 
                                   Dictionary<string, WonkaBizSource> poSourceMap, 
                                   Dictionary<string, WonkaBizSource> poCustomOpBlockchainSources,
                                   IMetadataRetrievable               piMetadataSource = null,
                                   bool                               pbAddToRegistry = true)
        {
            if ((psRules == null) || (psRules.Length <= 0))
            {
                throw new WonkaBizRuleException("ERROR!  Provided rules are null or empty!");
            }

            UsingOrchestrationMode = true;
            AddToRegistry          = pbAddToRegistry;

            RefEnvHandle = Init(piMetadataSource);

            WonkaBizRulesXmlReader BreXmlReader = new WonkaBizRulesXmlReader(psRules, piMetadataSource, this);

            foreach (string sKey in poCustomOpBlockchainSources.Keys)
            {
                WonkaBizSource oTargetSource = poCustomOpBlockchainSources[sKey];

                BreXmlReader.AddCustomOperator(sKey, oTargetSource);
            }

            RuleTreeRoot = BreXmlReader.ParseRuleTree();
            SourceMap    = poSourceMap;
            CustomOpMap  = poCustomOpBlockchainSources;
            AllRuleSets  = BreXmlReader.AllParsedRuleSets;

            this.RetrieveCurrRecord = AssembleCurrentProduct;
        }

        /// <summary>
        /// 
        /// This constructor should only be used when we call Splinter() to break up a current RuleTree
        /// and form a new Grove with the main child branches under the root.  Several assumptions are then
        /// assumed here, like an instantiation of the WonkaRefEnvironment taking place already.
        /// 
        /// 
        /// </summary>
        public WonkaBizRulesEngine(WonkaBizRuleSet poRootRuleSet, WonkaBizRulesEngine poRefEngine)
        {
            UsingOrchestrationMode = true;
            AddToRegistry          = false;

            RefEnvHandle = Init(null);

			AllRuleSets  = new List<WonkaBizRuleSet>();
            RuleTreeRoot = poRootRuleSet;
            SourceMap    = poRefEngine.SourceMap;
            CustomOpMap  = poRefEngine.CustomOpMap;

            AllRuleSets.Add(poRootRuleSet);
            foreach (WonkaBizRuleSet TmpBizRuleSet in poRootRuleSet.ChildRuleSets)
            {
                AddRuleSets(TmpBizRuleSet);
            }               

            this.RetrieveCurrRecord = AssembleCurrentProduct;
        }


        #endregion

        #region Public Methods

        /// <summary>
        /// 
        /// This method will assemble the new product by iterating through each specified source
        /// and retrieving the data from it.
        /// 
        /// <param name="poKeyValues">The keys for the product whose data we wish to extract/param>
        /// <returns>Contains the assembled product data that represents the current product</returns>
        /// </summary>
        public WonkaProduct AssembleCurrentProduct(Dictionary<string, string> poKeyValues)
        {
            WonkaProduct CurrentProduct = new WonkaProduct();

            // NOTE: Do work here
            if (SourceMap != null)
            {
                foreach (string sTmpAttName in SourceMap.Keys)
                {
                    WonkaBizSource TmpSource  = SourceMap[sTmpAttName];
                    WonkaRefAttr   TargetAttr = RefEnvHandle.GetAttributeByAttrName(sTmpAttName);

                    string sTmpValue = TmpSource.RetrievalDelegate.Invoke(TmpSource, TargetAttr.AttrName);

                    CurrentProduct.SetAttribute(TargetAttr, sTmpValue);
                }
            }

            return CurrentProduct;
        }

        /// <summary>
        /// 
        /// This method will extract the keys from the product and return them in a Dictionary.
        /// 
        /// <param name="poTargetProduct">The product whose keys we wish to extract/param>
        /// <returns>Contains the keys for the provided product</returns>
        /// </summary>
        public Dictionary<string, string> GetProductKeys(WonkaProduct poTargetProduct)
        {
            WonkaRefEnvironment WonkaRefEnv = WonkaRefEnvironment.GetInstance();

            Dictionary<string, string> ProductKeys = new Dictionary<string, string>();

            foreach (WonkaRefAttr TempAttrKey in WonkaRefEnv.AttrKeys)
            {
                // NOTE: 1 is the primary group and has the keys that identify the product as a whole
                if (TempAttrKey.GroupId == 1)
                {
                    if (poTargetProduct.GetProductGroup(TempAttrKey.GroupId).GetRowCount() <= 0)
                    {
                        throw new WonkaBizRuleException("ERROR!  Provided incoming product has empty group for needed key (" + TempAttrKey.AttrName + ").");
                    }

                    string sTempKeyValue = poTargetProduct.GetProductGroup(TempAttrKey.GroupId)[0][TempAttrKey.AttrId];

                    if (String.IsNullOrEmpty(sTempKeyValue))
                    {
                        throw new WonkaBizRuleException("ERROR!  Provided incoming product has no value for needed key(" + TempAttrKey.AttrName + ").");
                    }

                    ProductKeys[TempAttrKey.AttrName] = sTempKeyValue;
                }
            }

            return ProductKeys;
        }

        /// <summary>
        /// 
        /// This method will:
        /// 
        /// 1.) Grab the current product by retrieving it through the invocation of th delegate
        /// 2.) Validate the incoming product (and possibly the current product) using the RuleTree initialized in the constructor
        /// 
        /// <param name="poIncomingProduct">The product that we are attempting to validate</param>
        /// <returns>Contains a detailed report of the RuleTree's application to the provided product</returns>
        /// </summary>
        public WonkaBizRuleTreeReport Validate(WonkaProduct poIncomingProduct)
        {
            WonkaRefEnvironment        WonkaRefEnv = WonkaRefEnvironment.GetInstance();
            Dictionary<string, string> ProductKeys = GetProductKeys(poIncomingProduct);

            if (poIncomingProduct == null)
            {
                throw new WonkaBizRuleException("ERROR!  Provided incoming product is null!");
            }

            if ((TransactionState != null) && !TransactionState.IsTransactionConfirmed())
            {
                throw new WonkaBizPermissionsException("ERROR!  Pending transaction has not yet been confirmed!", TransactionState);
            }

            WonkaBizRuleTreeReport RuleTreeReport = new WonkaBizRuleTreeReport();

            try
            {
                if (GetCurrentProductDelegate != null)
                {
                    CurrentProductOnDB = GetCurrentProductDelegate.Invoke(ProductKeys);
                }
                else
                {
                    CurrentProductOnDB = new WonkaProduct();
                }

                WonkaBizRuleMediator.MediateRuleTreeExecution(RuleTreeRoot, poIncomingProduct, CurrentProductOnDB, RuleTreeReport);

                /*
                 * NOTE: Do we need anything like this method
                 * 
                if (PostApplicationDelegate != null)
                    PostApplicationDelegate.Invoke(poIncomingProduct, CurrentProductOnDB);
                */
            }
            finally
            {
                if (TransactionState != null)
                {
                    TransactionState.ClearPendingTransaction();
                }
            }

            RuleTreeReport.EndTime = DateTime.Now;

            return RuleTreeReport;
        }

		#endregion

		#region Private Methods

        private void AddRuleSets(WonkaBizRuleSet poTargetRuleSet)
        {
            AllRuleSets.Add(poTargetRuleSet);

            poTargetRuleSet.ChildRuleSets.ForEach(x => AddRuleSets(x));
        }

        private WonkaRefEnvironment Init(IMetadataRetrievable piMetadataSource)
        {
            WonkaRefEnvironment RefEnv = null;

            try
            {
                RefEnv = WonkaRefEnvironment.GetInstance();
            }
            catch (Exception ex)
            {
                RefEnv = WonkaRefEnvironment.CreateInstance(false, piMetadataSource);
            }

            this.CurrentProductOnDB = null;
            this.TempDirectory      = "C:\tmp";
            this.RetrieveCurrRecord = null;
            this.TransactionState   = null;

            GroveId       = RegistrationId = string.Empty;
            GroveIndex    = 0;
            StdOpMap      = new Dictionary<STD_OP_TYPE, RetrieveStdOpValDelegate>();
            SourceMap     = new Dictionary<string, WonkaBizSource>();
            CustomOpMap   = new Dictionary<string, WonkaBizSource>();
            DefaultSource = string.Empty;

            OnSuccessTriggers = new List<ISuccessTrigger>();
            OnFailureTriggers = new List<IFailureTrigger>();

            return RefEnv;
        }

		#endregion

		#region Members

		private RetrieveOldRecordDelegate RetrieveCurrRecord;

        private Dictionary<STD_OP_TYPE, RetrieveStdOpValDelegate> StandardOps;

        #endregion

        #region Properties

        private string TempDirectory { get; set; }
            
        public readonly bool AddToRegistry;

        public readonly bool UsingOrchestrationMode;

        public readonly WonkaRefEnvironment RefEnvHandle;

        public string RegistrationId { get; set; }

        public string GroveId { get; set; }

        public uint GroveIndex { get; set; }

        public WonkaBizRuleSet RuleTreeRoot { get; set; }

        public WonkaProduct CurrentProductOnDB { get; set; }

        public RetrieveOldRecordDelegate GetCurrentProductDelegate
        {
            get
            {
                return RetrieveCurrRecord;
            }

            set
            {
                if (!UsingOrchestrationMode)
                {
                    RetrieveCurrRecord = value;
                }
                else
                {
                    throw new WonkaBizRuleException("ERROR!  Cannot reassign the delegate when running in orchestration mode.");
                }
            }
        }

        public Dictionary<STD_OP_TYPE, RetrieveStdOpValDelegate> StdOpMap
        {   
            get
            {
                return new Dictionary<STD_OP_TYPE, RetrieveStdOpValDelegate>(StandardOps);
            }

            set
            {
                StandardOps = value;

                if (AllRuleSets != null)
                {
                    foreach (WonkaBizRuleSet TempRuleSet in AllRuleSets)
                    {
                        foreach (WonkaBizRule TempRule in TempRuleSet.EvaluativeRules)
                        {
                            if (StandardOps != null)
                            {
                                if ((TempRule is ArithmeticLimitRule) && StandardOps.ContainsKey(STD_OP_TYPE.STD_OP_BLOCK_NUM))
                                {
                                    ((ArithmeticLimitRule)TempRule).BlockNumDelegate = StandardOps[STD_OP_TYPE.STD_OP_BLOCK_NUM];
                                }
                            }
                        }
                    }
                }
            }
        }

        public Dictionary<string, WonkaBizSource> SourceMap { get; set; }

        public Dictionary<string, WonkaBizSource> CustomOpMap { get; set; }

        public string DefaultSource { get; set; }

        public ITransactionState TransactionState { get; set; }

        public List<WonkaBizRuleSet> AllRuleSets { get; set; }

        public List<ISuccessTrigger> OnSuccessTriggers { get; set; }

        public List<IFailureTrigger> OnFailureTriggers { get; set; }

        #endregion

    }
}
