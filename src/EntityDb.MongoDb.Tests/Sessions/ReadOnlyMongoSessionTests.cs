using EntityDb.Common.Exceptions;
using EntityDb.Common.Tests;
using EntityDb.MongoDb.Sessions;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace EntityDb.MongoDb.Tests.Sessions
{
    public class ReadOnlyMongoSessionTests : TestsBase<Startup>
    {
        public ReadOnlyMongoSessionTests(IServiceProvider startupServiceProvider) : base(startupServiceProvider)
        {
        }

        [Fact]
        public async Task WhenExecutingWriteMethods_ThenThrow()
        {
            // ARRANGE

            var readOnlyMongoSession = new ReadOnlyMongoSession(default!, default!, default!, default!);

            // ASSERT

            await Should.ThrowAsync<CannotWriteInReadOnlyModeException>(() => readOnlyMongoSession.Insert<object>(default!, default!));

            await Should.ThrowAsync<CannotWriteInReadOnlyModeException>(() => readOnlyMongoSession.Delete<object>(default!, default!));

            Should.Throw<CannotWriteInReadOnlyModeException>(() => readOnlyMongoSession.StartTransaction());
            
            await Should.ThrowAsync<CannotWriteInReadOnlyModeException>(() => readOnlyMongoSession.CommitTransaction());

            await Should.ThrowAsync<CannotWriteInReadOnlyModeException>(() => readOnlyMongoSession.AbortTransaction());
        }

        [Fact]
        public void TransactionStartedAlwaysFalse()
        {
            // ARRANGE

            var readOnlyMongoSession = new ReadOnlyMongoSession(default!, default!, default!, default!);

            // ASSERT

            readOnlyMongoSession.TransactionStarted.ShouldBeFalse();
        }
    }
}
