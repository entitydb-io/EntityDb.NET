using EntityDb.Abstractions.Loggers;
using EntityDb.Common.Exceptions;
using EntityDb.Common.TypeResolvers;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using Xunit;

namespace EntityDb.Common.Tests.TypeResolvers;

public class LifoTypeResolverTests
{
    [Fact]
    public void GivenPartialTypeResolverThrows_WhenResolvingType_ThenExceptionIsLogged()
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

        var partialTypeResolver = new Mock<IPartialTypeResolver>();

        partialTypeResolver
            .Setup(resolver => resolver.TryResolveType(It.IsAny<Dictionary<string, string>>(), out It.Ref<Type?>.IsAny))
            .Throws(new Exception());

        var typeResolver =
            new LifoTypeResolver(loggerFactoryMock.Object, new[] { partialTypeResolver.Object });

        // ASSERT

        Should.Throw<CannotResolveTypeException>(() => typeResolver.ResolveType(default!));

        loggerMock.Verify();
    }

    private delegate bool TryResolveTypeDelegate(Dictionary<string, string> headers, out Type? resolvedType);

    [Fact]
    public void GivenFirstPartialTypeResolverReturnsNullAndSecondReturnsNotNull_WhenResolvingType_ThenReturnType()
    {
        // ARRANGE

        var expectedType = typeof(object);

        var loggerFactoryMock = new Mock<ILoggerFactory>(MockBehavior.Loose);

        var sequence = new MockSequence();

        var firstPartialTypeResolver = new Mock<IPartialTypeResolver>();

        firstPartialTypeResolver
            .InSequence(sequence)
            .Setup(resolver => resolver.TryResolveType(It.IsAny<Dictionary<string, string>>(), out It.Ref<Type?>.IsAny))
            .Returns(new TryResolveTypeDelegate((Dictionary<string, string> _, out Type? resolvedType) =>
            {
                resolvedType = null;
                return false;
            }));

        var secondPartialTypeResolver = new Mock<IPartialTypeResolver>();

        secondPartialTypeResolver
            .InSequence(sequence)
            .Setup(resolver => resolver.TryResolveType(It.IsAny<Dictionary<string, string>>(), out It.Ref<Type?>.IsAny))
            .Returns(new TryResolveTypeDelegate((Dictionary<string, string> _, out Type? resolvedType) =>
            {
                resolvedType = expectedType;
                return true;
            }));

        var typeResolver =
            new LifoTypeResolver(loggerFactoryMock.Object, new[] { secondPartialTypeResolver.Object, firstPartialTypeResolver.Object });

        // ACT

        var actualType = typeResolver.ResolveType(default!);

        // ASSERT

        actualType.ShouldBe(expectedType);
    }
}