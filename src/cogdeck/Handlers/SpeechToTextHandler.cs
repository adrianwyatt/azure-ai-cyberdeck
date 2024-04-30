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
        private readonly AzureCognitiveServicesOptions _options;
        private readonly AudioConfig _audioConfig;
        private readonly SpeechRecognizer _speechRecognizer;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpeechToTextHandler"/> class.
        /// </summary>
        public SpeechToTextHandler(
            ILogger<SpeechToTextHandler> logger,
            IOptions<AzureCognitiveServicesOptions> options,
            StatusManager statusManager)
        {
            _logger = logger;
            _statusManager = statusManager;
            _options = options.Value;

            // Create a speech recognizer
            SpeechConfig speechConfig = SpeechConfig.FromSubscription(_options.Key, _options.Region);
            speechConfig.SpeechRecognitionLanguage = _options.SpeechRecognitionLanguage;

            // Create an audio configuration
            _audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            _speechRecognizer = new SpeechRecognizer(speechConfig, _audioConfig);

            // Set the post processing option
            speechConfig.SetProperty(PropertyId.SpeechServiceResponse_PostProcessingOption, "2");
        }

        /// <summary>
        /// Transcribes the speech to input and updates the status.
        /// </summary>
        public async Task<string> Execute(string input, CancellationToken cancellationToken)
        {
            string? recognizedText = null;
            _statusManager.Status = "Started listening...";
            SpeechRecognitionResult result = await _speechRecognizer.RecognizeOnceAsync();
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
