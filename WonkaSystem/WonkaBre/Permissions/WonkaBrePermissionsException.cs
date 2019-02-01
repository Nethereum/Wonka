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
    class WonkaBrePermissionsException : Exception
    {
        public WonkaBrePermissionsException(string psErrorMessage) : base(psErrorMessage)
        {
            CurrentScoreForApproval = MinReqScoreForApproval = 0;

            OwnersConfirmed   = new HashSet<string>();
            OwnersUnconfirmed = new HashSet<string>();
        }

        public WonkaBrePermissionsException(string psErrorMessage, ITransactionState poTransactionState) : base(psErrorMessage)
        {
            CurrentScoreForApproval = MinReqScoreForApproval = 0;

            OwnersConfirmed   = new HashSet<string>();
            OwnersUnconfirmed = new HashSet<string>();

            if (poTransactionState != null)
            {
                CurrentScoreForApproval = poTransactionState.GetCurrentScore();
                MinReqScoreForApproval  = poTransactionState.GetMinScoreRequirement();

                OwnersConfirmed   = poTransactionState.GetOwnersConfirmed();
                OwnersUnconfirmed = poTransactionState.GetOwnersUnconfirmed();
            }
        }

        #region Properties

        public readonly uint CurrentScoreForApproval;

        public readonly uint MinReqScoreForApproval;

        public readonly HashSet<string> OwnersConfirmed;

        public readonly HashSet<string> OwnersUnconfirmed;

        #endregion
    }
}