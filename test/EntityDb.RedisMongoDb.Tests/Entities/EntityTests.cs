using EntityDb.Common.Tests.Entities;
using System;

namespace EntityDb.RedisMongoDb.Tests.Entities;

public class EntityTests : EntityTestsBase<Startup>
{
    public EntityTests(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}