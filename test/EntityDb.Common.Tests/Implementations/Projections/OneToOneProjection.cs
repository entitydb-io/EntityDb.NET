using System.Runtime.CompilerServices;
using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Snapshots.Transforms;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Extensions;
using EntityDb.Common.Sources.Queries.Standard;
using EntityDb.Common.Tests.Implementations.Snapshots;
using Microsoft.Extensions.DependencyInjection;
using Version = EntityDb.Abstractions.ValueObjects.Version;

namespace EntityDb.Common.Tests.Implementations.Projections;

public class OneToOneProjection : IProjection<OneToOneProjection>, ISnapshotWithTestLogic<OneToOneProjection>
{
    public TimeStamp LastSourceAt { get; set; }

    public static OneToOneProjection Construct(Pointer pointer)
    {
        return new OneToOneProjection
        {
            Pointer = pointer,
        };
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
            if (message.Delta is not IMutator<OneToOneProjection> mutator) continue;

            mutator.Mutate(this);

            Pointer = message.EntityPointer;
        }
    }

    public bool ShouldRecord()
    {
        return ShouldRecordLogic.Value is not null && ShouldRecordLogic.Value.Invoke(this);
    }

    public bool ShouldRecordAsLatest(OneToOneProjection? previousSnapshot)
    {
        return ShouldRecordAsLatestLogic.Value is not null &&
               ShouldRecordAsLatestLogic.Value.Invoke(this, previousSnapshot);
    }

    public async IAsyncEnumerable<Source> EnumerateSources
    (
        IServiceProvider serviceProvider,
        Pointer projectionPointer,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var query = new GetDeltasQuery(projectionPointer, default);

        var sourceRepository = await serviceProvider
            .GetRequiredService<ISourceRepositoryFactory>()
            .CreateRepository(TestSessionOptions.ReadOnly, cancellationToken);

        var sourceIds = await sourceRepository
            .EnumerateSourceIds(query, cancellationToken)
            .ToArrayAsync(cancellationToken);

        foreach (var sourceId in sourceIds)
            yield return await sourceRepository
                .GetSource(sourceId, cancellationToken);
    }

    public static IEnumerable<Id> EnumerateEntityIds(Source source)
    {
        return source.Messages
            .Where(message => message.Delta is IMutator<OneToOneProjection>)
            .Select(message => message.EntityPointer.Id)
            .Distinct();
    }

    public required Pointer Pointer { get; set; }

    public static string MongoDbCollectionName => "OneToOneProjections";
    public static string RedisKeyNamespace => "one-to-one-projection";

    public OneToOneProjection WithVersion(Version version)
    {
        Pointer = Pointer.Id + version;

        return this;
    }

    public static AsyncLocal<Func<OneToOneProjection, bool>?> ShouldRecordLogic { get; } = new();

    public static AsyncLocal<Func<OneToOneProjection, OneToOneProjection?, bool>?> ShouldRecordAsLatestLogic { get; } =
        new();
}