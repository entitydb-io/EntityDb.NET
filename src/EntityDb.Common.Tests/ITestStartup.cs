using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.DependencyInjection;

namespace EntityDb.Common.Tests
{
    public interface ITestStartup
    {
        void ConfigureServices(IServiceCollection serviceCollection);
        void Configure(ILoggerFactory loggerFactory, ITestOutputHelperAccessor testOutputHelperAccessor);
    }
}
