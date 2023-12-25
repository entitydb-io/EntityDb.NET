using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.ValueObjects;
using System.Collections.Immutable;

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
    
    /// <summary>
    ///     The leases to be added.
    /// </summary>
    ImmutableArray<ILease> AddLeases { get; }
    
    /// <summary>
    ///     The tags to be added.
    /// </summary>
    ImmutableArray<ITag> AddTags { get; }
    
    /// <summary>
    ///     The leases to be deleted.
    /// </summary>
    ImmutableArray<ILease> DeleteLeases { get; }
    
    /// <summary>
    ///     The tags to be deleted.
    /// </summary>
    ImmutableArray<ITag> DeleteTags { get; }

    /// <ignore />
    [Obsolete("Please use Data instead. This will be removed in a future version.")]
    object Command => Data;
}
