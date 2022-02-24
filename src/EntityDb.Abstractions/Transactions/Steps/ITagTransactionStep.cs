using EntityDb.Abstractions.Tags;

namespace EntityDb.Abstractions.Transactions.Steps;

/// <summary>
///     Represents a modification to an entity's tags.
/// </summary>
/// <typeparam name="TEntity">The type of entity to be modified.</typeparam>
public interface ITagTransactionStep<TEntity> : ITransactionStep<TEntity>
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
