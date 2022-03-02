namespace EntityDb.Abstractions.Transactions.Steps;

/// <summary>
///     Represents a modification to an entity's state.
/// </summary>
public interface ICommandTransactionStep : ITransactionStep
{
    /// <summary>
    ///     The modifier.
    /// </summary>
    object Command { get; }

    /// <summary>
    ///     The version number for the previous command.
    /// </summary>
    ulong PreviousEntityVersionNumber { get; }

    /// <summary>
    ///     The version number for this command.
    /// </summary>
    ulong NextEntityVersionNumber { get; }
}
