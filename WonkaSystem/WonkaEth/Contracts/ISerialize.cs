using System;
using System.Collections;

using WonkaBre.RuleTree;

namespace WonkaEth.Contracts
{
    public interface ISerialize<T> where T : ICommand
    {
        void DeserializeRecordFromBlockchain(T poCommand);

        Nethereum.Contracts.Contract GetContract(WonkaBreSource poBlockchainSource);

        void SerializeRecordToBlockchain(T poCommand);
    }
}

