using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Facts;

namespace EntityDb.Common.Entities
{
    /// <summary>
    /// Represents a type that can be used for an implementation of <see cref="IVersioningStrategy{TEntity}"/> which uses <see cref="VersionNumberSet{TEntity}"/> to modify the version number.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity that is versioned.</typeparam>
    public interface IVersionedEntity<TEntity>
    {
        /// <summary>
        /// The version number.
        /// </summary>
        ulong VersionNumber { get; }

        /// <summary>
        /// Returns a new <typeparamref name="TEntity"/> that represents the entity at a specific version number.
        /// </summary>
        /// <param name="versionNumber">The version number.</param>
        /// <returns>A new <typeparamref name="TEntity"/> that represents the entity at <paramref name="versionNumber"/>.</returns>
        TEntity WithVersionNumber(ulong versionNumber);
    }
}
