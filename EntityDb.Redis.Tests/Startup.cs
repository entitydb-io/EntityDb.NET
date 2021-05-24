using EntityDb.Common.Extensions;
using EntityDb.Redis.Extensions;
using EntityDb.TestImplementations.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Redis2Go;
using Xunit.DependencyInjection;
using Xunit.DependencyInjection.Logging;

namespace EntityDb.Redis.Tests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddLogging();

            serviceCollection.AddSingleton((serviceProvider) =>
            {
                var task = RedisRunner.StartAsync();

                task.Wait();

                return task.Result;
            });

            serviceCollection.AddDefaultResolvingStrategy();

            serviceCollection.AddLifoResolvingStrategyChain();

            serviceCollection.AddTestModeRedisSnapshots<TransactionEntity>(TransactionEntity.RedisKeyNamespace, (serviceProvider) =>
            {
                var redisRunner = serviceProvider.GetRequiredService<RedisRunner>();

                return $"127.0.0.1:{redisRunner.Port}";
            });
        }

        public void Configure(ILoggerFactory loggerFactory, ITestOutputHelperAccessor testOutputHelperAccessor)
        {
            loggerFactory.AddProvider(new XunitTestOutputLoggerProvider(testOutputHelperAccessor));
        }
    }
}
