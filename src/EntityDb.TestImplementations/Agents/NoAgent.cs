using EntityDb.Abstractions.Agents;
using EntityDb.TestImplementations.AgentSignature;

namespace EntityDb.TestImplementations.Agents
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
