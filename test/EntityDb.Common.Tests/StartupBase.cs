using EntityDb.Common.Extensions;
using EntityDb.Common.Sources.Agents;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDb.Common.Tests;

public abstract class StartupBase : IStartup
{
    public virtual void AddServices(IServiceCollection serviceCollection)
    {
        // Resolving

        serviceCollection.AddLifoTypeResolver();

        serviceCollection.AddMemberInfoNamePartialTypeResolver(Array.Empty<Type>());

        serviceCollection.AddDefaultPartialTypeResolver();

        // Agent Accessor

        serviceCollection.AddAgentAccessor<UnknownAgentAccessor>();

        // Source Processor Queue

        serviceCollection.AddSourceProcessorQueue(true);
    }
}