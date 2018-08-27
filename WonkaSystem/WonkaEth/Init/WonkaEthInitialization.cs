using System;

namespace WonkaEth.Init
{
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public class WonkaEthSource
    {
        private string msBusinessRulesEmbeddedResource;
        private string msBusinessRulesFileResource;

        private string msContractABIEmbeddedResource;
        private string msContractABIFileResource;

        public WonkaEthSource()
        {
            ContractABI = BusinessRules = null;

            msBusinessRulesEmbeddedResource = msBusinessRulesFileResource = "";
            msContractABIEmbeddedResource   = msContractABIFileResource   = "";
        }

        public string ContractMarkupId { get; set; }

        public string ContractAddress { get; set; }

        public string ContractSender { get; set; }

        public string ContractPassword { get; set; }

        public string TargetAttrName { get; set; }

        public string ContractGetterMethod { get; set; }

        public string ContractSetterMethod { get; set; }

        public string CustomOpMarkupId { get; set; }

        public string CustomOpContractMethod { get; set; }

        public string ContractABIEmbeddedResource
        { 
            get { return msContractABIEmbeddedResource;  } 

            set 
            {
                msContractABIEmbeddedResource = value;

                /*
                 * NOTE: Not working yet
                 * 
                if (!String.IsNullOrEmpty(msContractABIEmbeddedResource))
                {
                    var TmpAssembly = System.Reflection.Assembly.GetCallingAssembly();

                    using (var AbiReader = new System.IO.StreamReader(TmpAssembly.GetManifestResourceStream(msContractABIEmbeddedResource)))
                    {
                        ContractABI = AbiReader.ReadToEnd();
                    }
                }
                */
            }
        }

        public string ContractABIFileResource
        {
            get { return msContractABIFileResource; }

            set
            {
                msContractABIFileResource = value;

                if (!String.IsNullOrEmpty(msContractABIFileResource))
                {
                    ContractABI = System.IO.File.ReadAllText(msContractABIFileResource);
                }
            }
        }

        public string ContractABI { get; set; }

        public string BusinessRulesEmbeddedResource
        {
            get { return msBusinessRulesEmbeddedResource; }

            set { msBusinessRulesEmbeddedResource = value; }
        }

        public string BusinessRulesFileResource
        {
            get { return msBusinessRulesFileResource; }

            set
            {
                msBusinessRulesFileResource = value;

                if (!String.IsNullOrEmpty(msBusinessRulesFileResource))
                {
                    BusinessRules = System.IO.File.ReadAllText(msBusinessRulesFileResource);
                }
            }
        }

        public string BusinessRules { get; set; }
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

        public void RetrieveEmbeddedResources(System.Reflection.Assembly poTargetAssembly)
        {
            if (this.BlockchainEngine != null)
            {
                if (String.IsNullOrEmpty(BlockchainEngine.ContractABI) && !String.IsNullOrEmpty(BlockchainEngine.ContractABIEmbeddedResource))
                {
                    using (var AbiReader = new System.IO.StreamReader(poTargetAssembly.GetManifestResourceStream(BlockchainEngine.ContractABIEmbeddedResource)))
                    {
                        BlockchainEngine.ContractABI = AbiReader.ReadToEnd();
                    }
                }

                if (String.IsNullOrEmpty(BlockchainEngine.BusinessRules) && !String.IsNullOrEmpty(BlockchainEngine.BusinessRulesEmbeddedResource))
                {
                    using (var RulesReader = new System.IO.StreamReader(poTargetAssembly.GetManifestResourceStream(BlockchainEngine.BusinessRulesEmbeddedResource)))
                    {
                        BlockchainEngine.BusinessRules = RulesReader.ReadToEnd();
                    }
                }
            }

            if (this.DefaultValueRetrieval != null)
            {
                if (String.IsNullOrEmpty(DefaultValueRetrieval.ContractABI) && !String.IsNullOrEmpty(DefaultValueRetrieval.ContractABIEmbeddedResource))
                {
                    using (var AbiReader = new System.IO.StreamReader(poTargetAssembly.GetManifestResourceStream(DefaultValueRetrieval.ContractABIEmbeddedResource)))
                    {
                        DefaultValueRetrieval.ContractABI = AbiReader.ReadToEnd();
                    }
                }
            }

            if (this.AttributeSourceList != null)
            {
                foreach (WonkaEthSource TmpEthSource in AttributeSourceList)
                {
                    if (String.IsNullOrEmpty(TmpEthSource.ContractABI) && !String.IsNullOrEmpty(TmpEthSource.ContractABIEmbeddedResource))
                    {
                        using (var AbiReader = new System.IO.StreamReader(poTargetAssembly.GetManifestResourceStream(TmpEthSource.ContractABIEmbeddedResource)))
                        {
                            TmpEthSource.ContractABI = AbiReader.ReadToEnd();
                        }
                    }
                }
            }

            if (CustomOperatorList != null)
            {
                foreach (WonkaEthSource TmpEthSource in CustomOperatorList)
                {
                    if (String.IsNullOrEmpty(TmpEthSource.ContractABI) && !String.IsNullOrEmpty(TmpEthSource.ContractABIEmbeddedResource))
                    {
                        using (var AbiReader = new System.IO.StreamReader(poTargetAssembly.GetManifestResourceStream(TmpEthSource.ContractABIEmbeddedResource)))
                        {
                            TmpEthSource.ContractABI = AbiReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        public WonkaEthSource BlockchainEngine { get; set; }

        public WonkaEthSource DefaultValueRetrieval { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("AttributeSource")]
        public WonkaEthSource[] AttributeSourceList { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("CustomOperator")]
        public WonkaEthSource[] CustomOperatorList { get; set; }
    }
}