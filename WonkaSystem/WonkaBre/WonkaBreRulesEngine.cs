using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WonkaBre.Readers;
using WonkaBre.Reporting;
using WonkaBre.RuleTree;
using WonkaPrd;
using WonkaRef;

namespace WonkaBre
{
    /// <summary>
    /// 
    /// This class is the main console that should be used by users of this library.  All functionality
    /// should be utilized through an instance of this class.
    /// 
    /// </summary>
    public class WonkaBreRulesEngine
    {
        #region Delegates
        //public delegate bool ComplexEditsDelegate(WonkaProduct poNewProduct, WonkaProduct poOldProduct);
        public delegate WonkaProduct RetrieveOldRecordDelegate(Dictionary<string,string> KeyValues);
        #endregion

        #region CONSTANTS

        public const int CONST_ISBN_LEN = 10;
        public const int CONST_EAN_LEN  = 13;

        public static readonly int[] CONST_EAN_WEIGHTS = { 1, 3, 1, 3, 1, 3, 1, 3, 1, 3, 1, 3, 1 };

        public const int CONST_MAX_STRING_LEN = 512;

        public const int CONST_MAX_PROPS = 32;
        public const int MAX_RULESET_NES = 32;

        public const int RULE_EXEC_SEVERE_FAIL     = -1;
        public const int RULE_EXEC_SEVERE_FAIL_VAL = 666;

        public const int RULE_VALUE_MAX = 9;

        #endregion

        #region Constructors
        public WonkaBreRulesEngine(string psRulesFilepath, IMetadataRetrievable piMetadataSource = null)
        {
            if (String.IsNullOrEmpty(psRulesFilepath))
                throw new Exception("ERROR!  Provided rules file is null or empty!");

            if (!File.Exists(psRulesFilepath))
                throw new Exception("ERROR!  Provided rules file(" + psRulesFilepath + ") does not exist on the filesystem.");

            Init(piMetadataSource);

            WonkaBreXmlReader BreXmlReader = new WonkaBreXmlReader(psRulesFilepath);

            RuleTreeRoot = BreXmlReader.ParseRuleTree();
        }

        public WonkaBreRulesEngine(StringBuilder psRules, IMetadataRetrievable piMetadataSource = null)
        {
            if ((psRules == null) || (psRules.Length <= 0))
                throw new Exception("ERROR!  Provided rules are null or empty!");

            Init(piMetadataSource);

            WonkaBreXmlReader BreXmlReader = new WonkaBreXmlReader(psRules);

            RuleTreeRoot = BreXmlReader.ParseRuleTree();
        }

        #endregion

        #region Methods

        private void Init(IMetadataRetrievable piMetadataSource)
        {
            try
            {
                WonkaRefEnvironment.GetInstance();
            }
            catch (Exception ex)
            {
                WonkaRefEnvironment.CreateInstance(false, piMetadataSource);
            }

            CurrentProductOnDB = null;

            SourceId      = -1;
            TempDirectory = "C:\tmp";

            GetCurrentProductDelegate = null;
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
				if (poTargetProduct.GetProductGroup(TempAttrKey.GroupId).GetRowCount() <= 0)
                    throw new Exception("ERROR!  Provided incoming product has empty group for needed key (" + TempAttrKey.AttrName + ").");

				string sTempKeyValue = poTargetProduct.GetProductGroup(TempAttrKey.GroupId)[0][TempAttrKey.AttrId];

				if (String.IsNullOrEmpty(sTempKeyValue))
					throw new Exception("ERROR!  Provided incoming product has no value for needed key(" + TempAttrKey.AttrName + ").");

                ProductKeys[TempAttrKey.AttrName] = sTempKeyValue;
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
        public WonkaBreRuleTreeReport Validate(WonkaProduct poIncomingProduct)
        {
            WonkaRefEnvironment        WonkaRefEnv = WonkaRefEnvironment.GetInstance();
            Dictionary<string, string> ProductKeys = GetProductKeys(poIncomingProduct);

            if (poIncomingProduct == null)
                throw new Exception("ERROR!  Provided incoming product is null!");

            WonkaBreRuleTreeReport RuleTreeReport = new WonkaBreRuleTreeReport();

            if (GetCurrentProductDelegate != null)
                CurrentProductOnDB = GetCurrentProductDelegate.Invoke(ProductKeys);
            else
                CurrentProductOnDB = new WonkaProduct();

            WonkaBreRuleMediator.MediateRuleTreeExecution(RuleTreeRoot, poIncomingProduct, CurrentProductOnDB, RuleTreeReport);

            /*
             * NOTE: Do we need anything like this method
             * 
            if (PostApplicationDelegate != null)
                PostApplicationDelegate.Invoke(poIncomingProduct, CurrentProductOnDB);
            */

            return RuleTreeReport;
        }

        #endregion

        #region Properties

        private int SourceId { get; set; }

        private string TempDirectory { get; set; }

        public WonkaBreRuleSet RuleTreeRoot { get; set; }

        public WonkaProduct CurrentProductOnDB { get; set; }

        public RetrieveOldRecordDelegate GetCurrentProductDelegate { get; set; }

        #endregion

    }
}
