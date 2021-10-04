using EntityDb.Common.Transactions;
using System;

namespace EntityDb.Common.Exceptions
{
    /// <summary>
    /// The exception that is thrown when an actor passes a command to <see cref="TransactionBuilder{TEntity}.Append(Guid, Abstractions.Commands.ICommand{TEntity})"/> with an entity id which has not been loaded.
    /// </summary>
    public sealed class EntityNotLoadedException : Exception
    {
    }
}
