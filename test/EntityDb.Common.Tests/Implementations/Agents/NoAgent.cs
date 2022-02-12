using EntityDb.Abstractions.Agents;
using EntityDb.Common.Tests.Implementations.AgentSignature;
using System;

namespace EntityDb.Common.Tests.Implementations.Agents
{
    public class NoAgent : IAgent
    {
        public bool HasRole(string role)
        {
            return false;
        }

        public DateTime GetTimestamp()
        {
            return DateTime.UtcNow;
        }

        public object GetSignature(string signatureOptionsName)
        {
            return new NoAgentSignature();
        }
    }
}
