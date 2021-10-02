using EntityDb.Abstractions.Strategies;
using EntityDb.Abstractions.Tags;
using EntityDb.Common.Entities;
using System.Linq;

namespace EntityDb.Common.Strategies
{
    internal sealed class TaggedEntityTaggingStrategy<TEntity> : ITaggingStrategy<TEntity>
        where TEntity : ITaggedEntity
    {
        public ITag[] GetTags(TEntity entity)
        {
            return entity.GetTags().ToArray();
        }
    }
}
