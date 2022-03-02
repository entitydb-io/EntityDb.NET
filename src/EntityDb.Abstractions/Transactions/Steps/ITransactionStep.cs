using System;

namespace EntityDb.Abstractions.Transactions.Steps;

/// <summary>
///     Represents a modification to an entity.
/// </summary>
public interface ITransactionStep
{
    /// <summary>
    ///     The id of the entity.
    /// </summary>
    Guid EntityId { get; }
}
