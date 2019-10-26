using System;

namespace Wonka.Eth.Contracts
{
    public interface IOrchestrate : ISerialize
    {
        bool Orchestrate(ICommand instance, bool pbSimulationMode);
    }
}

