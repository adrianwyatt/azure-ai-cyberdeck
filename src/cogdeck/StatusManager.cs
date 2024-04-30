namespace cogdeck
{
    /// <summary>
    /// Manages the status line at the bottom of the console window.
    /// </summary>
    internal class StatusManager
    {
        /// <summary>
        /// The current status message.
        /// </summary>
        private string _status = string.Empty;

        /// <summary>
        /// Gets or sets the current status message.
        /// </summary>
        public string Status
        {
            get { return _status; }
            set
            {
                // Clear the current status message
                ClearStatus();
                // Set the new status message
                _status = value;
                // Render the new status message
                RenderStatus();
            }
        }

        /// <summary>
        /// Renders the status message to the console.
        /// </summary>
        public void RenderStatus()
        {
            Console.SetCursorPosition(0, Console.BufferHeight - 1);
            Console.Write(_status);
        }

        /// <summary>
        /// Clears the status message from the console.
        /// </summary>
        private void ClearStatus()
        {
            Console.SetCursorPosition(0, Console.BufferHeight - 1);
            for (int i = 0; i < _status.Length; i++)
            {
                Console.Write(' ');
            }
        }
    }
}
