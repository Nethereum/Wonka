using System;

namespace Wonka.Eth.Init
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

            // Quick sanity check
            if (!String.IsNullOrEmpty(Web3HttpUrl) && (Web3HttpUri == null))
                throw new Exception("ERRROR!  Provided Web3HttpUrl(" + Web3HttpUrl + ") is invalid.");
        }

        public string Web3HttpUrl { get; set; }

        public Uri Web3HttpUri
        {
            get
            {
                Uri uriResult = null;

                bool bValid = !String.IsNullOrEmpty(Web3HttpUrl);

                if (bValid)
                {
                    bValid = Uri.TryCreate(Web3HttpUrl, UriKind.Absolute, out uriResult)
                               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                }

                return (bValid ? uriResult : null);
            }
        }

        public WonkaEthSource BlockchainRegistry { get; set; }
    }

}
