using System;
using System.Collections.Generic;
using EntityDb.Common.Agents;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDb.Common.Tests.Agents;

public class UnknownAgentAccessorTests : AgentAccessorTestsBase<Startup, object>
{
    public UnknownAgentAccessorTests(IServiceProvider startupServiceProvider) : base(startupServiceProvider)
    {
    }

    protected override bool CanBeInactive => false;

    protected override void ConfigureInactiveAgentAccessor(IServiceCollection serviceCollection)
    {
        throw new NotSupportedException();
    }

    protected override void ConfigureActiveAgentAccessor(IServiceCollection serviceCollection, object options)
    {
        // Does not need to do anything
    }

    protected override IEnumerable<object> GetAgentAccessorOptions()
    {
        return new[]
        {
            new object()
        };
    }

    protected override Dictionary<string, string>? GetApplicationInfo(object agentSignature)
    {
        return agentSignature is not UnknownAgentSignature unknownAgentSignature
            ? null
            : unknownAgentSignature.ApplicationInfo;
    }
}