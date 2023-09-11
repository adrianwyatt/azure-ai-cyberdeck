namespace cogdeck.Configuration
{
    /// <summary>
    /// Configuration options class for interacting with Azure Content Safety.
    /// </summary>
    public class AzureContentSafetyOptions
    {
        /// <summary>
        /// Location/region (e.g. WestUS3)
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Access Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Service endpoint
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Validate options, throw an exception is any are invalid.
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Region))
                throw new ArgumentException("Argument is invalid.", nameof(Region));

            if (string.IsNullOrWhiteSpace(Key))
                throw new ArgumentException("Argument is invalid.", nameof(Key));

            if (string.IsNullOrWhiteSpace(Endpoint))
                throw new ArgumentException("Argument is invalid.", nameof(Endpoint));
        }
    }
}