namespace cogdeck.Configuration
{
    /// <summary>
    /// Configuration options class for interacting with Azure AI Services.
    /// </summary>
    public record AzureAiServicesOptions
    {
        /// <summary>
        /// Location/region (e.g. WestUS3)
        /// </summary>
        public required string Region { get; init; }

        /// <summary>
        /// Access Key
        /// </summary>
        public required string Key { get; init; }

        /// <summary>
        /// Azure AI services endpoint.
        /// </summary>
        public required string Endpoint { get; init; }
    }
}