using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Transactions.Steps;

/// <summary>
///     Represents a transaction step that appends a command.
/// </summary>
public interface IAppendCommandTransactionStep : ITransactionStep
{
    /// <summary>
    ///     The command that needs to be appended.
    /// </summary>
    object Command { get; }

    /// <summary>
    ///     The expected version number of the command committed before this one.
    /// </summary>
    /// <remarks>
    ///     The value zero is reserved to indicate that this command is the first command.
    /// </remarks>
    VersionNumber PreviousEntityVersionNumber { get; }
}
