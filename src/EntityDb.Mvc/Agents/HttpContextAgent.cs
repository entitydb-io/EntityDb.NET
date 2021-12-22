using EntityDb.Abstractions.Agents;
using Microsoft.AspNetCore.Http;

namespace EntityDb.Mvc.Agents
{
    internal record HttpContextAgent(HttpContext HttpContext) : IAgent
    {
        public bool HasRole(string role)
        {
            return HttpContext.User.IsInRole(role);
        }

        public object GetSignature()
        {
            return HttpContextAgentSignature.FromHttpContext(HttpContext);
        }
    }
}
