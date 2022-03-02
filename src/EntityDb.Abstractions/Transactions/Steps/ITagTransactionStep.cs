using EntityDb.Abstractions.Tags;

namespace EntityDb.Abstractions.Transactions.Steps;

/// <summary>
///     Represents a modification to an entity's tags.
/// </summary>
public interface ITagTransactionStep : ITransactionStep
{
    /// <summary>
    ///     The tags of the entity.
    /// </summary>
    ITransactionMetaData<ITag> Tags { get; }

    /// <summary>
    ///     The version number of the entity to record when tags are inserted.
    /// </summary>
    ulong TaggedAtEntityVersionNumber { get; }
}
