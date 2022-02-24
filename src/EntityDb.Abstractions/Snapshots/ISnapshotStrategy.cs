namespace EntityDb.Abstractions.Snapshots;

/// <summary>
///     Represents a type used to determine if the next version of an entity should be put into snapshot storage.
/// </summary>
/// <typeparam name="TEntity">The type of the entity that can be put into snapshot storage.</typeparam>
public interface ISnapshotStrategy<in TEntity>
{
    /// <summary>
    ///     Determines if the next version of an entity should be put into snapshot storage.
    /// </summary>
    /// <param name="previousEntity">The previous version of the entity, if it exists.</param>
    /// <param name="nextEntity">The next version of the entity.</param>
    /// <returns>
    ///     <c>true</c> if the next version of the entity should be put into snapshot storage, or <c>false</c> if the next
    ///     version of the entity should not be put into snapshot storage.
    /// </returns>
    bool ShouldPutSnapshot(TEntity? previousEntity, TEntity nextEntity);
}
