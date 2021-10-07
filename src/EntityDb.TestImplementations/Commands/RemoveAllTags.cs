using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Tags;
using EntityDb.TestImplementations.Entities;
using System;

namespace EntityDb.TestImplementations.Commands
{
    public record RemoveAllTags : ICommand<TransactionEntity>
    {
        public TransactionEntity Reduce(TransactionEntity entity)
        {
            return entity with { VersionNumber = entity.VersionNumber + 1, Tags = Array.Empty<ITag>() };
        }
    }
}
