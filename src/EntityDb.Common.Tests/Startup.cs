using EntityDb.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDb.Common.Tests
{
    public class Startup : StartupBase
    {
        public override void AddServices(IServiceCollection serviceCollection)
        {
            base.AddServices(serviceCollection);

            // Just Added For Coverage

            serviceCollection.AddMemberInfoNameResolvingStrategy(new[] { typeof(object) });
        }
    }
}
