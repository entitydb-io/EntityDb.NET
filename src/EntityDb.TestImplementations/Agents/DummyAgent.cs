using EntityDb.Abstractions.Agents;

namespace EntityDb.TestImplementations.Agents
{
    public class DummyAgent : IAgent
    {
        public bool HasRole(string role)
        {
            return false;
        }
    }
}
