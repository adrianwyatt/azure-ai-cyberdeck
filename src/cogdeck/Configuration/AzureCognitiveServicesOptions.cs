namespace cogdeck.Configuration
{
    /// <summary>
    /// Configuration options class for interacting with Azure AI Services.
    /// </summary>
    public record AzureCognitiveServicesOptions
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
        /// Default language for speech recognition (speech-to-text).
        /// </summary>
        public required string SpeechRecognitionLanguage { get; init; }

        /// <summary>
        /// Name of the voice to use for speaking (text-to-speech).
        /// </summary>
        /// <remarks>
        /// https://learn.microsoft.com/azure/cognitive-services/speech-service/language-support?tabs=stt-tts#text-to-speech
        /// </remarks>
        public required string SpeechSynthesisVoiceName { get; init; }

        /// <summary>
        /// Azure AI services endpoint.
        /// </summary>
        public required string Endpoint { get; init; }
    }
}