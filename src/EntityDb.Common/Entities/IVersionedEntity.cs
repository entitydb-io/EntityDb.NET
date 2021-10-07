using EntityDb.Abstractions.Strategies;

namespace EntityDb.Common.Entities
{
    /// <summary>
    ///     Represents a type that can be used for an implementation of <see cref="IVersioningStrategy{TEntity}" />.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity that is versioned.</typeparam>
    public interface IVersionedEntity<out TEntity>
    {
        /// <summary>
        ///     The version number.
        /// </summary>
        ulong VersionNumber { get; }

        /// <summary>
        ///     Returns a new <typeparamref name="TEntity" /> that represents the entity at a specific version number.
        /// </summary>
        /// <param name="versionNumber">The version number.</param>
        /// <returns>A new <typeparamref name="TEntity" /> that represents the entity at <paramref name="versionNumber" />.</returns>
        TEntity WithVersionNumber(ulong versionNumber);
    }
}
