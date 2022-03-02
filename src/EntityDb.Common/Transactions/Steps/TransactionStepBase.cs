using System;

namespace EntityDb.Common.Transactions.Steps;

internal abstract record TransactionStepBase
{
    public Guid EntityId { get; init; }

    public object Entity { get; init; } = default!;
    
    public ulong EntityVersionNumber { get; init; }
}
