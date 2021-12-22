using EntityDb.Abstractions.Agents;

namespace EntityDb.TestImplementations.Agents
{
    public class NoAgentAccessor : IAgentAccessor
    {
        public IAgent GetAgent()
        {
            return new NoAgent();
        }
    }
}
