using cogdeck.Configuration;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace cogdeck.Handlers
{
    internal class TextToSpeechHandler : IHandler
    {
        public string MenuTitle => $"Text-to-speech ({_languageManager.Get().Language})";
        private readonly StatusManager _statusManager;
        private readonly LanguageManager _languageManager;
        private readonly AzureCognitiveServicesOptions _options;
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _synthesizingCompleted = new SemaphoreSlim(1, 1);
        
        public TextToSpeechHandler(
            ILogger<TextToSpeechHandler> logger,
            IOptions<AzureCognitiveServicesOptions> options,
            StatusManager statusManager,
            LanguageManager languageManager)
        {
            _logger = logger;
            _statusManager = statusManager;
            _options = options.Value;
            _languageManager = languageManager;
        }

        public async Task<string> Execute(string input,  CancellationToken cancellationToken)
        {
            SpeechConfig speechConfig = SpeechConfig.FromSubscription(_options.Key, _options.Region);
            speechConfig.SpeechSynthesisVoiceName = _languageManager.Get().Voice;
            SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer(speechConfig);
            speechSynthesizer.SynthesisCompleted += (sender, e) => _synthesizingCompleted.Release();
            speechSynthesizer.SynthesisStarted += (sender, e) => _synthesizingCompleted.Wait();

            if (string.IsNullOrWhiteSpace(input))
            {
                _statusManager.Status = "Nothing to speak.";
            }
            else
            {
                await speechSynthesizer.StartSpeakingTextAsync(input);
                
                _statusManager.Status = "Press button to stop speaking...";
                // Wait for either a key to be pressed or the synthesizing to complete normally.
                int stopReason = Task.WaitAny(
                    Task.Run(() => Console.ReadKey()), 
                    _synthesizingCompleted.WaitAsync());

                if (stopReason == 0)
                {
                    await speechSynthesizer.StopSpeakingAsync();
                    _statusManager.Status = "Speaking stopped.";
                }
                else
                {
                    _statusManager.Status = "Speaking completed.";
                }
            }
            
            return input;
        }
    }
}
