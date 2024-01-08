using System.Collections.Immutable;
using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Sources.Agents;
using EntityDb.Common.Tests.Implementations.Snapshots;

namespace EntityDb.Common.Tests.Implementations.Seeders;

public static class SourceSeeder
{
    public static Source Create(params Message[] messages)
    {
        return new Source
        {
            Id = Id.NewId(),
            TimeStamp = TimeStamp.UtcNow,
            AgentSignature = new UnknownAgentSignature(new Dictionary<string, string>()),
            Messages = messages.ToImmutableArray(),
        };
    }

    public static Source Create<TEntity>(Id entityId, uint numDeltas,
        ulong previousVersionValue = 0)
        where TEntity : class, IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        var sourceMessages = MessageSeeder
            .CreateFromDeltas<TEntity>(entityId, numDeltas, previousVersionValue).ToArray();

        return Create(sourceMessages);
    }
}