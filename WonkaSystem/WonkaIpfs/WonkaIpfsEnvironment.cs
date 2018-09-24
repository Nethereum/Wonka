using System;
using System.Collections.Generic;
using System.Linq;

namespace WonkaIpfs
{
    /// <summary>
    /// 
    /// This singleton, when initialized, will contain all of the cached metadata
    /// that drives the Wonka System.
    /// 
    /// </summary>
    public class WonkaIpfsEnvironment
    {
        private static object mLock = new object();

        private static WonkaIpfsEnvironment mInstance = null;

        private WonkaIpfsEnvironment()
        {
        }

        static public WonkaIpfsEnvironment CreateInstance()
        {
            lock (mLock)
            {
                if (mInstance == null)
                    mInstance = new WonkaIpfsEnvironment();

                return mInstance;
            }
        }

        static public WonkaIpfsEnvironment GetInstance()
        {
            lock (mLock)
            {
                if (mInstance == null)
                    throw new Exception("ERROR!  WonkaIpfsEnvironment has not yet been initialized!");

                return mInstance;
            }
        }

        #region Methods

        // NOTE: We can only add files once all projects are set to .NET 4.6.2 and we then add back
        //       the current .NET security and crypto libraries
        public string AddFile(string psPeerKey, string psFileName, string psFileBody)
        {
            // NOTE: TBD
            return "";
        }

        public void Log(string psPeerKeyId, string psLogBody)
        {
            // NOTE: TBD
        }

        public string GetFile(string psPeerKeyAndFileName)
        {
            var ipfs = new Ipfs.Api.IpfsClient();

            string filename = psPeerKeyAndFileName;

            return ipfs.FileSystem.ReadAllTextAsync(filename).Result;
        }

        public string GetFile(string psPeerKeyId, string psFileName)
        {
            var ipfs = new Ipfs.Api.IpfsClient();

            string filename = psPeerKeyId + "/" + psFileName;

            return ipfs.FileSystem.ReadAllTextAsync(filename).Result;
        }

        public void Test()
        {
            var ipfs = new Ipfs.Api.IpfsClient();

            const string filename = "QmXarR6rgkQ2fDSHjSY5nM2kuCXKYGViky5nohtwgF65Ec/about";

            string text = ipfs.FileSystem.ReadAllTextAsync(filename).Result;
        }

        #endregion

        #region Properties 

        // NOTE: TBD

        #endregion
    }

}
