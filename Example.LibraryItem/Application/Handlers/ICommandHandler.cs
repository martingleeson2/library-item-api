namespace Example.LibraryItem.Application.Handlers
{
    /// <summary>
    /// Base interface for command handlers that modify state.
    /// Command handlers should be used for write operations that change system state.
    /// </summary>
    /// <typeparam name="TCommand">The command object type containing command data</typeparam>
    /// <typeparam name="TResult">The result type returned by the command</typeparam>
    public interface ICommandHandler<in TCommand, TResult>
    {
        /// <summary>
        /// Handles the command and returns the result.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
        /// <returns>The command result</returns>
        Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
    }
}