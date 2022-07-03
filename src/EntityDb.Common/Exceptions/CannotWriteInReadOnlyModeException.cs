using EntityDb.Abstractions.Transactions;
using System;

namespace EntityDb.Common.Exceptions;

/// <summary>
///     The exception that is thrown when an actor passes a <see cref="ITransaction" /> to an
///     <see cref="ITransactionRepository" /> that was created for read-only mode.
/// </summary>
public class CannotWriteInReadOnlyModeException : Exception
{
}
