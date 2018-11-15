using System;
using System.Collections.Generic;

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
        /// This method will serialize the data domain of the WonkaRevEnvironment to a local file.
        /// 
        /// <param name="poRefEnv">The instance of the WonkaRefEnvironment which we wish to serialize</param>
        /// <returns>Indicates whether or not the serialization was successful</returns>
        /// </summary>
        public static bool SerializeToLocalFile(this WonkaRefEnvironment poRefEnv)
        {
            bool bResult = true;

            // NOTE: Do work here

            return bResult;
        }

        /// <summary>
        /// 
        /// This method will serialize the data domain of the WonkaRefEnvironment to IPFS.
        /// 
        /// <param name="poRefEnv">The instance of the WonkaRefEnvironment which we wish to serialize</param>
        /// <returns>Indicates whether or not the serialization was successful</returns>
        /// </summary>
        public static bool SerializeToIPFS(this WonkaRefEnvironment poRefEnv)
        {
            bool bResult = true;

            // NOTE: Do work here

            return bResult;
        }

    }

}

