using Microsoft.Extensions.DependencyInjection;

namespace EntityDb.Common.Tests;

public interface IStartup
{
    void AddServices(IServiceCollection serviceCollection);
}