using EntityDb.Abstractions.Facts;
using EntityDb.Abstractions.Tags;
using EntityDb.TestImplementations.Entities;
using EntityDb.TestImplementations.Tags;
using System.Collections.Generic;

namespace EntityDb.TestImplementations.Facts
{
    public record Counted(int Number) : IFact<TransactionEntity>
    {
        public TransactionEntity Reduce(TransactionEntity entity)
        {
            var tags = new List<ITag>();

            if (entity.Tags != null)
            {
                tags.AddRange(entity.Tags);
            }

            tags.Add(new CountTag(Number));

            return entity with
            {
                Tags = tags.ToArray(),
            };
        }
    }
}
