using EntityDb.Abstractions.Facts;
using EntityDb.Abstractions.Tags;
using EntityDb.Common.Tags;
using EntityDb.TestImplementations.Entities;
using System.Collections.Generic;

namespace EntityDb.TestImplementations.Facts
{
    public record TagAdded(string TagScope, string TagLabel, string TagValue) : IFact<TransactionEntity>
    {
        public TransactionEntity Reduce(TransactionEntity entity)
        {
            var tags = new List<ITag>();

            if (entity.Tags != null)
            {
                tags.AddRange(entity.Tags);
            }

            tags.Add(new Tag(TagScope, TagLabel, TagValue));

            return entity with
            {
                Tags = tags.ToArray(),
            };
        }
    }
}
