using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Transactions.Steps;

/// <summary>
///     Represents a modification to an entity.
/// </summary>
public interface ITransactionStep
{
    /// <summary>
    ///     The id of the entity.
    /// </summary>
    Id EntityId { get; }
    
    /// <summary>
    ///     The state of the entity associated with this step.
    /// </summary>
    object Entity { get; }

    /// <summary>
    ///     The version number associated with this step.
    /// </summary>
    VersionNumber EntityVersionNumber { get; }
}
