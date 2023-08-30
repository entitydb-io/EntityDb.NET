using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Sources;

/// <summary>
///     Represents a generic piece of information.
/// </summary>
/// <remarks>
///     For now, only <see cref="ITransaction"/> implements
///     this, but once events are added, ICommunication
///     will also implement this.
/// </remarks>
public interface ISource
{
    /// <summary>
    ///     The id associated with the source.
    /// </summary>
    Id Id { get; }

    /// <summary>
    ///     The date and time associated with the source.
    /// </summary>
    TimeStamp TimeStamp { get; }

    /// <summary>
    ///     The signature of the source agent.
    /// </summary>
    object AgentSignature { get; }
}
