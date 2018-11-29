using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace WonkaRef.Extensions
{
    /// <summary>
    /// 
    /// This extensions class provides the functionality to serialize/deserialize the data domain (i.e., WonkaRef data)
    /// with respect to a local file or an IPFS file.
    /// 
    /// </summary>
    public static class WonkaRefExtensions
    {
        /// <summary>
        /// 
        /// This method will deserialize the data domain of the WonkaRevEnvironment from a local file.
        /// 
        /// <param name="poDataDomainFile">The file location where the data domain exists to be deserialized</param>
        /// <returns>The WonkaRefEnvironment instantiated from the data domain file</returns>
        /// </summary>
        public static WonkaRefEnvironment DeserializeRefEnvFromLocalFile(this FileInfo poDataDomainFile)
        {
            WonkaRefEnvironment RefEnv = null;

            if (poDataDomainFile == null)
                throw new WonkaRefException("ERROR!  Reference to data domain file is invalid.");

            if (!poDataDomainFile.Exists)
                throw new WonkaRefException("ERROR!  Data domain file does not exist.");

            RefEnv = WonkaRefEnvironment.CreateInstance(false, new WonkaRefDeserializeLocalSource(poDataDomainFile));

            return RefEnv;
        }

        /// <summary>
        /// 
        /// This method will deserialize the data domain of the WonkaRevEnvironment from a provided string.
        /// 
        /// <param name="poDataDomainPayload">The string payload that needs to be deserialized</param>
        /// <returns>The WonkaRefEnvironment instantiated from the data domain file</returns>
        /// </summary>
        public static WonkaRefEnvironment DeserializeRefEnvFromStringPayload(this string psDataDomainPayload)
        {
            WonkaRefEnvironment RefEnv = null;

            if (String.IsNullOrEmpty(psDataDomainPayload))
                throw new WonkaRefException("ERROR!  Reference to data domain payload is invalid.");

            RefEnv = WonkaRefEnvironment.CreateInstance(false, new WonkaRefDeserializeLocalSource(psDataDomainPayload));

            return RefEnv;
        }

        /// <summary>
        /// 
        /// This method will serialize the data domain of the WonkaRevEnvironment to a local file.
        /// 
        /// <param name="poRefEnv">The instance of the WonkaRefEnvironment which we wish to serialize</param>
        /// <param name="psFileUrl">The file location where the data should be serialized to</param>
        /// <returns>Indicates whether or not the serialization was successful</returns>
        /// </summary>
        public static void SerializeToLocalFile(this WonkaRefEnvironment poRefEnv, string psFileUrl)
        {
            if (poRefEnv == null)
                throw new WonkaRefException("ERROR!  An invalid Ref envirionment has been provided.");

            if (string.IsNullOrEmpty(psFileUrl))
                throw new WonkaRefException("ERROR!  A blank file URL has been provided.");

            FileInfo TargetFile = new FileInfo(psFileUrl);
            if (!TargetFile.Directory.Exists)
                throw new WonkaRefException("ERROR!  Target directory does not exist.");

            string sRefEnvBody = SerializeToString(poRefEnv);
            if (!String.IsNullOrEmpty(sRefEnvBody))
                File.WriteAllText(psFileUrl, sRefEnvBody);
        }

        /// <summary>
        /// 
        /// This method will serialize the data domain of the WonkaRefEnvironment to IPFS.
        /// 
        /// <param name="poRefEnv">The instance of the WonkaRefEnvironment which we wish to serialize</param>
        /// <param name="psPeerKey">The IPFS node that belongs to the caller</param>
        /// <param name="psFileName">The file name where the data should be serialized to</param>
        /// <returns>Indicates whether or not the serialization was successful</returns>
        /// </summary>
        public static bool SerializeToIPFS(this WonkaRefEnvironment poRefEnv, string psPeerKey, string psFileName)
        {
            bool bResult = true;

            // NOTE: Still undecided as to whether or not this function should be here

            return bResult;
        }

        /// <summary>
        /// 
        /// This method will serialize the data domain of the WonkaRefEnvironment into a string.
        /// 
        /// <param name="poRefEnv">The instance of the WonkaRefEnvironment which we wish to serialize</param>
        /// <returns>Indicates whether or not the serialization was successful</returns>
        /// </summary>
        public static string SerializeToString(this WonkaRefEnvironment poRefEnv)
        {
            XmlSerializerNamespaces DefaultNamespaces = new XmlSerializerNamespaces();
            DefaultNamespaces.Add("", "");

            XmlWriterSettings DefaultRootWriterSettings =
                new XmlWriterSettings() { OmitXmlDeclaration = true, Indent = true, IndentChars = "  " };

            XmlSerializer RefEnvSerializer  = new XmlSerializer(typeof(WonkaRefEnvironment));
            StringBuilder RefEnvBodyBuilder = new StringBuilder();
            XmlWriter     RefEnvXmlWriter   = XmlWriter.Create(RefEnvBodyBuilder, DefaultRootWriterSettings);

            RefEnvSerializer.Serialize(RefEnvXmlWriter, poRefEnv, DefaultNamespaces);

            return RefEnvBodyBuilder.ToString();
        }
    }

}
