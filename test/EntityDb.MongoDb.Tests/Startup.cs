using EntityDb.Common.Tests;
using EntityDb.MongoDb.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDb.MongoDb.Tests;

public class Startup : StartupBase
{
    public override void AddServices(IServiceCollection serviceCollection)
    {
        base.AddServices(serviceCollection);

        serviceCollection.AddBsonDocumentEnvelopeService(true);
    }
}