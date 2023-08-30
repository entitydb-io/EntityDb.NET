using EntityDb.Abstractions.Sources;
using System.Collections.Immutable;

namespace EntityDb.Abstractions.Transactions;

/// <summary>
///     Represents a set of objects which must be committed together or not at all. Possible objects include:
///     agentSignatures, commands, leases, and tags.
/// </summary>
public interface ITransaction : ISource
{
    /// <summary>
    ///     A series commands used to modify an entity.
    /// </summary>
    ImmutableArray<ITransactionCommand> Commands { get; }
}
