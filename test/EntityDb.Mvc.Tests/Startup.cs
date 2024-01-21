using EntityDb.Common.Tests;
using EntityDb.Mvc.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDb.Mvc.Tests;

public sealed class Startup : StartupBase
{
    public override void AddServices(IServiceCollection serviceCollection)
    {
        base.AddServices(serviceCollection);

        // Agent

        serviceCollection.AddHttpContextAgentAccessor();
    }
}
