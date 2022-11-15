using cogdeck.Configuration;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace cogdeck.Handlers
{
    internal class SpeechToTextHandler : IHandler
    {
        public string MenuTitle => "Speech-to-Text";
        private readonly StatusManager _statusManager;
        private readonly AzureCognitiveServicesOptions _options;
        private readonly AudioConfig _audioConfig;
        private readonly SpeechRecognizer _speechRecognizer;
        private readonly ILogger _logger;

        public SpeechToTextHandler(
            ILogger<SpeechToTextHandler> logger,
            IOptions<AzureCognitiveServicesOptions> options,
            StatusManager statusManager)
        {
            _logger = logger;
            _statusManager = statusManager;
            _options = options.Value;

            SpeechConfig speechConfig = SpeechConfig.FromSubscription(_options.Key, _options.Region);
            speechConfig.SpeechRecognitionLanguage = _options.SpeechRecognitionLanguage;

            _audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            _speechRecognizer = new SpeechRecognizer(speechConfig, _audioConfig);

            speechConfig.SetProperty(PropertyId.SpeechServiceResponse_PostProcessingOption, "2");
        }

        public async Task<string> Execute(string input,  CancellationToken cancellationToken)
        {
            string recognizedText = null;
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

            if (string.IsNullOrEmpty(recognizedText))
            {
                return input;
            }

            if (string.IsNullOrEmpty(input))
            {
                return recognizedText;
            }

            return string.Join(Environment.NewLine, input, recognizedText);

        }
    }
}
