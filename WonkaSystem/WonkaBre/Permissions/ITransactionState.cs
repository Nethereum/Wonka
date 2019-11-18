using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wonka.BizRulesEngine.Permissions
{
    /// <summary>
    /// 
    /// This interface defines the methods which are required by an object that will store the transaction state of a pending
    /// RuleTree execution.  The transaction state consists of:
    /// 
    /// 1.) Those stakeholders who are allowed to the execute the RuleTree
    /// 2.) Those stakeholders (and their assigned weights) who provide their consent for the impending execution of a RuleTree
    /// 3.) The minimum score required to determine whether or not the RuleTree can be executed
    /// 4.) The current score, which is decided by calculating with the weights of the consenting owners and dissenting owners
    /// 
    /// </summary>
    public interface ITransactionState
    {
        void AddConfirmation(string psOwner);

        void AddExecutor(string psExecutor);

        void ClearPendingTransaction();

        uint GetCurrentScore();

        HashSet<string> GetExecutors();

        uint GetMinScoreRequirement();

        HashSet<string> GetOwnersConfirmed();

        HashSet<string> GetOwnersUnconfirmed();

        uint GetOwnerWeight(string psOwner);

        bool HasConfirmed(string psOwner);

        bool IsOwner(string psOwner);

        bool IsTransactionConfirmed();

        void RemoveExecutor(string psExecutor);

        void RemoveOwner(string psNewOwner);

        bool RevokeAllConfirmations();

        bool RevokeConfirmation(string psOwner);

        void SetMinScoreRequirement(uint pnMinReq);

        void SetOwner(string psNewOwner, uint pnWeight);
    }
}
