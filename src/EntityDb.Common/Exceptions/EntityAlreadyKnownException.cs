using EntityDb.Abstractions.Transactions.Builders;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Exceptions;

/// <summary>
///     The exception that is thrown when an actor passes an entity id to
///     <see cref="ITransactionBuilder{TEntity}.Load(Id, TEntity)" />
///     with an entity id that has already been loaded.
/// </summary>
public sealed class EntityAlreadyKnownException : Exception
{
}
