using System;
using System.Collections.Generic;

using WonkaPrd;

namespace WonkaBre.RuleTree
{
    /// <summary>
    /// 
    /// When the rules engine is flagged to run in "orchestra" mode (which is the opposite of its "solo" mode), the engine
    /// will not examine a readily available record (provided by a user, pulled from the storage of a a contract, etc.).  Instead,
    /// in "orchestra" mode, the engine will be ready to retrieve the Attribute values from other venues (contracts, APIS, etc.).  
    /// This class will represent an instance of such a source that will return the string value for that Attribute.
    /// 
    /// NOTE: Currently, an API method (like a web REST call) to the outside cannot be called from the blockchain (even though it would
    /// work within a .NET instance of the engine).  However, future versions of Ethereum might create exceptions for such use cases (via IPFS, etc.).
    /// 
    /// </summary>
    public class WonkaBreSource
    {
        #region Delegates

        public delegate string RetrieveDataMethod(WonkaBreSource TargetSource, string psAttrName);

        #endregion

        public WonkaBreSource(string psSourceId, string psSenderAddr, string psPwd, string psContractAddr, string psABI, string psMethodName, string psSetterMethodName, RetrieveDataMethod poRetrievalDelegate)
        {
            SourceId     = psSourceId;
            TypeOfSource = SOURCE_TYPE.SRC_TYPE_CONTRACT;

            SenderAddress   = psSenderAddr;
            Password        = psPwd;
            ContractAddress = psContractAddr;
            ContractABI     = psABI;

            APIServerAddress = "";
            APIServerPort    = -1;

            MethodName        = psMethodName;
            SetterMethodName  = psSetterMethodName;
            RetrievalDelegate = poRetrievalDelegate;
        }

        public WonkaBreSource(string psSourceId, string psSenderAddr, string psPwd, string psContractAddr, string psABI, WonkaBreXmlReader.ExecuteCustomOperator poCustomOpDelegate, string psCustomOpMethodName)
        {
            SourceId     = psSourceId;
            TypeOfSource = SOURCE_TYPE.SRC_TYPE_CONTRACT;

            SenderAddress   = psSenderAddr;
            Password        = psPwd;
            ContractAddress = psContractAddr;
            ContractABI     = psABI;

            APIServerAddress = "";
            APIServerPort    = -1;

            CustomOpDelegate   = poCustomOpDelegate;
            CustomOpMethodName = psCustomOpMethodName; 
        }

        public WonkaBreSource(string psSourceId, string psAPISrvrAddr, int pnAPISrvrPort, string psMethodName, RetrieveDataMethod poRetrievalDelegate)
        {
            SourceId     = psSourceId;
            TypeOfSource = SOURCE_TYPE.SRC_TYPE_API;

            SenderAddress = Password = ContractAddress = ContractABI = "";

            APIServerAddress = psAPISrvrAddr;
            APIServerPort    = pnAPISrvrPort;

            MethodName        = psMethodName;
            SetterMethodName  = "";
            RetrievalDelegate = poRetrievalDelegate;
        }

        #region Properties for all scenarios

        public readonly string SourceId;

        public readonly SOURCE_TYPE TypeOfSource;

        #endregion

        #region Properties for Simple Orchestration (i.e., data getter/setter)

        public readonly RetrieveDataMethod RetrievalDelegate;

        public readonly string MethodName;

        public readonly string SetterMethodName;

        #endregion

        #region Properties for Custom Orchestration (i.e., custom operators)

        public readonly WonkaBreXmlReader.ExecuteCustomOperator CustomOpDelegate;

        public readonly string CustomOpMethodName;

        #endregion

        #region Properties for calls to blockchain contracts

        public readonly string SenderAddress;

        public readonly string Password;

        public readonly string ContractAddress;

        public readonly string ContractABI;

        #endregion

        #region Properties for calls to API server

        public readonly string APIServerAddress;

        public readonly int APIServerPort;

        #endregion
    }
}


