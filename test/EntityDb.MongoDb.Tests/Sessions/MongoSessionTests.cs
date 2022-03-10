using EntityDb.Common.Exceptions;
using EntityDb.Common.Tests;
using EntityDb.MongoDb.Sessions;
using Shouldly;
using System;
using System.Threading.Tasks;
using EntityDb.Common.Transactions;
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

        var mongoSession = new MongoSession(default!, default!, default!, new TransactionSessionOptions
        {
            ReadOnly = true
        });

        // ASSERT

        await Should.ThrowAsync<CannotWriteInReadOnlyModeException>(() => mongoSession.Insert<object>(default!, default!));

        await Should.ThrowAsync<CannotWriteInReadOnlyModeException>(() => mongoSession.Delete<object>(default!, default!));

        Should.Throw<CannotWriteInReadOnlyModeException>(() => mongoSession.StartTransaction());

        await Should.ThrowAsync<CannotWriteInReadOnlyModeException>(() => mongoSession.CommitTransaction());

        await Should.ThrowAsync<CannotWriteInReadOnlyModeException>(() => mongoSession.AbortTransaction());
    }
}