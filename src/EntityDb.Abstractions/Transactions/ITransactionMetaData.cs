using System.Collections.Immutable;

namespace EntityDb.Abstractions.Transactions
{
    /// <summary>
    ///     Represemts a set of metadata for a single entity.
    /// </summary>
    /// <typeparam name="TMetaData"></typeparam>
    public interface ITransactionMetaData<TMetaData>
    {
        /// <summary>
        ///     Meta data properties which must be deleted.
        /// </summary>
        ImmutableArray<TMetaData> Delete { get; }

        /// <summary>
        ///     Meta data properties which must be inserted.
        /// </summary>
        ImmutableArray<TMetaData> Insert { get; }
    }
}
