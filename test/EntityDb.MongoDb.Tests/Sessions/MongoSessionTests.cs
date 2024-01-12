using EntityDb.Common.Exceptions;
using EntityDb.Common.Tests;
using EntityDb.MongoDb.Sources.Sessions;
using Shouldly;
using Xunit;

namespace EntityDb.MongoDb.Tests.Sessions;

public sealed class MongoSessionTests : TestsBase<Startup>
{
    public MongoSessionTests(IServiceProvider startupServiceProvider) : base(startupServiceProvider)
    {
    }

    [Fact]
    public async Task WhenExecutingWriteMethods_ThenThrow()
    {
        // ARRANGE

        var mongoSession = new MongoSession(default!, default!, default!,
            new MongoDbSourceSessionOptions { ReadOnly = true });

        // ASSERT

        await Should.ThrowAsync<ReadOnlyWriteException>(() =>
            mongoSession.Insert<object>(default!, default!, default));

        await Should.ThrowAsync<ReadOnlyWriteException>(() =>
            mongoSession.Delete<object>(default!, default!, default));

        Should.Throw<ReadOnlyWriteException>(() => mongoSession.StartTransaction());

        await Should.ThrowAsync<ReadOnlyWriteException>(() => mongoSession.CommitTransaction(default));

        await Should.ThrowAsync<ReadOnlyWriteException>(() => mongoSession.AbortTransaction());
    }
}
