namespace EntityDb.Common.Transactions
{
    /// <summary>
    ///     Modifies the behavior of a repository to accomodate tests.
    /// </summary>
    public enum TransactionTestMode
    {
        /// <summary>
        ///     Transactions will be aborted after all transaction repositories have been disposed.
        /// </summary>
        /// <remarks>
        ///     This is the preferred option when a test is using a service provider that is shared with other tests.
        /// </remarks>
        AllRepositoriesDisposed,

        /// <summary>
        ///     Transactions will be aborted after the transaction repository factory has been disposed.
        /// </summary>
        /// <remarks>
        ///     This is the preferred option when a test is creating a service provider that will only be used by that test.
        /// </remarks>
        RepositoryFactoryDisposed
    }
}
