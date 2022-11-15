using cogdeck.Configuration;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace cogdeck.Handlers
{
    internal class TextToSpeechHandler : IHandler
    {
        public string MenuTitle => "Text-to-Speech";
        private readonly StatusManager _statusManager;
        private readonly AzureCognitiveServicesOptions _options;
        private readonly SpeechSynthesizer _speechSynthesizer;
        private readonly ILogger _logger;

        public TextToSpeechHandler(
            ILogger<SpeechToTextHandler> logger,
            IOptions<AzureCognitiveServicesOptions> options,
            StatusManager statusManager)
        {
            _logger = logger;
            _statusManager = statusManager;
            _options = options.Value;

            SpeechConfig speechConfig = SpeechConfig.FromSubscription(_options.Key, _options.Region);
            speechConfig.SpeechSynthesisVoiceName = _options.SpeechSynthesisVoiceName;
            _speechSynthesizer = new SpeechSynthesizer(speechConfig);
        }

        public async Task<string> Execute(string input,  CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(input))
            {
                _statusManager.Status = "Speaking...";
                await _speechSynthesizer.SpeakTextAsync(input);
                _statusManager.Status = "Done speaking.";
            }
            else
            {
                _statusManager.Status = "Nothing to speak.";
            }
            
            return input;
        }
    }
}
