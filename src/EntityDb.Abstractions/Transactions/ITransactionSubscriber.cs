namespace EntityDb.Abstractions.Transactions
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface ITransactionSubscriber<TEntity>
    {
        /// <summary>
        /// Foo
        /// </summary>
        /// <param name="transaction"></param>
        void Notify(ITransaction<TEntity> transaction);
    }
}
