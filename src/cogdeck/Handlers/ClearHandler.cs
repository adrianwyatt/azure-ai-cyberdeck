namespace cogdeck.Handlers
{
    /// <summary>
    /// Handles the "Clear" command.
    /// </summary>
    internal class ClearHandler : IHandler
    {
        public string MenuTitle => "Clear";
        private readonly StatusManager _statusManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClearHandler"/> class.
        /// </summary>
        public ClearHandler(
            StatusManager statusManager)
        {
            _statusManager= statusManager;
        }
        
        /// <summary>
        /// Clears the workspace.
        /// </summary>
        public Task<string> Execute(string input, CancellationToken cancellationToken)
        {
            _statusManager.Status = "Cleared workspace.";
            return Task.FromResult(string.Empty);
        }
    }
}
