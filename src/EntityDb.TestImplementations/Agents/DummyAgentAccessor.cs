using EntityDb.Abstractions.Agents;

namespace EntityDb.TestImplementations.Agents
{
    public class DummyAgentAccessor : IAgentAccessor
    {
        public IAgent GetAgent()
        {
            return new DummyAgent();
        }
    }
}
