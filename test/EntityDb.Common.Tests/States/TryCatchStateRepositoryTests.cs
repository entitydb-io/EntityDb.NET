using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.States;
using EntityDb.Common.States;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace EntityDb.Common.Tests.States;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed class TryCatchStateRepositoryTests : TestsBase<Startup>
{
    public TryCatchStateRepositoryTests(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    private async Task Generic_GivenRepositoryAlwaysThrows_WhenExecutingAnyMethod_ThenExceptionIsLogged<TEntity>(
        EntityRepositoryAdder entityRepositoryAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        var logs = new List<Log>();

        var loggerFactory = GetMockedLoggerFactory(logs);

        var stateRepositoryMock = new Mock<IStateRepository<TEntity>>(MockBehavior.Strict);

        stateRepositoryMock
            .Setup(repository => repository.Get(It.IsAny<StatePointer>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException());

        stateRepositoryMock
            .Setup(repository =>
                repository.Put(It.IsAny<StatePointer>(), It.IsAny<TEntity>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException());

        stateRepositoryMock
            .Setup(repository => repository.Delete(It.IsAny<StatePointer[]>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException());

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityRepositoryAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.RemoveAll(typeof(ILoggerFactory));

            serviceCollection.AddSingleton(loggerFactory);
        });

        var tryCatchStateRepository = TryCatchStateRepository<TEntity>
            .Create(serviceScope.ServiceProvider, stateRepositoryMock.Object);

        // ACT

        var state = await tryCatchStateRepository.Get(default);
        var persisted = await tryCatchStateRepository.Put(default, default!);
        var deleted = await tryCatchStateRepository.Delete(default!);

        // ASSERT

        state.ShouldBe(default);
        persisted.ShouldBeFalse();
        deleted.ShouldBeFalse();

        logs.Count(log => log.Exception is NotImplementedException).ShouldBe(3);
    }

    [Theory]
    [MemberData(nameof(With_Entity))]
    public Task GivenRepositoryAlwaysThrows_WhenExecutingAnyMethod_ThenExceptionIsLogged(
        EntityRepositoryAdder entityRepositoryAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityRepositoryAdder.EntityType },
            new object?[] { entityRepositoryAdder }
        );
    }
}
