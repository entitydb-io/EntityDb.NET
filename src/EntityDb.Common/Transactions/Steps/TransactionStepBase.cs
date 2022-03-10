using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Transactions.Steps;

internal abstract record TransactionStepBase
{
    public Id EntityId { get; init; }

    public object Entity { get; init; } = default!;
    
    public VersionNumber EntityVersionNumber { get; init; }
}
