using EntityDb.Abstractions.Facts;

namespace EntityDb.Abstractions.Transactions
{
    /// <summary>
    /// Represents a single modifier for a single entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity being modified.</typeparam>
    public interface ITransactionFact<TEntity>
    {
        /// <summary>
        /// The order in which the modifier must be applied.
        /// </summary>
        ulong SubversionNumber { get; }

        /// <summary>
        /// The modifier.
        /// </summary>
        IFact<TEntity> Fact { get; }
    }
}
