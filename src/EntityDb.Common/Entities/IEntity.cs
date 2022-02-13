using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Tags;
using System;
using System.Collections.Generic;

namespace EntityDb.Common.Entities
{
    /// <summary>
    ///     Provides basic functionality for the common implementations.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IEntity<TEntity>
    {
        /// <summary>
        ///     Creates a new instance of a <typeparamref name="TEntity" />.
        /// </summary>
        /// <param name="entityId">The id of the entity.</param>
        /// <returns>A new instaance of <typeparamref name="TEntity" />.</returns>
        abstract static TEntity Construct(Guid entityId);

        /// <summary>
        ///     Returns the version number of the entity.
        /// </summary>
        /// <returns></returns>
        ulong GetVersionNumber();

        /// <summary>
        ///     Returns the set of <see cref="ILease"/> objects for the entity.
        /// </summary>
        /// <returns>The set of <see cref="ILease"/>.</returns>
        IEnumerable<ILease> GetLeases();

        /// <summary>
        ///     Returns the set of <see cref="ITag"/> objects for the entity.
        /// </summary>
        /// <returns>The set of <see cref="ITag"/> objects.</returns>
        IEnumerable<ITag> GetTags();
    }
}
