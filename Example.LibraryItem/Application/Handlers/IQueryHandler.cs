namespace Example.LibraryItem.Application.Handlers
{
    /// <summary>
    /// Base interface for query handlers that return data without modifying state.
    /// Query handlers should be used for read operations that don't change system state.
    /// </summary>
    /// <typeparam name="TQuery">The query object type containing query parameters</typeparam>
    /// <typeparam name="TResult">The result type returned by the query</typeparam>
    public interface IQueryHandler<in TQuery, TResult>
    {
        /// <summary>
        /// Handles the query and returns the result.
        /// </summary>
        /// <param name="query">The query to execute</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
        /// <returns>The query result</returns>
        Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
    }
}