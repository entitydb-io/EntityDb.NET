using System;

namespace EntityDb.Abstractions.Transactions.Steps;

/// <summary>
///     Represents a modification to an entity.
/// </summary>
public interface ITransactionStep<TEntity>
{
    /// <summary>
    ///     The id of the entity.
    /// </summary>
    Guid EntityId { get; }
}
