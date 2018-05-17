using System;
using System.Collections.Generic;
using System.Reflection;

using WonkaEth.Contracts;
using WonkaRef;

namespace WonkaSystem.CQS.Contracts
{
    public class AccountUpdateCommand : ICommand
    {
        public string AccountId { get; set; }
        public string AccountName { get; set; }
        public string Status { get; set; }
        public double UpdateValue { get; set; }
        public string AccountType { get; set; }
        public string Currency { get; set; }

        public AccountUpdateCommand()
        { }

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
                if (Prop.Name == "AccountId")
                    PropertyMap[Prop] = RefEnv.GetAttributeByAttrName("BankAccountID");
                else if (Prop.Name == "AccountName")
                    PropertyMap[Prop] = RefEnv.GetAttributeByAttrName("BankAccoutName");
                else if (Prop.Name == "Status")
                    PropertyMap[Prop] = RefEnv.GetAttributeByAttrName("AccountStatus");
                else if (Prop.Name == "UpdateValue")
                    PropertyMap[Prop] = RefEnv.GetAttributeByAttrName("AccountCurrValue");
                else if (Prop.Name == "AccountType")
                    PropertyMap[Prop] = RefEnv.GetAttributeByAttrName("AccountType");
                else if (Prop.Name == "Currency")
                    PropertyMap[Prop] = RefEnv.GetAttributeByAttrName("AccountCurrency");
            }

            return PropertyMap;
        }
    }
}