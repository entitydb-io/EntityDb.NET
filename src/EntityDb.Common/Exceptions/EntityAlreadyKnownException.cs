using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Transactions.Builders;

namespace EntityDb.Common.Exceptions;

/// <summary>
///     The exception that is thrown when an actor passes an entity id to
///     <see cref="TransactionBuilder{TEntity}.Load(Id, TEntity)" />
///     with an entity id that has already been loaded.
/// </summary>
public sealed class EntityAlreadyKnownException : Exception
{
}
