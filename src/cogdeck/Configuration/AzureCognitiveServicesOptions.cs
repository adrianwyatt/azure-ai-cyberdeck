﻿namespace cogdeck.Configuration
{
    /// <summary>
    /// Configuration options class for interacting with Azure Cognitive Services.
    /// </summary>
    public class AzureCognitiveServicesOptions
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
        /// Default language for speech recognition (speech-to-text).
        /// </summary>
        public string SpeechRecognitionLanguage { get; set; }

        /// <summary>
        /// Name of the voice to use for speaking (text-to-speech).
        /// </summary>
        /// <remarks>
        /// https://learn.microsoft.com/en-us/azure/cognitive-services/speech-service/language-support?tabs=stt-tts#text-to-speech
        /// </remarks>
        public string SpeechSynthesisVoiceName { get; set; }

        public string Endpoint { get; set; }

        public string TranslateToLanguage { get; set; }

        /// <summary>
        /// Validate options, throw an exception is any are invalid.
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Region))
                throw new ArgumentException("Argument is invalid.", nameof(Region));

            if (string.IsNullOrWhiteSpace(Key))
                throw new ArgumentException("Argument is invalid.", nameof(Key));

            if (string.IsNullOrWhiteSpace(SpeechRecognitionLanguage))
                throw new ArgumentException("Argument is invalid.", nameof(SpeechRecognitionLanguage));

            if (string.IsNullOrWhiteSpace(SpeechSynthesisVoiceName))
                throw new ArgumentException("Argument is invalid.", nameof(SpeechSynthesisVoiceName));

            if (string.IsNullOrWhiteSpace(Endpoint))
                throw new ArgumentException("Argument is invalid.", nameof(Endpoint));

            if (string.IsNullOrWhiteSpace(TranslateToLanguage))
                throw new ArgumentException("Argument is invalid.", nameof(TranslateToLanguage));
        }
    }
}