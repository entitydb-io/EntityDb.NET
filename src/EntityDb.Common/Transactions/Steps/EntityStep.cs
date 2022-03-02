using EntityDb.Abstractions.Transactions.Steps;
using System;

namespace EntityDb.Common.Transactions.Steps;

internal class EntityStep : IEntityStep
{
    public Guid EntityId { get; init; }
    public object Entity { get; init; } = default!;
}
