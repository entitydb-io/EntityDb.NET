using EntityDb.Common.Transactions;
using System;

namespace EntityDb.Common.Exceptions
{
    /// <summary>
    /// The exception that is thrown when an actor passes an entity id to <see cref="TransactionBuilder{TEntity}.Load(Guid, Abstractions.Transactions.ITransactionRepository{TEntity}, Abstractions.Snapshots.ISnapshotRepository{TEntity}?)"/> with an entity id that has already been loaded.
    /// </summary>
    public sealed class EntityAlreadyLoadedException : Exception
    {
    }
}
