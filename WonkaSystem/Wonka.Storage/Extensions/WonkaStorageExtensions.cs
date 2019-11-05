using System;
using System.Collections.Generic;

using Wonka.BizRulesEngine;
using Wonka.BizRulesEngine.RuleTree;
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
        /// and retrieving the data from the chain.
        /// 
        /// <param name="poEngine">The Wonka.NET instance that represents the instance on the chain</param>
        /// <param name="poKeyValues">The keys for the product whose data we wish to extract</param>
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
        /// and retrieving the data from the chain.
        /// 
        /// <param name="poEngine">The Wonka.NET instance that represents the instance on the chain</param>
        /// <param name="poKeyValues">The keys for the product whose data we wish to extract</param>
        /// <returns>Contains the assembled product data that represents the current product</returns>
        /// </summary>
        public static WonkaProduct AssembleCurrentProductFromChainWonka(this WonkaBizRulesEngine poEngine, Dictionary<string, string> poKeyValues, string psWeb3Url = "")
        {
            WonkaProduct CurrentProduct = new WonkaProduct();

            // NOTE: Do work here
            // NOTE: TBD

            return CurrentProduct;
        }

        public static bool RetrieveData()
        {
            return true;
        }

        public static bool PersistData()
        {
            return true;
        }
    }
}
