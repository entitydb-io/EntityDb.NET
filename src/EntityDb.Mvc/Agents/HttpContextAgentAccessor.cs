using EntityDb.Abstractions.Agents;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Agents;
using EntityDb.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EntityDb.Mvc.Agents;

internal sealed class HttpContextAgentAccessor : IAgentAccessor
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

    public async Task<IAgent> GetAgentAsync(string signatureOptionsName)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            throw new NoAgentException();
        }

        var applicationInfo = DefaultApplicationInfo;

        if (_agentSignatureAugmenter != null)
        {
            applicationInfo = await _agentSignatureAugmenter.GetApplicationInfoAsync();
        }

        var signatureOptions = _httpContextAgentOptionsFactory.Create(signatureOptionsName);

        var signature = HttpContextAgentSignature.GetSnapshot
        (
            httpContext,
            signatureOptions,
            applicationInfo
        );

        return new Agent(TimeStamp.UtcNow, signature);
    }
}
