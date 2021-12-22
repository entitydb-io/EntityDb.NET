using EntityDb.Abstractions.Agents;
using Microsoft.AspNetCore.Http;
using System;

namespace EntityDb.Mvc.Agents
{
    internal record HttpContextAgentAccessor(IHttpContextAccessor HttpContextAccessor) : IAgentAccessor
    {
        private IAgent? _agent;

        private IAgent CreateAgent()
        {
            var httpContext = HttpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                //TODO: Well-named exception
                throw new Exception("");
            }

            return new HttpContextAgent(httpContext);
        }

        public IAgent GetAgent()
        {
            return _agent ??= CreateAgent();
        }
    }
}
