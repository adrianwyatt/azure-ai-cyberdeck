using cogdeck.Configuration;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace cogdeck.Handlers
{
    /// <summary>
    /// Handles the "Speech-to-Text" command.
    /// </summary>
    internal class SpeechToTextHandler : IHandler
    {
        public string MenuTitle => "Speech-to-Text";
        private readonly StatusManager _statusManager;
        private readonly AzureAiServicesOptions _options;
        private readonly AudioConfig _audioConfig;
        private readonly LanguageManager _languageManager;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpeechToTextHandler"/> class.
        /// </summary>
        public SpeechToTextHandler(
            ILogger<SpeechToTextHandler> logger,
            IOptions<AzureAiServicesOptions> options,
            LanguageManager languageManager,
            StatusManager statusManager)
        {
            _logger = logger;
            _statusManager = statusManager;
            _languageManager = languageManager;
            _options = options.Value;
            
            // Create an audio configuration
            _audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        }

        /// <summary>
        /// Transcribes the speech to input and updates the status.
        /// </summary>
        public async Task<string> Execute(string input, CancellationToken cancellationToken)
        {
            // Create a speech recognizer for the current language.
            SpeechConfig speechConfig = SpeechConfig.FromSubscription(_options.Key, _options.Region);
            speechConfig.SpeechRecognitionLanguage = _languageManager.Get().Language;
            speechConfig.SetProperty(PropertyId.SpeechServiceResponse_PostProcessingOption, "2");
            SpeechRecognizer speechRecognizer = new SpeechRecognizer(speechConfig, _audioConfig);

            string? recognizedText = null;
            _statusManager.Status = "Started listening...";
            SpeechRecognitionResult result = await speechRecognizer.RecognizeOnceAsync();
            switch (result.Reason)
            {
                case ResultReason.RecognizedSpeech:
                    _logger.LogInformation($"Recognized: {result.Text}");
                    recognizedText = result.Text;
                    break;
                case ResultReason.Canceled:
                    _logger.LogInformation($"Speech recognizer session canceled.");
                    break;
            }

            _statusManager.Status = "Stopped listening.";

            // If the recognized text is empty, return the original input
            if (string.IsNullOrEmpty(recognizedText))
            {
                return input;
            }

            // If the input is empty, return the recognized text
            if (string.IsNullOrEmpty(input))
            {
                return recognizedText;
            }

            // Otherwise, append the recognized text to the input
            return string.Join(Environment.NewLine, input, recognizedText);

        }
    }
}
