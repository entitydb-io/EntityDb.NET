using EntityDb.Abstractions.Agents;
using EntityDb.Common.Agents;
using EntityDb.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EntityDb.Mvc.Agents;

internal sealed class HttpContextAgentAccessor : AgentAccessorBase
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOptionsFactory<HttpContextAgentSignatureOptions> _httpContextAgentOptionsFactory;

    public HttpContextAgentAccessor(IHttpContextAccessor httpContextAccessor, IOptionsFactory<HttpContextAgentSignatureOptions> httpContextAgentOptionsFactory)
    {
        _httpContextAccessor = httpContextAccessor;
        _httpContextAgentOptionsFactory = httpContextAgentOptionsFactory;
    }

    protected override IAgent CreateAgent()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            throw new NoAgentException();
        }

        return new HttpContextAgent(httpContext, _httpContextAgentOptionsFactory);
    }
}
