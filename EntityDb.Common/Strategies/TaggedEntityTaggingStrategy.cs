using EntityDb.Abstractions.Strategies;
using EntityDb.Abstractions.Tags;
using EntityDb.Common.Entities;
using System;
using System.Linq;

namespace EntityDb.Common.Strategies
{
    internal sealed class TaggedEntityTaggingStrategy<TEntity> : ITaggingStrategy<TEntity>
        where TEntity : ITaggedEntity
    {
        public ITag[] GetTags(Guid entityId, TEntity entity)
        {
            return entity.GetTags(entityId).ToArray();
        }
    }
}
