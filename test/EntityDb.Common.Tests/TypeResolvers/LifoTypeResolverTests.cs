using System;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Extensions;
using EntityDb.Common.TypeResolvers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace EntityDb.Common.Tests.TypeResolvers;

public class LifoTypeResolverTests : TestsBase<Startup>
{
    public LifoTypeResolverTests(IServiceProvider startupServiceProvider) : base(startupServiceProvider)
    {
    }

    [Fact]
    public void GivenPartialTypeResolverThrows_WhenResolvingType_ThenExceptionIsLogged()
    {
        // ARRANGE

        var (loggerFactory, loggerVerifier) = GetMockedLoggerFactory<Exception>();

        var partialTypeResolver = new Mock<IPartialTypeResolver>();

        partialTypeResolver
            .Setup(resolver => resolver.TryResolveType(It.IsAny<EnvelopeHeaders>(), out It.Ref<Type?>.IsAny))
            .Throws(new Exception());

        var typeResolver =
            new LifoTypeResolver(loggerFactory.CreateLogger<LifoTypeResolver>(), new[] { partialTypeResolver.Object });

        // ASSERT

        Should.Throw<CannotResolveTypeException>(() => typeResolver.ResolveType(default!));

        loggerVerifier.Invoke(Times.Once());
    }

    private delegate bool TryResolveTypeDelegate(EnvelopeHeaders headers, out Type? resolvedType);

    [Fact]
    public void GivenFirstPartialTypeResolverReturnsNullAndSecondReturnsNotNull_WhenResolvingType_ThenReturnType()
    {
        // ARRANGE

        var expectedType = typeof(object);

        var sequence = new MockSequence();

        var firstPartialTypeResolver = new Mock<IPartialTypeResolver>(MockBehavior.Strict);

        firstPartialTypeResolver
            .InSequence(sequence)
            .Setup(resolver => resolver.TryResolveType(It.IsAny<EnvelopeHeaders>(), out It.Ref<Type?>.IsAny))
            .Returns(new TryResolveTypeDelegate((EnvelopeHeaders _, out Type? resolvedType) =>
            {
                resolvedType = null;
                return false;
            }));

        var secondPartialTypeResolver = new Mock<IPartialTypeResolver>(MockBehavior.Strict);

        secondPartialTypeResolver
            .InSequence(sequence)
            .Setup(resolver => resolver.TryResolveType(It.IsAny<EnvelopeHeaders>(), out It.Ref<Type?>.IsAny))
            .Returns(new TryResolveTypeDelegate((EnvelopeHeaders _, out Type? resolvedType) =>
            {
                resolvedType = expectedType;
                return true;
            }));

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.RemoveAll(typeof(IPartialTypeResolver));
            serviceCollection.RemoveAll(typeof(ITypeResolver));

            serviceCollection.AddLifoTypeResolver();

            serviceCollection.AddSingleton(secondPartialTypeResolver.Object);
            serviceCollection.AddSingleton(firstPartialTypeResolver.Object);
        });

        var typeResolver = serviceScope.ServiceProvider.GetRequiredService<ITypeResolver>();

        // ACT

        var actualType = typeResolver.ResolveType(default);

        // ASSERT

        actualType.ShouldBe(expectedType);
    }
}