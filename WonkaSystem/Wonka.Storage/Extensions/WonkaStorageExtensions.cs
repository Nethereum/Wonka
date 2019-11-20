using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Wonka.BizRulesEngine;
using Wonka.BizRulesEngine.RuleTree;
using Wonka.Eth.Autogen.BizDataStorage;
using Wonka.Eth.Extensions;
using Wonka.MetaData;
using Wonka.Product;

namespace Wonka.Storage.Extensions
{
    public static class WonkaStorageExtensions
    {
		/// <summary>
		/// 
		/// This method will assemble the new product by iterating through each specified source
		/// and retrieving the data directly from the third-party storage contract on the chain.
		/// 
		/// <param name="poEngine">The Wonka.NET instance that represents the instance on the chain</param>
		/// <param name="poKeyValues">The keys for the product whose data we wish to extract (though not yet used)</param>
		/// <param name="psWeb3Url">The URL for the Ethereum client to which we want to connect</param>
		/// <returns>Contains the assembled product data that represents the current product</returns>
		/// </summary>
		public static WonkaProduct AssembleCurrentProductFromChainSources(this WonkaBizRulesEngine poEngine, Dictionary<string, string> poKeyValues, string psWeb3Url = "")
        {
            WonkaProduct CurrentProduct = new WonkaProduct();

            // NOTE: Do work here
            if (poEngine.SourceMap != null)
            {
                foreach (string sTmpAttrName in poEngine.SourceMap.Keys)
                {
                    WonkaBizSource TmpSource  = poEngine.SourceMap[sTmpAttrName];
                    WonkaRefAttr   TargetAttr = poEngine.RefEnvHandle.GetAttributeByAttrName(sTmpAttrName);

                    string sTmpValue = TmpSource.GetAttrValueFromChain(sTmpAttrName, psWeb3Url);

                    CurrentProduct.SetAttribute(TargetAttr, sTmpValue);
                }
            }

            return CurrentProduct;
        }

        /// <summary>
        /// 
        /// This method will assemble the new product by iterating through each specified source
        /// and retrieving the data directly from the third-party storage contract on the chain.
        /// 
        /// <param name="poEngine">The Wonka.NET instance that represents the instance on the chain</param>
        /// <param name="poKeyValues">The keys for the product whose data we wish to extract (though not yet used)</param>
        /// <param name="psWeb3Url">The URL for the Ethereum client to which we want to connect</param>
        /// <returns>Contains the assembled product data that represents the current product</returns>
        /// </summary>
        public static async Task<WonkaProduct> AssembleCurrentProductFromChainSourcesAsync(this WonkaBizRulesEngine poEngine, Dictionary<string, string> poKeyValues, string psWeb3Url = "")
        {
            WonkaProduct CurrentProduct = new WonkaProduct();

            if (poEngine.SourceMap != null)
            {
                foreach (string sTmpAttrName in poEngine.SourceMap.Keys)
                {
                    WonkaBizSource TmpSource = poEngine.SourceMap[sTmpAttrName];
                    WonkaRefAttr TargetAttr  = poEngine.RefEnvHandle.GetAttributeByAttrName(sTmpAttrName);

                    string sTmpValue = await TmpSource.GetAttrValueFromChainAsync(sTmpAttrName, psWeb3Url).ConfigureAwait(false);

                    CurrentProduct.SetAttribute(TargetAttr, sTmpValue);
                }
            }

            return CurrentProduct;
        }

        /// <summary>
        /// 
        /// This method will assemble the new product by iterating through the known Attributes
        /// and retrieving the data via the Wonka engine on the chain (acting as a proxy).
        /// 
        /// <param name="poEngineSource">The address that represents the instance on the chain</param>
        /// <param name="psWeb3Url">The URL for the Ethereum client to which we want to connect</param>
        /// <returns>Contains the assembled product data that represents the current product</returns>
        /// </summary>
        public static WonkaProduct AssembleCurrentProductFromChainWonka(this Wonka.Eth.Init.WonkaEthSource poEngineSource, string psWeb3Url = "")
        {
			var RefEnv = WonkaRefEnvironment.GetInstance();

			WonkaProduct CurrentProduct = new WonkaProduct();

			var AllAttributes = new HashSet<string>();
			RefEnv.AttrCache.ForEach(x => AllAttributes.Add(x.AttrName));

			var CurrRecordOnChain = poEngineSource.GetAttrValuesViaChainEngine(AllAttributes, psWeb3Url);

			foreach (string sTmpAttrName in CurrRecordOnChain.Keys)
			{
				WonkaRefAttr TargetAttr = RefEnv.GetAttributeByAttrName(sTmpAttrName);

				CurrentProduct.SetAttribute(TargetAttr, CurrRecordOnChain[sTmpAttrName]);
			}

			return CurrentProduct;
        }

        /// <summary>
        /// 
        /// This method will assemble the new product by iterating through the known Attributes
        /// and retrieving the data via the Wonka engine on the chain (acting as a proxy).
        /// 
        /// <param name="poEngineSource">The address that represents the instance on the chain</param>
        /// <param name="psWeb3Url">The URL for the Ethereum client to which we want to connect</param>
        /// <returns>Contains the assembled product data that represents the current product</returns>
        /// </summary>
        public static async Task<WonkaProduct> AssembleCurrentProductFromChainWonkaAsync(this Wonka.Eth.Init.WonkaEthSource poEngineSource, string psWeb3Url = "")
        {
			var RefEnv = WonkaRefEnvironment.GetInstance();

			WonkaProduct CurrentProduct = new WonkaProduct();

			var AllAttributes = new HashSet<string>();
			RefEnv.AttrCache.ForEach(x => AllAttributes.Add(x.AttrName));

            var CurrRecordOnChain = 
                await poEngineSource.GetAttrValuesViaChainEngineAsync(AllAttributes, psWeb3Url).ConfigureAwait(false);

			foreach (string sTmpAttrName in CurrRecordOnChain.Keys)
			{
				WonkaRefAttr TargetAttr = RefEnv.GetAttributeByAttrName(sTmpAttrName);

				CurrentProduct.SetAttribute(TargetAttr, CurrRecordOnChain[sTmpAttrName]);
			}

			return CurrentProduct;
        }

		/// <summary>
		/// 
		/// This method will return an Attribute value using.
		/// 
		/// <param name="poTargetSource">The Source of the Attribute's location, defined on the chain</param>
		/// <param name="psAttrName">The name of the Attribute which we are seeking</param>
		/// <param name="psWeb3Url">The URL for the Ethereum client to which we want to connect</param>
		/// <returns>Contains the value of the sought Attribute</returns>
		/// </summary>
		public static string GetAttrValue(this WonkaBizSource poTargetSource, string psAttrName, string psWeb3Url = "")
		{
			return poTargetSource.GetAttrValueFromChain(psAttrName, psWeb3Url);
		}

		/// <summary>
		/// 
		/// This method will return an Attribute value using.
		/// 
		/// <param name="poTargetSource">The Source of the Attribute's location, defined on the chain</param>
		/// <param name="psAttrName">The name of the Attribute which we are seeking</param>
		/// <param name="psWeb3Url">The URL for the Ethereum client to which we want to connect</param>
		/// <returns>Contains the value of the sought Attribute</returns>
		/// </summary>
		public static async Task<string> GetAttrValueAsync(this WonkaBizSource poTargetSource, string psAttrName, string psWeb3Url = "")
		{
			return await poTargetSource.GetAttrValueFromChainAsync(psAttrName, psWeb3Url).ConfigureAwait(false);
		}

        /// <summary>
        /// 
        /// This method will return an Attribute value using.
        /// 
        /// NOTE: UNDER CONSTRUCTION
        /// 
        /// <param name="poEngine">The Wonka.NET instance that represents the instance on the chain</param>
        /// <param name="poEngineSource">The address that represents the instance on the chain</param>
        /// <param name="pnEpochTimeToStartFrom">The time of the reports from which we want to start pulling reports</param>
        /// <param name="psWeb3Url">The URL for the Ethereum client to which we want to connect</param>
        /// <returns>Contains the array of rule reports</returns>
        /// </summary>
        public static List<Wonka.Eth.Extensions.RuleTreeReport> GetRuleTreeReportsFromChainStorage(this WonkaBizRulesEngine poEngine, 
                                                                                              Wonka.Eth.Init.WonkaEthSource poEngineSource, 
                                                                                                                       long pnEpochTimeToStartFrom = 0, 
                                                                                                                     string psWeb3Url = "")
        {
            var RuleTreeReports = new List<Wonka.Eth.Extensions.RuleTreeReport>();

            // NOTE: Do work here

            return RuleTreeReports;
        }

        /// <summary>
        /// 
        /// This method will return an Attribute value using.
        /// 
        /// NOTE: UNDER CONSTRUCTION
        /// 
        /// <param name="poEngine">The Wonka.NET instance that represents the instance on the chain</param>
        /// <param name="poEngineSource">The address that represents the instance on the chain</param>
        /// <param name="pnEpochTimeToStartFrom">The time of the reports from which we want to start pulling reports</param>
        /// <param name="psWeb3Url">The URL for the Ethereum client to which we want to connect</param>
        /// <returns>Contains the array of rule reports</returns>
        /// </summary>
        public static async Task<List<Wonka.Eth.Extensions.RuleTreeReport>> GetRuleTreeReportsFromChainStorageAsync(this WonkaBizRulesEngine poEngine,
                                                                                                               Wonka.Eth.Init.WonkaEthSource poEngineSource,
                                                                                                                                        long pnEpochTimeToStartFrom = 0,
                                                                                                                                      string psWeb3Url = "")
        {
            var RuleTreeReports = new List<Wonka.Eth.Extensions.RuleTreeReport>();

            // NOTE: Do work here

            return RuleTreeReports;
        }

        /// <summary>
        /// 
        /// This method will retrieve data via an instance of the official Nethereum Storage contract.
        ///
        /// NOTE: UNDER CONSTRUCTION
        /// 
        /// <param name="poStorageSource">The address that represents the instance on the chain</param>
        /// <param name="poSaveEntity">The entity that we are trying to save to the chain</param>
        /// <param name="psWeb3Url">The URL for the Ethereum client to which we want to connect</param>
        /// <returns>Contains indicator of success</returns>
        /// </summary>
        public static bool PersistEntity(this Wonka.Eth.Init.WonkaEthSource poStorageSource, WonkaProduct poSaveEntity, string psWeb3Url = "")
		{
			BizDataStorageService storageService = poStorageSource.GetStorageService(psWeb3Url);

			// NOTE: Additional work needed
			// storageService.SetEntityRequestAsync(setEntityFunction);

			return true;
		}

        /// <summary>
        /// 
        /// This method will retrieve data via an instance of the official Nethereum Storage contract.
        ///
        /// NOTE: UNDER CONSTRUCTION
        /// 
        /// <param name="poStorageSource">The address that represents the instance on the chain</param>
        /// <param name="poSaveEntity">The entity that we are trying to save to the chain</param>
        /// <param name="psWeb3Url">The URL for the Ethereum client to which we want to connect</param>
        /// <returns>Contains indicator of success</returns>
        /// </summary>
        public static async Task<bool> PersistEntityAsync(this Wonka.Eth.Init.WonkaEthSource poStorageSource, WonkaProduct poSaveEntity, string psWeb3Url = "")
        {
            BizDataStorageService storageService = poStorageSource.GetStorageService(psWeb3Url);

            // NOTE: Additional work needed
            // storageService.SetEntityRequestAsync(setEntityFunction);

            return true;
        }

        /// <summary>
        /// 
        /// This method will retrieve data via an instance of the official Nethereum Storage contract.
        ///
        /// NOTE: UNDER CONSTRUCTION
        ///
        /// <param name="poStorageSource">The address that represents the instance on the chain</param>
        /// <param name="psWeb3Url">The URL for the Ethereum client to which we want to connect</param>
        /// <returns>Contains the assembled product data that represents the current product</returns>
        /// </summary>
        public static WonkaProduct RetrieveEntity(this Wonka.Eth.Init.WonkaEthSource poStorageSource, string psWeb3Url = "")
        {
			WonkaProduct CurrentEntity = new WonkaProduct();

			BizDataStorageService storageService = poStorageSource.GetStorageService(psWeb3Url);

			// NOTE: Additional work needed
			// storageService.GetEntityRequestAsync(getEntityFunction);

			return CurrentEntity;
		}

        /// <summary>
        /// 
        /// This method will retrieve data via an instance of the official Nethereum Storage contract.
        ///
        /// NOTE: UNDER CONSTRUCTION
        ///
        /// <param name="poStorageSource">The address that represents the instance on the chain</param>
        /// <param name="psWeb3Url">The URL for the Ethereum client to which we want to connect</param>
        /// <returns>Contains the assembled product data that represents the current product</returns>
        /// </summary>
        public static async Task<WonkaProduct> RetrieveEntityAsync(this Wonka.Eth.Init.WonkaEthSource poStorageSource, string psWeb3Url = "")
        {
            WonkaProduct CurrentEntity = new WonkaProduct();

            BizDataStorageService storageService = poStorageSource.GetStorageService(psWeb3Url);

            // NOTE: Additional work needed
            // storageService.GetEntityRequestAsync(getEntityFunction);

            return CurrentEntity;
        }

    }
}
