namespace EntityDb.Abstractions.Queries.SortBuilders
{
    /// <summary>
    ///     Builds a sort for an object repository. Possible objects include: agentSignatures, commands, facts, and leases.
    /// </summary>
    /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
    public interface ISortBuilder<TSort>
    {
        /// <summary>
        ///     Returns a <typeparamref name="TSort" /> that orders objects by transaction timestamp.
        /// </summary>
        /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
        /// <returns>A <typeparamref name="TSort" /> that orders objects by transaction timestamp.</returns>
        TSort TransactionTimeStamp(bool ascending);

        /// <summary>
        ///     Returns a <typeparamref name="TSort" /> that orders objects by transaction id.
        /// </summary>
        /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
        /// <returns>A <typeparamref name="TSort" /> that orders objects by transaction id.</returns>
        TSort TransactionId(bool ascending);

        /// <summary>
        ///     Returns a <typeparamref name="TSort" /> that orders objects ordered by a series of sorts.
        /// </summary>
        /// <param name="sorts">The series of sorts.</param>
        /// <returns>A <typeparamref name="TSort" /> that orders objects ordered by <paramref name="sorts" />.</returns>
        TSort Combine(params TSort[] sorts);
    }
}
