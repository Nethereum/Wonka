using System;

namespace WonkaEth.Init
{
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public class WonkaEthSource
    {
        public WonkaEthSource()
        {}

        public string ContractMarkupId { get; set; }

        public string ContractAddress { get; set; }

        public string ContractSender { get; set; }

        public string ContractPassword { get; set; }

        public string ContractGetterMethod { get; set; }

        public string ContractSetterMethod { get; set; }

        public string CustomOpMarkupId { get; set; }

        public string CustomOpContractMethod { get; set; }
    }

    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable=false)]
    public class WonkaEthInitialization
    {
        public WonkaEthInitialization()
        {
            BlockchainEngine      = new WonkaEthSource();
            DefaultValueRetrieval = new WonkaEthSource();

            AttributeSourceList = new WonkaEthSource[0];
            CustomOperatorList  = new WonkaEthSource[0];
        }

        public WonkaEthSource BlockchainEngine { get; set; }

        public WonkaEthSource DefaultValueRetrieval { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("AttributeSource")]
        public WonkaEthSource[] AttributeSourceList { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("CustomOperator")]
        public WonkaEthSource[] CustomOperatorList { get; set; }
    }
}