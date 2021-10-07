using EntityDb.Abstractions.Commands;
using EntityDb.Common.Extensions;
using Moq;
using System.Linq;
using Xunit;

namespace EntityDb.Common.Tests.Extensions
{
    public class EntityExtensions
    {
        [Theory]
        [InlineData(2)]
        public void GivenEntityAndRepeatedCommand_WhenExecutingAndReducing_ThenEnsureReduceIsCalled(
            int numberOfTimes)
        {
            // ARRANGE

            var commandMock = new Mock<ICommand<object>>(MockBehavior.Strict);

            commandMock
                .Setup(command => command.Reduce(It.IsAny<object>()))
                .Returns((object @object) => @object)
                .Verifiable();

            var entity = new object();

            // ACT

            entity.Reduce(Enumerable.Repeat(commandMock.Object, numberOfTimes));

            // ASSERT

            commandMock.Verify(command => command.Reduce(It.IsAny<object>()), Times.Exactly(numberOfTimes));
        }
    }
}
