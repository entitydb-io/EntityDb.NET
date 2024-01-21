using EntityDb.Common.Tests;
using EntityDb.Redis.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDb.Redis.Tests;

public sealed class Startup : StartupBase
{
    public override void AddServices(IServiceCollection serviceCollection)
    {
        base.AddServices(serviceCollection);

        serviceCollection.AddJsonElementEnvelopeService();
    }
}
