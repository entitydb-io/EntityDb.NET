using EntityDb.Abstractions.Agents;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;

namespace EntityDb.Mvc.Agents
{
    internal record HttpContextAgent(HttpContext HttpContext, IOptionsFactory<HttpContextAgentSignatureOptions> HttpContextAgentSignatureOptionsFactory) : IAgent
    {
        public bool HasRole(string role)
        {
            return HttpContext.User.IsInRole(role);
        }

        public DateTime GetTimestamp()
        {
            return DateTime.UtcNow;
        }

        public object GetSignature(string signatureOptionsName)
        {
            return HttpContextAgentSignature.GetSnapshot(HttpContext, HttpContextAgentSignatureOptionsFactory.Create(signatureOptionsName));
        }
    }
}
