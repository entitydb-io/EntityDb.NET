using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Tags;
using EntityDb.Common.Tests.Implementations.Entities;
using System;

namespace EntityDb.Common.Tests.Implementations.Commands
{
    public record RemoveAllTags : ICommand<TransactionEntity>
    {
        public TransactionEntity Reduce(TransactionEntity entity)
        {
            return entity with { VersionNumber = entity.VersionNumber + 1, Tags = Array.Empty<ITag>() };
        }
    }
}
