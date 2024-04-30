using cogdeck.Configuration;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Options;

namespace cogdeck.Handlers
{
    /// <summary>
    /// Handles the "Text-to-speech" command.
    /// </summary>
    internal class TextToSpeechHandler : IHandler
    {
        public string MenuTitle => $"Text-to-speech ({_languageManager.Get().Language})";
        private readonly StatusManager _statusManager;
        private readonly LanguageManager _languageManager;
        private readonly AzureCognitiveServicesOptions _options;
        private readonly SemaphoreSlim _synthesizingCompleted = new SemaphoreSlim(1, 1);

        public TextToSpeechHandler(
            IOptions<AzureCognitiveServicesOptions> options,
            StatusManager statusManager,
            LanguageManager languageManager)
        {
            _statusManager = statusManager;
            _options = options.Value;
            _languageManager = languageManager;
        }

        /// <summary>
        /// Synthesizes the input.
        /// </summary>
        public async Task<string> Execute(string input, CancellationToken cancellationToken)
        {
            // Check if there is nothing to speak.
            if (string.IsNullOrWhiteSpace(input))
            {
                _statusManager.Status = "Nothing to speak.";
                return input;
            }

            // Setup the speech config
            SpeechConfig speechConfig = SpeechConfig.FromSubscription(_options.Key, _options.Region);
            speechConfig.SpeechSynthesisVoiceName = _languageManager.Get().Voice;

            // Setup the speech synthesizer
            SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer(speechConfig);
            speechSynthesizer.SynthesisCompleted += (sender, e) => _synthesizingCompleted.Release();
            speechSynthesizer.SynthesisStarted += (sender, e) => _synthesizingCompleted.Wait();

            // Synthesize the input
            await speechSynthesizer.StartSpeakingTextAsync(input);

            _statusManager.Status = "Press button to stop speaking...";

            // Wait for either a key to be pressed or the synthesizing to complete normally.
            // TODO support non-keyboard input
            int stopReason = Task.WaitAny(
                Task.Run(() => Console.ReadKey()),
                _synthesizingCompleted.WaitAsync());

            // Stop speaking if a key was pressed
            if (stopReason == 0)
            {
                await speechSynthesizer.StopSpeakingAsync();
                _statusManager.Status = "Speaking stopped.";
            }
            else
            {
                // Otherwise, the synthesizing completed normally
                _statusManager.Status = "Speaking completed.";
            }

            return input;
        }
    }
}
