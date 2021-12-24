using EntityDb.Abstractions.Agents;
using EntityDb.Common.Tests.Implementations.AgentSignature;

namespace EntityDb.Common.Tests.Implementations.Agents
{
    public class NoAgent : IAgent
    {
        public bool HasRole(string role)
        {
            return false;
        }

        public object GetSignature()
        {
            return new NoAgentSignature();
        }
    }
}
