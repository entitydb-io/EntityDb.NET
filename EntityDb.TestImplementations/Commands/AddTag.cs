using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Facts;
using EntityDb.TestImplementations.Entities;
using EntityDb.TestImplementations.Facts;
using System.Collections.Generic;

namespace EntityDb.TestImplementations.Commands
{
    public record AddTag(string TagLabel, string TagValue) : ICommand<TransactionEntity>
    {
        public IEnumerable<IFact<TransactionEntity>> Execute(TransactionEntity entity)
        {
            yield return new TagAdded(TagLabel, TagValue);
        }
    }
}
