using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using System.Collections.Immutable;

namespace EntityDb.Common.Transactions;

internal record TransactionCommand : ITransactionCommand
{
    public required Id EntityId { get; init; }
    public required VersionNumber EntityVersionNumber { get; init; }
    public required object Data { get; init; } = default!;
    public required ImmutableArray<ILease> AddLeases { get; init; }
    public required ImmutableArray<ITag> AddTags { get; init; }
    public required ImmutableArray<ILease> DeleteLeases { get; init; }
    public required ImmutableArray<ITag> DeleteTags { get; init; }
}
