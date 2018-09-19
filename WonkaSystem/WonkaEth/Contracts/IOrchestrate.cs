using System;

namespace WonkaEth.Contracts
{
    public interface IOrchestrate : ISerialize
    {
        bool Orchestrate(ICommand instance, bool pbSimulationMode);
    }
}

