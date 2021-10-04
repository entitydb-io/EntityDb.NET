using EntityDb.Abstractions.Facts;
using EntityDb.Common.Entities;

namespace EntityDb.Common.Facts
{
    /// <summary>
    ///     Represents the modification of the version number of an entity that implements
    ///     <see cref="IVersionedEntity{TEntity}" />.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity that is versioned.</typeparam>
    public sealed record VersionNumberSet<TEntity>(ulong VersionNumber) : IFact<TEntity>
        where TEntity : IVersionedEntity<TEntity>
    {
        /// <inheritdoc />
        public TEntity Reduce(TEntity previousEntity)
        {
            return previousEntity.WithVersionNumber(VersionNumber);
        }
    }
}
