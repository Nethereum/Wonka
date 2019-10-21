using System;
using System.Collections.Generic;

namespace WonkaBre.Permissions
{
    /// <summary>
    /// 
    /// This exception should be used when encountering a permissions issue with using
    /// the Business Rules Engine.
    /// 
    /// </summary>
    public class WonkaBrePermissionsException : Exception
    {
        #region Properties

        public readonly uint CurrentScoreForApproval;

        public readonly uint MinReqScoreForApproval;

        public readonly HashSet<string> OwnersConfirmed;

        public readonly HashSet<string> OwnersUnconfirmed;

        #endregion

        public WonkaBrePermissionsException(string psErrorMessage) 
            : base(psErrorMessage)
        {
            this.CurrentScoreForApproval = this.MinReqScoreForApproval = 0;

            this.OwnersConfirmed   = new HashSet<string>();
            this.OwnersUnconfirmed = new HashSet<string>();
        }

        public WonkaBrePermissionsException(string psErrorMessage, ITransactionState poTransactionState) 
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