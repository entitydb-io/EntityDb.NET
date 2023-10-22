using EntityDb.Common.Exceptions;
using EntityDb.Common.Tests;
using EntityDb.MongoDb.Transactions.Sessions;
using Shouldly;
using Xunit;

namespace EntityDb.MongoDb.Tests.Sessions;

public class MongoSessionTests : TestsBase<Startup>
{
    public MongoSessionTests(IServiceProvider startupServiceProvider) : base(startupServiceProvider)
    {
    }

    [Fact]
    public async Task WhenExecutingWriteMethods_ThenThrow()
    {
        // ARRANGE

        var mongoSession = new MongoSession(default!, default!, default!, new MongoDbTransactionSessionOptions
        {
            ReadOnly = true
        });

        // ASSERT

        await Should.ThrowAsync<CannotWriteInReadOnlyModeException>(() =>
            mongoSession.Insert<object>(default!, default!, default));

        await Should.ThrowAsync<CannotWriteInReadOnlyModeException>(() =>
            mongoSession.Delete<object>(default!, default!, default));

        Should.Throw<CannotWriteInReadOnlyModeException>(() => mongoSession.StartTransaction());

        await Should.ThrowAsync<CannotWriteInReadOnlyModeException>(() => mongoSession.CommitTransaction(default));

        await Should.ThrowAsync<CannotWriteInReadOnlyModeException>(() => mongoSession.AbortTransaction());
    }
}