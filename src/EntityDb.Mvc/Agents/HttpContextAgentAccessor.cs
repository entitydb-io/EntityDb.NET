using EntityDb.Abstractions.Agents;
using EntityDb.Common.Agents;
using EntityDb.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace EntityDb.Mvc.Agents;

internal sealed class HttpContextAgentAccessor : AgentAccessorBase
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOptionsFactory<HttpContextAgentSignatureOptions> _httpContextAgentOptionsFactory;
    private readonly IAgentSignatureAugmenter? _agentSignatureAugmenter;

    public HttpContextAgentAccessor
    (
        IHttpContextAccessor httpContextAccessor,
        IOptionsFactory<HttpContextAgentSignatureOptions> httpContextAgentOptionsFactory,
        IAgentSignatureAugmenter? agentSignatureAugmenter = null
    )
    {
        _httpContextAccessor = httpContextAccessor;
        _httpContextAgentOptionsFactory = httpContextAgentOptionsFactory;
        _agentSignatureAugmenter = agentSignatureAugmenter;
    }

    private static readonly Dictionary<string, string> DefaultApplicationInfo = new();

    protected override IAgent CreateAgent()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            throw new NoAgentException();
        }

        var applicationInfo = _agentSignatureAugmenter?
            .GetApplicationInfo() ?? DefaultApplicationInfo;

        return new HttpContextAgent(httpContext, _httpContextAgentOptionsFactory, applicationInfo);
    }
}
