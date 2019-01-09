using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WonkaBre.Permissions
{
    /// <summary>
    /// 
    /// This class will contain all state data in relation to the pending transaction of a RuleTree,
    /// which will be invoked within the rules engine.
    /// 
    /// </summary>
    public class WonkaBreTransactionState
    {
        #region CONSTANTS

        public const int CONST_MAX_OWNERS = 250;

        #endregion

        public WonkaBreTransactionState(IEnumerable<string> poOwners, int pnMinReqScoreForApproval = 0)
        {
            PendingTransactionConfirmed = false;

            OwnerScores = new Dictionary<string, int>();

            foreach (string sTmpOwner in poOwners)
            {
                OwnerScores[sTmpOwner] = 1;

                OwnerConfirmations[sTmpOwner] = false;
            }                

            if (OwnerScores.Keys.Count == 0)
                throw new Exception("ERROR!  No owners were provided.");

            MinReqScoreForApproval = (pnMinReqScoreForApproval > 0) ? pnMinReqScoreForApproval : OwnerScores.Count / 2;
        }

        #region Methods

        public void AddConfirmation(string psOwner)
        {
            // NOTE: To be implemented
        }

        public void ClearPendingTransaction()
        {
            // NOTE: To be implemented
        }

        public bool HasConfirmed(string psOwner)
        {
            // NOTE: To be implemented
            return true;
        }

        public bool IsOwner(string psOwner)
        {
            // NOTE: To be implemented
            return true;
        }

        public bool IsTransactionConfirmed()
        {
            // NOTE: To be implemented
            return true;
        }

        public void RemoveOwner(string psNewOwner)
        {
            // NOTE: To be implemented
        }

        public bool RevokeAllConfirmations()
        {
            // NOTE: To be implemented
            return true;
        }

        public bool RevokeConfirmation(string psOwner)
        {
            // NOTE: To be implemented
            return true;
        }

        public void SetMinRequirement(int pnMinReq)
        {
            // NOTE: To be implemented
        }

        public void SetOwner(string psNewOwner, int pnWeight = 1)
        {
            // NOTE: To be implemented
        }

        #endregion

        #region Properties

        private bool PendingTransactionConfirmed { get; set; }

        private int MinReqScoreForApproval { get; set; }

        private Dictionary<string, bool> OwnerConfirmations { get; set; }

        private Dictionary<string, int> OwnerScores { get; set; }

        #endregion
    }
}
