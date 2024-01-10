using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.States.Transforms;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Sources;
using EntityDb.Common.Sources.Queries.Standard;
using EntityDb.Common.Tests.Implementations.States;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using Version = EntityDb.Abstractions.ValueObjects.Version;

namespace EntityDb.Common.Tests.Implementations.Projections;

public class OneToOneProjection : IProjection<OneToOneProjection>, IStateWithTestLogic<OneToOneProjection>
{
    public TimeStamp LastSourceAt { get; set; }

    public required Pointer Pointer { get; set; }

    public static OneToOneProjection Construct(Pointer pointer)
    {
        return new OneToOneProjection { Pointer = pointer };
    }

    public Pointer GetPointer()
    {
        return Pointer;
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

            Pointer = message.StatePointer;
        }
    }

    public bool ShouldRecord()
    {
        return ShouldRecordLogic.Value is not null && ShouldRecordLogic.Value.Invoke(this);
    }

    public bool ShouldRecordAsLatest(OneToOneProjection? previousLatestState)
    {
        return ShouldRecordAsLatestLogic.Value is not null &&
               ShouldRecordAsLatestLogic.Value.Invoke(this, previousLatestState);
    }

    public async IAsyncEnumerable<Source> EnumerateSources
    (
        IServiceProvider serviceProvider,
        Pointer projectionPointer,
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

    public static IEnumerable<Id> EnumerateRelevantStateIds(Source source)
    {
        return source.Messages
            .Where(message => message.Delta is IMutator<OneToOneProjection>)
            .Select(message => message.StatePointer.Id)
            .Distinct();
    }

    public static string MongoDbCollectionName => "OneToOneProjections";
    public static string RedisKeyNamespace => "one-to-one-projection";

    public static AsyncLocal<Func<OneToOneProjection, bool>?> ShouldRecordLogic { get; } = new();

    public static AsyncLocal<Func<OneToOneProjection, OneToOneProjection?, bool>?> ShouldRecordAsLatestLogic { get; } =
        new();

    public OneToOneProjection WithVersion(Version version)
    {
        Pointer = Pointer.Id + version;

        return this;
    }
}
