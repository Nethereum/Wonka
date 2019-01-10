using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WonkaBre.Permissions
{
    public interface ITransactionState
    {
        void AddConfirmation(string psOwner);

        void ClearPendingTransaction();

        uint GetCurrentScore();

        uint GetMinScoreRequirement();

        HashSet<string> GetOwnersConfirmed();

        HashSet<string> GetOwnersUnconfirmed();

        bool HasConfirmed(string psOwner);

        bool IsOwner(string psOwner);

        bool IsTransactionConfirmed();

        void RemoveOwner(string psNewOwner);

        bool RevokeAllConfirmations();

        bool RevokeConfirmation(string psOwner);

        void SetMinScoreRequirement(uint pnMinReq);

        void SetOwner(string psNewOwner, uint pnWeight);
    }
}
