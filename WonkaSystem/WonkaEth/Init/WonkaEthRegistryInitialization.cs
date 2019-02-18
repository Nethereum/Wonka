using System;

namespace WonkaEth.Init
{
    /// <summary>
    /// 
    /// This class represents the information needed to communicate with the Registry.
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public class WonkaEthRegistryInitialization
    {
        public WonkaEthRegistryInitialization()
        {
            Web3HttpUrl = null;

            BlockchainRegistry = new WonkaEthSource();
        }

        public void RetrieveEmbeddedResources(System.Reflection.Assembly poTargetAssembly)
        {
            if (this.BlockchainRegistry != null)
            {
                if (String.IsNullOrEmpty(BlockchainRegistry.ContractABI) && !String.IsNullOrEmpty(BlockchainRegistry.ContractABIEmbeddedResource))
                {
                    using (var AbiReader = new System.IO.StreamReader(poTargetAssembly.GetManifestResourceStream(BlockchainRegistry.ContractABIEmbeddedResource)))
                    {
                        BlockchainRegistry.ContractABI = AbiReader.ReadToEnd();
                    }
                }
            }
        }

        public string Web3HttpUrl { get; set; }

        public WonkaEthSource BlockchainRegistry { get; set; }
    }

}
