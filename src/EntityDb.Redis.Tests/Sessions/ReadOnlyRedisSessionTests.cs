using EntityDb.Common.Exceptions;
using EntityDb.Common.Tests;
using EntityDb.Redis.Sessions;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace EntityDb.Redis.Tests.Sessions
{
    public class ReadOnlyRedisSessionTests : TestsBase<Startup>
    {
        public ReadOnlyRedisSessionTests(IServiceProvider startupServiceProvider) : base(startupServiceProvider)
        {
        }

        [Fact]
        public async Task WhenExecutingWriteMethods_ThenThrow()
        {
            // ARRANGE

            var readOnlyRedisSession = new ReadOnlyRedisSession(default!, default!);

            // ASSERT

            await Should.ThrowAsync<CannotWriteInReadOnlyModeException>(() => readOnlyRedisSession.Insert(default!, default!));

            await Should.ThrowAsync<CannotWriteInReadOnlyModeException>(() => readOnlyRedisSession.Delete(default!));
        }
    }
}
