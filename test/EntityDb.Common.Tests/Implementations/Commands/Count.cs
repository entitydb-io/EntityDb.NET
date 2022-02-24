using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Tags;
using EntityDb.Common.Tests.Implementations.Entities;
using EntityDb.Common.Tests.Implementations.Leases;
using EntityDb.Common.Tests.Implementations.Tags;
using System.Collections.Generic;

namespace EntityDb.Common.Tests.Implementations.Commands
{
    public record Count(int Number) : ICommand<TransactionEntity>
    {
        public TransactionEntity Reduce(TransactionEntity entity)
        {
            return entity with
            {
                VersionNumber = entity.VersionNumber + 1,
            };
        }
    }
}
