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
        public required string Region { get; set; }

        /// <summary>
        /// Access Key
        /// </summary>
        public required string Key { get; set; }

        /// <summary>
        /// Service endpoint
        /// </summary>
        public required string Endpoint { get; set; }
    }
}