using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WonkaBre.Permissions
{
    public interface ITransactionState
    {
        void AddOwner(string psNewOwner, int pnWeight);

        void AddConfirmation(string psOwner);

        void ClearPendingTransaction();

        bool HasConfirmed(string psOwner);

        bool IsOwner(string psOwner);

        bool IsTransactionConfirmed();

        void RemoveOwner(string psNewOwner);

        bool RevokeAllConfirmations();

        bool RevokeConfirmation(string psOwner);

        void SetMinRequirement(int pnMinReq);
    }
}
