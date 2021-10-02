using EntityDb.Abstractions.Strategies;
using EntityDb.Abstractions.Tags;
using System.Collections.Generic;

namespace EntityDb.Common.Entities
{
    /// <summary>
    /// Represents a type that can be used for an implementation of <see cref="ITaggingStrategy{TEntity}"/>.
    /// </summary>
    public interface ITaggedEntity
    {
        /// <inheritdoc cref="ITaggingStrategy{TEntity}.GetTags(TEntity)"/>
        public IEnumerable<ITag> GetTags();
    }
}
