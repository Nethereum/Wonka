using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WonkaRef;

namespace WonkaSystem.TestHarness
{
    public class WonkaMetadataVATSource : IMetadataRetrievable
    {
        public WonkaMetadataVATSource()
        { }

        #region Standard Metadata Cache (Minimum Set)

        public List<WonkaRefAttr> GetAttrCache()
        {
            List<WonkaRefAttr> AttrCache = new List<WonkaRefAttr>();

            AttrCache.Add(new WonkaRefAttr() { AttrId = 1, AttrName = "NewSalesTransSeq",    FieldId = 101, GroupId = 1, IsAudited = false, IsNumeric = true, IsKey = true });
            AttrCache.Add(new WonkaRefAttr() { AttrId = 2, AttrName = "NewSaleVATRateDenom", FieldId = 102, GroupId = 1, IsAudited = true, IsNumeric = true, });
            AttrCache.Add(new WonkaRefAttr() { AttrId = 3, AttrName = "NewSaleItemType",     FieldId = 103, GroupId = 1, IsAudited = false, MaxLength = 1024 });
            AttrCache.Add(new WonkaRefAttr() { AttrId = 4, AttrName = "CountryOfSale",       FieldId = 104, GroupId = 1, IsAudited = true, MaxLength = 3 });
            AttrCache.Add(new WonkaRefAttr() { AttrId = 5, AttrName = "NewSalePrice",        FieldId = 105, GroupId = 1, IsAudited = true, IsNumeric = true });
            AttrCache.Add(new WonkaRefAttr() { AttrId = 6, AttrName = "PrevSellTaxAmount",   FieldId = 106, GroupId = 1, IsAudited = true, IsNumeric = true });
            AttrCache.Add(new WonkaRefAttr() { AttrId = 7, AttrName = "NewSellTaxAmount",    FieldId = 107, GroupId = 1, IsAudited = true, IsNumeric = true });
            AttrCache.Add(new WonkaRefAttr() { AttrId = 8, AttrName = "NewVATAmountForHMRC", FieldId = 108, GroupId = 1, IsAudited = true, IsNumeric = true });
            AttrCache.Add(new WonkaRefAttr() { AttrId = 9, AttrName = "NewSaleEAN",          FieldId = 109, GroupId = 1, IsAudited = false, IsNumeric = true, IsKey = false });

            return AttrCache;
        }

        public List<WonkaRefCurrency> GetCurrencyCache()
        {
            List<WonkaRefCurrency> CurrencyCache = new List<WonkaRefCurrency>();

            CurrencyCache.Add(new WonkaRefCurrency() { CurrencyId = 1, CurrencyCd = "USD", USDCost = 1,        USDList = 1 });
            CurrencyCache.Add(new WonkaRefCurrency() { CurrencyId = 2, CurrencyCd = "EUR", USDCost = 1.24f,    USDList = 1.24f });
            CurrencyCache.Add(new WonkaRefCurrency() { CurrencyId = 3, CurrencyCd = "CNY", USDCost = 0.16f,    USDList = 0.16f });
            CurrencyCache.Add(new WonkaRefCurrency() { CurrencyId = 4, CurrencyCd = "BTC", USDCost = 9722.73f, USDList = 9722.73f });
            CurrencyCache.Add(new WonkaRefCurrency() { CurrencyId = 5, CurrencyCd = "USD", USDCost = 811.68f,  USDList = 811.68f });

            return CurrencyCache;
        }

        public List<WonkaRefField> GetFieldCache()
        {
            List<WonkaRefField> FieldCache = new List<WonkaRefField>();

            FieldCache.Add(new WonkaRefField() { FieldId = 101, FieldName = "NewSalesTransSeq",    GroupId = 1, AttrIds = new List<int>() { 1 } });
            FieldCache.Add(new WonkaRefField() { FieldId = 102, FieldName = "NewSaleVATRateDenom", GroupId = 1, AttrIds = new List<int>() { 2 } });
            FieldCache.Add(new WonkaRefField() { FieldId = 103, FieldName = "NewSaleItemType",     GroupId = 1, AttrIds = new List<int>() { 3 } });
            FieldCache.Add(new WonkaRefField() { FieldId = 104, FieldName = "CountryOfSale",       GroupId = 1, AttrIds = new List<int>() { 4 } });
            FieldCache.Add(new WonkaRefField() { FieldId = 105, FieldName = "NewSalePrice",        GroupId = 1, AttrIds = new List<int>() { 5 } });
            FieldCache.Add(new WonkaRefField() { FieldId = 106, FieldName = "PrevSellTaxAmount",   GroupId = 1, AttrIds = new List<int>() { 6 } });
            FieldCache.Add(new WonkaRefField() { FieldId = 107, FieldName = "NewSellTaxAmount",    GroupId = 1, AttrIds = new List<int>() { 7 } });
            FieldCache.Add(new WonkaRefField() { FieldId = 108, FieldName = "NewVATAmountForHMRC", GroupId = 1, AttrIds = new List<int>() { 8 } });
            FieldCache.Add(new WonkaRefField() { FieldId = 109, FieldName = "NewSaleEAN",          GroupId = 1, AttrIds = new List<int>() { 9 } });

            return FieldCache;
        }

        public List<WonkaRefGroup> GetGroupCache()
        {
            List<WonkaRefGroup> GroupCache = new List<WonkaRefGroup>();

            GroupCache.Add(new WonkaRefGroup() { GroupId = 1, GroupName = "Main", Description = "Main data" });

            return GroupCache;
        }

        public List<WonkaRefSource> GetSourceCache()
        {
            List<WonkaRefSource> SourceCache = new List<WonkaRefSource>();

            SourceCache.Add(new WonkaRefSource() { SourceId = 1, SourceName = "TransUnion", Status = "ACT" });
            SourceCache.Add(new WonkaRefSource() { SourceId = 2, SourceName = "Experian", Status = "ACT" });

            return SourceCache;
        }

        public List<WonkaRefSourceField> GetSourceFieldCache()
        {
            List<WonkaRefSourceField> SourceFieldCache = new List<WonkaRefSourceField>();

            SourceFieldCache.Add(new WonkaRefSourceField() { SourceFieldId = 1, SourceId = 1, FieldId = 102, SecurityLevel = 3 });
            SourceFieldCache.Add(new WonkaRefSourceField() { SourceFieldId = 2, SourceId = 1, FieldId = 2,   SecurityLevel = 1 });
            SourceFieldCache.Add(new WonkaRefSourceField() { SourceFieldId = 3, SourceId = 2, FieldId = 102, SecurityLevel = 3 });
            SourceFieldCache.Add(new WonkaRefSourceField() { SourceFieldId = 4, SourceId = 2, FieldId = 2,   SecurityLevel = 1 });

            return SourceFieldCache;
        }

        public List<WonkaRefStandard> GetStandardCache()
        {
            List<WonkaRefStandard> StandardCache = new List<WonkaRefStandard>();

            return StandardCache;
        }

        #endregion

        #region Extended Metadata Cache

        public List<WonkaRefAttrCollection> GetAttrCollectionCache()
        {
            List<WonkaRefAttrCollection> AttrCollCache = new List<WonkaRefAttrCollection>();

            return AttrCollCache;
        }

        #endregion
    }
}
