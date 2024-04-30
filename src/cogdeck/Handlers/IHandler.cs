namespace cogdeck.Handlers
{
    /// <summary>
    /// Interface for command handlers.
    /// </summary>
    internal interface IHandler
    {
        /// <summary>
        /// The title of the menu item.
        /// </summary>
        string MenuTitle { get; }

        /// <summary>
        /// Executes the command handler.
        /// </summary>
        /// <param name="input">Current workspace text.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>New workspace text.</returns>
        Task<string> Execute(string input, CancellationToken cancellationToken);
    }
}