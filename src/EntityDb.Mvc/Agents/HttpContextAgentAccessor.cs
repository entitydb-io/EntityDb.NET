using EntityDb.Abstractions.Agents;
using EntityDb.Common.Agents;
using EntityDb.Common.Exceptions;
using Microsoft.AspNetCore.Http;

namespace EntityDb.Mvc.Agents
{
    internal sealed class HttpContextAgentAccessor : AgentAccessorBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextAgentAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override IAgent CreateAgent()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                throw new NoAgentException();
            }

            return new HttpContextAgent(httpContext);
        }
    }
}
