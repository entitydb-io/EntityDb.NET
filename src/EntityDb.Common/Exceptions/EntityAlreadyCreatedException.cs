using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions;
using System;

namespace EntityDb.Common.Exceptions
{
    /// <summary>
    ///     The exception that is thrown when an actor passes a command to
    ///     <see cref="TransactionBuilder{TEntity}.Create(Guid, Abstractions.Commands.ICommand{TEntity})" /> with an entity id
    ///     which is known to already be created.
    /// </summary>
    /// <remarks>
    ///     The transaction builder does not attempt to auto-load entities. If an entity exists but has not been loaded into
    ///     the transaction builder, the <see cref="EntityAlreadyCreatedException" /> will not be thrown. Howevever, an
    ///     <see cref="OptimisticConcurrencyException" /> will be logged if an attempt is made to commit the transaction, and
    ///     <see cref="ITransactionRepository{TEntity}.PutTransaction(ITransaction{TEntity})" /> will return false.
    /// </remarks>
    public sealed class EntityAlreadyCreatedException : Exception
    {
    }
}
