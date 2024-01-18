using EntityDb.Abstractions;
using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.States;
using EntityDb.Abstractions.States.Transforms;
using EntityDb.Common.Sources;
using EntityDb.Common.Sources.Queries.Standard;
using EntityDb.Common.Tests.Implementations.States;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

namespace EntityDb.Common.Tests.Implementations.Projections;

public sealed class OneToOneProjection : IProjection<OneToOneProjection>, IStateWithTestLogic<OneToOneProjection>
{
    public TimeStamp LastSourceAt { get; set; }

    public required StatePointer StatePointer { get; set; }

    public static OneToOneProjection Construct(StatePointer statePointer)
    {
        return new OneToOneProjection { StatePointer = statePointer };
    }

    public StatePointer GetPointer()
    {
        return StatePointer;
    }

    public void Mutate(Source source)
    {
        LastSourceAt = source.TimeStamp;

        foreach (var message in source.Messages)
        {
            if (message.Delta is not IMutator<OneToOneProjection> mutator)
            {
                continue;
            }

            mutator.Mutate(this);

            StatePointer = message.StatePointer;
        }
    }

    public bool ShouldPersist()
    {
        return ShouldPersistLogic.Value is not null && ShouldPersistLogic.Value.Invoke(this);
    }

    public bool ShouldPersistAsLatest()
    {
        return ShouldPersistAsLatestLogic.Value is not null &&
               ShouldPersistAsLatestLogic.Value.Invoke(this);
    }

    public async IAsyncEnumerable<Source> EnumerateSources
    (
        IServiceProvider serviceProvider,
        StatePointer projectionPointer,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var query = new GetDeltasDataQuery(projectionPointer, default);

        var sourceRepository = await serviceProvider
            .GetRequiredService<ISourceRepositoryFactory>()
            .Create(TestSessionOptions.ReadOnly, cancellationToken);

        var sourceIds = await sourceRepository
            .EnumerateSourceIds(query, cancellationToken)
            .ToArrayAsync(cancellationToken);

        foreach (var sourceId in sourceIds)
        {
            yield return await sourceRepository
                .GetSource(sourceId, cancellationToken);
        }
    }

    public static IEnumerable<Id> EnumerateProjectionIds(Source source)
    {
        return source.Messages
            .Where(message => message.Delta is IMutator<OneToOneProjection>)
            .Select(message => message.StatePointer.Id);
    }

    public static string MongoDbCollectionName => "OneToOneProjections";
    public static string RedisKeyNamespace => "one-to-one-projection";

    public static AsyncLocal<Func<OneToOneProjection, bool>?> ShouldPersistLogic { get; } = new();

    public static AsyncLocal<Func<OneToOneProjection, bool>?> ShouldPersistAsLatestLogic { get; } =
        new();
}
