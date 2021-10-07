using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Transactions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.Common.Tests.Extensions
{
    /// <summary>
    ///     Extensions on <see cref="IEntityRepository{TEntity}"/>
    /// </summary>
    public static class EntityRepositoryExtensions
    {
        public static async Task ForEachCommandWithPreviousEntity<TEntity>
        (
            this IEntityRepository<TEntity> entityRepository,
            ITransaction<TEntity> transaction,
            Action<TEntity> processor
        )
        {
            var entityTaskDictionary = transaction.Commands
                .GroupBy(transactionCommand => transactionCommand.EntityId)
                .ToDictionary
                (
                    group => group.Key,
                    group =>
                    {
                        var firstCommand = group.First();

                        return entityRepository.GetAtVersion(firstCommand.EntityId, firstCommand.EntityVersionNumber - 1);
                    }
                );

            await Task.WhenAll(entityTaskDictionary.Values);

            var entityDictionary = entityTaskDictionary
                .ToDictionary
                (
                    x => x.Key,
                    x => x.Value.Result
                );

            foreach (var transactionCommand in transaction.Commands)
            {
                var previousEntity = entityDictionary[transactionCommand.EntityId];
                
                processor.Invoke(previousEntity);

                entityDictionary[transactionCommand.EntityId] = transactionCommand.Command.Reduce(previousEntity);
            }
        }
    }
}
