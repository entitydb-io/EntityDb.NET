using EntityDb.Common.Exceptions;
using EntityDb.Common.Tests;
using EntityDb.Redis.States.Sessions;
using Shouldly;
using Xunit;

namespace EntityDb.Redis.Tests.Sessions;

public class RedisSessionTests : TestsBase<Startup>
{
    public RedisSessionTests(IServiceProvider startupServiceProvider) : base(startupServiceProvider)
    {
    }

    [Fact]
    public async Task WhenExecutingWriteMethods_ThenThrow()
    {
        // ARRANGE

        var readOnlyRedisSession =
            new RedisSession(default!, default!, new RedisStateSessionOptions { ReadOnly = true });

        // ASSERT

        await Should.ThrowAsync<ReadOnlyWriteException>(() =>
            readOnlyRedisSession.Upsert(default!, default!));

        await Should.ThrowAsync<ReadOnlyWriteException>(() => readOnlyRedisSession.Delete(default!));
    }
}
