using System;
using System.Collections.Generic;

namespace Wonka.BizRulesEngine.Permissions
{
    /// <summary>
    /// 
    /// This exception should be used when encountering a permissions issue with using
    /// the Business Rules Engine.
    /// 
    /// </summary>
    public class WonkaBizPermissionsException : Exception
    {
        #region Properties

        public readonly uint CurrentScoreForApproval;

        public readonly uint MinReqScoreForApproval;

        public readonly HashSet<string> OwnersConfirmed;

        public readonly HashSet<string> OwnersUnconfirmed;

        #endregion

        public WonkaBizPermissionsException(string psErrorMessage) 
            : base(psErrorMessage)
        {
            this.CurrentScoreForApproval = this.MinReqScoreForApproval = 0;

            this.OwnersConfirmed   = new HashSet<string>();
            this.OwnersUnconfirmed = new HashSet<string>();
        }

        public WonkaBizPermissionsException(string psErrorMessage, ITransactionState poTransactionState) 
            : base(psErrorMessage)
        {
            this.CurrentScoreForApproval = this.MinReqScoreForApproval = 0;

            this.OwnersConfirmed   = new HashSet<string>();
            this.OwnersUnconfirmed = new HashSet<string>();

            if (poTransactionState != null)
            {
                this.CurrentScoreForApproval = poTransactionState.GetCurrentScore();
                this.MinReqScoreForApproval  = poTransactionState.GetMinScoreRequirement();

                this.OwnersConfirmed   = poTransactionState.GetOwnersConfirmed();
                this.OwnersUnconfirmed = poTransactionState.GetOwnersUnconfirmed();
            }
        }
    }
}