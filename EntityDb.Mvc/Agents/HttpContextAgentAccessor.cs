using EntityDb.Abstractions.Agents;
using Microsoft.AspNetCore.Http;
using System;

namespace EntityDb.Mvc.Agents
{
    internal record HttpContextAgentAccessor(IHttpContextAccessor HttpContextAccessor) : IAgentAccessor
    {
        private readonly Lazy<IAgent> _agent = new(() => new HttpContextAgent(HttpContextAccessor.HttpContext!));

        public IAgent GetAgent()
        {
            return _agent.Value;
        }
    }
}
