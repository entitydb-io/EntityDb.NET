using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Strategies.Resolving;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using Xunit;

namespace EntityDb.Common.Tests.Strategies.Resolving
{
    public class LifoResolvingStrategyChainTests
    {
        [Fact]
        public void GivenResolvingStrategyThrows_WhenResolvingType_ThenExceptionIsLogged()
        {
            // ARRANGE

            var loggerMock = new Mock<ILogger>(MockBehavior.Strict);

            loggerMock
                .Setup(logger => logger.LogError(It.IsAny<Exception>(), It.IsAny<string>()))
                .Verifiable();

            var loggerFactoryMock = new Mock<ILoggerFactory>();

            loggerFactoryMock
                .Setup(factory => factory.CreateLogger(It.IsAny<Type>()))
                .Returns(loggerMock.Object);

            var resolvingStrategyMock = new Mock<IResolvingStrategy>();

            resolvingStrategyMock
                .Setup(strategy => strategy.TryResolveType(It.IsAny<Dictionary<string, string>>(), out It.Ref<Type?>.IsAny))
                .Throws(new Exception());

            var resolvingStrategyChain =
                new LifoResolvingStrategyChain(loggerFactoryMock.Object, new[] { resolvingStrategyMock.Object });

            // ASSERT

            Should.Throw<CannotResolveTypeException>(() => resolvingStrategyChain.ResolveType(default!));

            loggerMock.Verify();
        }

        delegate bool TryResolveTypeDelegate(Dictionary<string, string> headers, out Type? resolvedType);

        [Fact]
        public void GivenFirstResolvingStrategyReturnsNullAndSecondReturnsNotNull_WhenResolvingType_ThenReturnType()
        {
            // ARRANGE

            var expectedType = typeof(object);

            var loggerFactoryMock = new Mock<ILoggerFactory>(MockBehavior.Loose);

            var sequence = new MockSequence();

            var firstResolvingStrategyMock = new Mock<IResolvingStrategy>();

            firstResolvingStrategyMock
                .InSequence(sequence)
                .Setup(strategy => strategy.TryResolveType(It.IsAny<Dictionary<string, string>>(), out It.Ref<Type?>.IsAny))
                .Returns(new TryResolveTypeDelegate((Dictionary<string, string> headers, out Type? resolvedType) =>
                {
                    resolvedType = null;
                    return false;
                }));

            var secondResolvingStrategyMock = new Mock<IResolvingStrategy>();

            secondResolvingStrategyMock
                .InSequence(sequence)
                .Setup(strategy => strategy.TryResolveType(It.IsAny<Dictionary<string, string>>(), out It.Ref<Type?>.IsAny))
                .Returns(new TryResolveTypeDelegate((Dictionary<string, string> headers, out Type? resolvedType) =>
                {
                    resolvedType = expectedType;
                    return true;
                }));

            var resolvingStrategyChain =
                new LifoResolvingStrategyChain(loggerFactoryMock.Object, new[] { secondResolvingStrategyMock.Object, firstResolvingStrategyMock.Object });

            // ACT

            var actualType = resolvingStrategyChain.ResolveType(default!);

            // ASSERT

            actualType.ShouldBe(expectedType);
        }
    }
}
