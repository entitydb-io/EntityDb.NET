using EntityDb.Common.Exceptions;
using EntityDb.MongoDb.Sessions;
using Moq;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace EntityDb.MongoDb.Tests.Sessions
{
    public class TestModeMongoSessionWrapperTests
    {
        [Fact]
        public async Task GivenTransactionNotStartedAndNotReadOnly_WhenExecutingTransactionMethods_ThenThrow()
        {
            // ARRANGE

            const bool TransactionStarted = false;
            const bool ReadOnly = false;

            var mongoSessionMock = new Mock<IMongoSession>();

            mongoSessionMock
                .SetupGet(session => session.TransactionStarted)
                .Returns(TransactionStarted);

            var testModeMongoSession = new TestModeMongoSessionWrapper(mongoSessionMock.Object, ReadOnly);

            // ACT

            Should.Throw<TestModeException>(() => testModeMongoSession.StartTransaction());

            await Should.ThrowAsync<TestModeException>(() => testModeMongoSession.CommitTransaction());

            await Should.ThrowAsync<TestModeException>(() => testModeMongoSession.AbortTransaction());
        }

        [Fact]
        public async Task GivenTransactionNotStartedAndReadOnly_WhenExecutingTransactionMethods_ThenDoNotThrow()
        {
            // ARRANGE

            const bool TransactionStarted = false;
            const bool ReadOnly = true;

            var mongoSessionMock = new Mock<IMongoSession>();

            mongoSessionMock
                .SetupGet(session => session.TransactionStarted)
                .Returns(TransactionStarted);

            var testModeMongoSession = new TestModeMongoSessionWrapper(mongoSessionMock.Object, ReadOnly);

            // ACT

            Should.NotThrow(() => testModeMongoSession.StartTransaction());

            await Should.NotThrowAsync(() => testModeMongoSession.CommitTransaction());

            await Should.NotThrowAsync(() => testModeMongoSession.AbortTransaction());
        }
    }
}
