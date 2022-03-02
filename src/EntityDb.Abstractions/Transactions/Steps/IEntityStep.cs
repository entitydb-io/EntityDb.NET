namespace EntityDb.Abstractions.Transactions.Steps;

/// <summary>
/// 
/// </summary>
public interface IEntityStep : ITransactionStep
{
    /// <summary>
    ///     The state of the entity at this transaction step.
    /// </summary>
    object Entity { get; }
}
