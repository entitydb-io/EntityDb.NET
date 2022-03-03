using EntityDb.Abstractions.Agents;
using EntityDb.Abstractions.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EntityDb.Mvc.Agents;

internal record HttpContextAgent(HttpContext HttpContext, IOptionsFactory<HttpContextAgentSignatureOptions> HttpContextAgentSignatureOptionsFactory) : IAgent
{
    public bool HasRole(string role)
    {
        return HttpContext.User.IsInRole(role);
    }

    public TimeStamp GetTimeStamp()
    {
        return TimeStamp.UtcNow;
    }

    public object GetSignature(string signatureOptionsName)
    {
        return HttpContextAgentSignature.GetSnapshot(HttpContext, HttpContextAgentSignatureOptionsFactory.Create(signatureOptionsName));
    }
}
