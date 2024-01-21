using EntityDb.Abstractions.Sources.Agents;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Sources.Agents;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EntityDb.Mvc.Agents;

internal sealed class HttpContextAgentAccessor : IAgentAccessor
{
    private static readonly Dictionary<string, string> DefaultApplicationInfo = new();
    private readonly IAgentSignatureAugmenter? _agentSignatureAugmenter;

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOptionsFactory<HttpContextAgentSignatureOptions> _httpContextAgentOptionsFactory;

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

    public async Task<IAgent> GetAgent(string signatureOptionsName, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            throw new NoAgentException();
        }

        var applicationInfo = DefaultApplicationInfo;

        if (_agentSignatureAugmenter != null)
        {
            applicationInfo = await _agentSignatureAugmenter.GetApplicationInfo(cancellationToken);
        }

        var signatureOptions = _httpContextAgentOptionsFactory.Create(signatureOptionsName);

        var signature = HttpContextAgentSignature.GetSnapshot
        (
            httpContext,
            signatureOptions,
            applicationInfo
        );

        return new StandardAgent(signature);
    }
}
