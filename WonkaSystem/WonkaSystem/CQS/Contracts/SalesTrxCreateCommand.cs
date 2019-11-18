using System;
using System.Collections.Generic;
using System.Reflection;

using Wonka.BizRulesEngine.RuleTree;
using Wonka.Eth.Contracts;
using Wonka.MetaData;

namespace WonkaSystem.CQS.Contracts
{
    public class SalesTrxCreateCommand : ICommand
    {
        public long?   NewSalesTrxSeq      { get; set; }
        public int?    NewSaleVATRateDenom { get; set; }
        public string  NewSaleItemType     { get; set; }
        public string  CountryOfSale       { get; set; }
        public double? NewSalePrice        { get; set; }
        public double? PrevSellTaxAmt      { get; set; }
        public double? NewSellTaxAmt       { get; set; }
        public double? NewVATAmtForHMRC    { get; set; }
        public long?   NewSaleEAN          { get; set; }

        public SalesTrxCreateCommand()
        {
            NewSalesTrxSeq      = null;
            NewSaleVATRateDenom = null;
            NewSaleItemType     = CountryOfSale = null;
            NewSalePrice        = PrevSellTaxAmt = NewSellTaxAmt = NewVATAmtForHMRC = null;
            NewSaleEAN          = null;
        }

        public PropertyInfo[] GetProperties()
        {
            return this.GetType().GetProperties();
        }

        public Dictionary<PropertyInfo, WonkaRefAttr> GetPropertyMap()
        {
            WonkaRefEnvironment RefEnv = WonkaRefEnvironment.GetInstance();

            Dictionary<PropertyInfo, WonkaRefAttr> PropertyMap = new Dictionary<PropertyInfo, WonkaRefAttr>();

            foreach (PropertyInfo Prop in GetProperties())
            {
                if (Prop.Name == "NewSalesTrxSeq")
                    PropertyMap[Prop] = RefEnv.GetAttributeByAttrName("NewSalesTransSeq");
                else if (Prop.Name == "NewSaleVATRateDenom")
                    PropertyMap[Prop] = RefEnv.GetAttributeByAttrName("NewSaleVATRateDenom");
                else if (Prop.Name == "NewSaleItemType")
                    PropertyMap[Prop] = RefEnv.GetAttributeByAttrName("NewSaleItemType");
                else if (Prop.Name == "CountryOfSale")
                    PropertyMap[Prop] = RefEnv.GetAttributeByAttrName("CountryOfSale");
                else if (Prop.Name == "NewSalePrice")
                    PropertyMap[Prop] = RefEnv.GetAttributeByAttrName("NewSalePrice");
                else if (Prop.Name == "PrevSellTaxAmt")
                    PropertyMap[Prop] = RefEnv.GetAttributeByAttrName("PrevSellTaxAmount");
                else if (Prop.Name == "NewSellTaxAmt")
                    PropertyMap[Prop] = RefEnv.GetAttributeByAttrName("NewSellTaxAmount");
                else if (Prop.Name == "NewVATAmtForHMRC")
                    PropertyMap[Prop] = RefEnv.GetAttributeByAttrName("NewVATAmountForHMRC");
                else if (Prop.Name == "NewSaleEAN")
                    PropertyMap[Prop] = RefEnv.GetAttributeByAttrName("NewSaleEAN");                
            }

            return PropertyMap;
        }
    }
}