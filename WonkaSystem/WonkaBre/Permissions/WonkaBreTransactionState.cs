using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Wonka.BizRulesEngine.Permissions
{
    /// <summary>
    /// 
    /// This class is a provided implementation of the ITransactionstate interface.  It can contain all state data 
    /// in relation to the pending transaction of a RuleTree, which will be invoked within the rules engine.
    /// 
    /// </summary>
    public class WonkaBreTransactionState : ITransactionState
    {
        #region CONSTANTS

        public const int CONST_MAX_OWNERS = 250;

        #endregion

        #region Properties

        public readonly string ContractAddress;

        public HashSet<string> ApprovedExecutors { get; set; }

        private uint MinReqScoreForApproval { get; set; }

        private Dictionary<string, bool> OwnerConfirmations { get; set; }

        private Dictionary<string, uint> OwnerWeights { get; set; }

        #endregion

        public WonkaBreTransactionState(IEnumerable<string> poOwners, uint pnMinReqScoreForApproval = 0, string psContractAddress = null)
        {
            this.ContractAddress = psContractAddress;

            this.ApprovedExecutors  = new HashSet<string>();
            this.OwnerWeights       = new Dictionary<string, uint>();
            this.OwnerConfirmations = new Dictionary<string, bool>();

            foreach (string sTmpOwner in poOwners)
            {
                this.OwnerWeights[sTmpOwner] = 1;

                this.OwnerConfirmations[sTmpOwner] = false;
            }

            if (this.OwnerWeights.Keys.Count >= CONST_MAX_OWNERS)
            {
                throw new WonkaBrePermissionsException("ERROR!  Too many owners were provided.  Maximum limit is [" + CONST_MAX_OWNERS + "].");
            }

            if (this.OwnerWeights.Keys.Count == 0)
            {
                throw new WonkaBrePermissionsException("ERROR!  No owners were provided.");
            }

            this.MinReqScoreForApproval = (pnMinReqScoreForApproval > 0) ? pnMinReqScoreForApproval : (uint)this.OwnerWeights.Count / 2;
        }

        #region Methods

        public void AddConfirmation(string psOwner)
        {
            if (!this.IsOwner(psOwner))
            {
                throw new WonkaBrePermissionsException("ERROR!  Account(" + psOwner + ") is not a registered owner that is associated with this RuleTree.");
            }

            this.OwnerConfirmations[psOwner] = true;
        }

        public void AddExecutor(string psExecutor)
        {
            if (!String.IsNullOrEmpty(psExecutor))
            {
                this.ApprovedExecutors.Add(psExecutor);
            }
        }

        public void ClearPendingTransaction()
        {
            this.RevokeAllConfirmations();

            // NOTE: In the case that we introduce othe state variables, they should be reset here
        }

        public uint GetCurrentScore()
        {
            uint nCurrentScore = 0;

            foreach (string sTmpOwner in this.OwnerConfirmations.Keys)
            {
                if (this.OwnerConfirmations[sTmpOwner])
                {
                    nCurrentScore += this.OwnerWeights[sTmpOwner];
                }
            }

            return nCurrentScore;
        }

        public HashSet<string> GetExecutors()
        {
            return this.ApprovedExecutors;
        }

        public uint GetMinScoreRequirement()
        {
            return this.MinReqScoreForApproval;
        }

        public HashSet<string> GetOwnersConfirmed()
        {
            HashSet<string> confirmed = new HashSet<string>();

            this.OwnerConfirmations.Where(x => this.HasConfirmed(x.Key)).ToList().ForEach(x => confirmed.Add(x.Key));

            return confirmed;
        }

        public HashSet<string> GetOwnersUnconfirmed()
        {
            HashSet<string> confirmed = new HashSet<string>();

            this.OwnerConfirmations.Where(x => !this.HasConfirmed(x.Key)).ToList().ForEach(x => confirmed.Add(x.Key));

            return confirmed;
        }

        public uint GetOwnerWeight(string psOwner)
        {
            if (!String.IsNullOrEmpty(psOwner))
            {
                if (this.OwnerWeights.ContainsKey(psOwner))
                {
                    return this.OwnerWeights[psOwner];
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        public bool HasConfirmed(string psOwner)
        {
            if (!this.IsOwner(psOwner))
            {
                throw new WonkaBrePermissionsException("ERROR!  Account(" + psOwner + ") is not a registered owner that is associated with this RuleTree.");
            }

            return this.OwnerConfirmations[psOwner];
        }

        public bool IsOwner(string psOwner)
        {
            if (String.IsNullOrEmpty(psOwner))
            {
                throw new WonkaBrePermissionsException("ERROR!  Provided owner cannot be null or blank.");
            }

            return this.OwnerWeights.ContainsKey(psOwner);
        }

        public bool IsTransactionConfirmed()
        {
            return (this.GetCurrentScore() >= this.MinReqScoreForApproval);
        }

        public void RemoveExecutor(string psExecutor)
        {
            if (!String.IsNullOrEmpty(psExecutor))
            {
                this.ApprovedExecutors.Remove(psExecutor);
            }
        }

        public void RemoveOwner(string psOwner)
        {
            if (!this.IsOwner(psOwner))
            {
                throw new WonkaBrePermissionsException("ERROR!  Account(" + psOwner + ") is not a registered owner that is associated with this RuleTree.");
            }

            this.OwnerWeights.Remove(psOwner);
            this.OwnerConfirmations.Remove(psOwner);
        }

        public bool RevokeAllConfirmations()
        {
            HashSet<string> allOwners = new HashSet<string>(this.OwnerConfirmations.Keys);

            foreach (string sTmpOwner in allOwners)
            {
                this.RevokeConfirmation(sTmpOwner);
            }

            return true;
        }

        public bool RevokeConfirmation(string psOwner)
        {
            if (!this.IsOwner(psOwner))
            {
                throw new WonkaBrePermissionsException("ERROR!  Account(" + psOwner + ") is not a registered owner that is associated with this RuleTree.");
            }

            this.OwnerConfirmations[psOwner] = false;

            return true;
        }

        public void SetMinScoreRequirement(uint pnMinReq)
        {
            if (pnMinReq == 0)
            {
                throw new WonkaBrePermissionsException("Minimum requirement has to be greater than 0.");
            }

            this.MinReqScoreForApproval = pnMinReq;
        }

        public void SetOwner(string psOwner, uint pnWeight = 1)
        {
            if (this.OwnerConfirmations.Count >= CONST_MAX_OWNERS)
            {
                throw new WonkaBrePermissionsException("ERROR!  Max count of owners [" + CONST_MAX_OWNERS + "] has already been reached.");
            }

            if (String.IsNullOrEmpty(psOwner))
            {
                throw new WonkaBrePermissionsException("ERROR!  Provided owner cannot be null or blank.");
            }

            this.OwnerWeights[psOwner] = pnWeight;
        }

        #endregion

    }
}
