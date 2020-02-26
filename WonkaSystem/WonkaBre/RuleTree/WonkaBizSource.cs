using System;
using System.Collections.Generic;

using Wonka.BizRulesEngine.Readers;
using Wonka.BizRulesEngine.RuleTree.RuleTypes;
using Wonka.Product;

namespace Wonka.BizRulesEngine.RuleTree
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
    public class WonkaBizSource
    {
        #region Delegates

        public delegate string             RetrieveDataMethod(WonkaBizSource TargetSource, string psAttrName);
        public delegate CustomOperatorRule BuildCustomOpRuleDelegate(WonkaBizSource poSource, int pnRuleID);

        #endregion

        #region Properties for all scenarios

        public readonly string SourceId;

        public readonly SOURCE_TYPE TypeOfSource;

        #endregion

        #region Properties for Simple Orchestration (i.e., data getter/setter)

        public RetrieveDataMethod RetrievalDelegate { get; set; }

        public readonly string MethodName;

        public readonly string SetterMethodName;

        #endregion

        #region Properties for Custom Orchestration (i.e., custom operators)

        public readonly WonkaBizRulesXmlReader.ExecuteCustomOperator CustomOpDelegate;

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

		#region Properties for calls to SQL Server query/stored procedure

		public readonly string SqlServer;

		public readonly string SqlDatabase;

		public readonly string SqlUsername;

		public readonly string SqlPassword;

        public readonly string SqlQueryOrProcedure;

        #endregion

        #region Other Properties

        public BuildCustomOpRuleDelegate CustomOpRuleBuilder { get; set; }

        public string DefaultWeb3Url { get; set; }

        #endregion

        public WonkaBizSource(string psSourceId, string psSenderAddr, string psPwd, string psContractAddr, string psABI, string psMethodName, string psSetterMethodName, RetrieveDataMethod poRetrievalDelegate)
        {
            this.SourceId     = psSourceId;
            this.TypeOfSource = SOURCE_TYPE.SRC_TYPE_CONTRACT;

            this.SenderAddress   = psSenderAddr;
            this.Password        = psPwd;
            this.ContractAddress = psContractAddr;
            this.ContractABI     = psABI;

            this.APIServerAddress = string.Empty;
            this.APIServerPort    = -1;
			this.SqlServer        = this.SqlDatabase = this.SqlUsername = this.SqlPassword = this.SqlQueryOrProcedure = string.Empty;

            this.MethodName          = psMethodName;
            this.SetterMethodName    = psSetterMethodName;
            this.RetrievalDelegate   = poRetrievalDelegate;
            this.CustomOpRuleBuilder = null;
        }

        public WonkaBizSource(string psSourceId, string psSenderAddr, string psPwd, string psContractAddr, string psABI, WonkaBizRulesXmlReader.ExecuteCustomOperator poCustomOpDelegate, string psCustomOpMethodName)
        {
            this.SourceId     = psSourceId;
            this.TypeOfSource = SOURCE_TYPE.SRC_TYPE_CONTRACT;

            this.SenderAddress   = psSenderAddr;
            this.Password        = psPwd;
            this.ContractAddress = psContractAddr;
            this.ContractABI     = psABI;

            this.APIServerAddress = string.Empty;
            this.APIServerPort    = -1;
			this.SqlServer        = this.SqlDatabase = this.SqlUsername = this.SqlPassword = this.SqlQueryOrProcedure = string.Empty;

            this.CustomOpDelegate    = poCustomOpDelegate;
            this.CustomOpMethodName  = psCustomOpMethodName;
            this.CustomOpRuleBuilder = null;
        }

        public WonkaBizSource(string psSourceId, string psAPISrvrAddr, int pnAPISrvrPort, string psMethodName, RetrieveDataMethod poRetrievalDelegate)
        {
            this.SourceId     = psSourceId;
            this.TypeOfSource = SOURCE_TYPE.SRC_TYPE_API;

            this.SenderAddress = this.Password = this.ContractAddress = this.ContractABI = string.Empty;
			this.SqlServer     = this.SqlDatabase = this.SqlUsername = this.SqlPassword = this.SqlQueryOrProcedure = string.Empty;

            this.APIServerAddress = psAPISrvrAddr;
            this.APIServerPort    = pnAPISrvrPort;

            this.MethodName          = psMethodName;
            this.SetterMethodName    = string.Empty;
            this.RetrievalDelegate   = poRetrievalDelegate;
            this.CustomOpRuleBuilder = null;
        }

        public WonkaBizSource(string psSourceId, string psSqlServer, string psDatabase, string psUsername, string psPassword, string psQueryOrProcedure, RetrieveDataMethod poRetrievalDelegate)
        {
            this.SourceId     = psSourceId;
            this.TypeOfSource = SOURCE_TYPE.SRC_TYPE_STORED_PROCEDURE;

            this.SenderAddress = this.Password = this.ContractAddress = this.ContractABI = string.Empty;

            this.APIServerAddress = string.Empty;
            this.APIServerPort    = -1;
            this.MethodName       = this.SetterMethodName = string.Empty;

			this.SqlServer   = psSqlServer;
			this.SqlDatabase = psDatabase;
			this.SqlUsername = psUsername;
			this.SqlPassword = psPassword;

            this.SqlQueryOrProcedure = psQueryOrProcedure;

            this.RetrievalDelegate   = poRetrievalDelegate;
            this.CustomOpRuleBuilder = null;
        }
        
    }
}


