using System;
using System.Collections;

using Wonka.BizRulesEngine.RuleTree;

namespace Wonka.Eth.Contracts
{
    public interface ISerialize
    {
        void DeserializeRecordFromBlockchain(ICommand poCommand);

        Nethereum.Contracts.Contract GetContract(WonkaBizSource poBlockchainSource);

        void SerializeRecordToBlockchain(ICommand poCommand);
    }
}