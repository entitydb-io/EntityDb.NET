using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Transactions;

/// <summary>
///     Represents a modification to an entity.
/// </summary>
public interface ITransactionCommand
{
    /// <summary>
    ///     The id of the entity.
    /// </summary>
    Id EntityId { get; }

    /// <summary>
    ///     The version number associated with this command.
    /// </summary>
    VersionNumber EntityVersionNumber { get; }

    /// <summary>
    ///     The command data.
    /// </summary>
    object Data { get; }

    /// <ignore />
    [Obsolete("Please use Data instead. This will be removed in a future version.")]
    object Command => Data;
}
