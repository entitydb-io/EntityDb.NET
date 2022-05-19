using EntityDb.Abstractions.Agents;
using EntityDb.Abstractions.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace EntityDb.Mvc.Agents;

internal record HttpContextAgent
(
    HttpContext HttpContext,
    IOptionsFactory<HttpContextAgentSignatureOptions> HttpContextAgentSignatureOptionsFactory,
    Dictionary<string, string> ApplicationInfo
) : IAgent
{
    public TimeStamp GetTimeStamp()
    {
        return TimeStamp.UtcNow;
    }

    public object GetSignature(string signatureOptionsName)
    {
        return HttpContextAgentSignature.GetSnapshot
        (
            HttpContext,
            HttpContextAgentSignatureOptionsFactory.Create(signatureOptionsName),
            ApplicationInfo
        );
    }
}
