﻿using EntityDb.Common.Extensions;
using EntityDb.Common.Tests;
using EntityDb.Common.Tests.Implementations.Entities;
using EntityDb.Common.Tests.Implementations.Strategies;
using EntityDb.Redis.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDb.Redis.Tests
{
    public class Startup : StartupBase
    {
        public override void AddServices(IServiceCollection serviceCollection)
        {
            base.AddServices(serviceCollection);

            // Snapshots

            serviceCollection.AddSnapshottingStrategy<TransactionEntity, TransactionEntitySnapshottingStrategy>();

            serviceCollection.AddRedisSnapshots<TransactionEntity>
            (
                TransactionEntity.RedisKeyNamespace,
                _ => "127.0.0.1:6379",
                true
            );
        }
    }
}
