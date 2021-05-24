using EntityDb.Abstractions.Facts;
using EntityDb.Abstractions.Tags;
using EntityDb.TestImplementations.Entities;
using System;

namespace EntityDb.TestImplementations.Facts
{
    public record AllTagsRemoved : IFact<TransactionEntity>
    {
        public TransactionEntity Reduce(TransactionEntity entity)
        {
            return entity with
            {
                Tags = Array.Empty<ITag>(),
            };
        }
    }
}
