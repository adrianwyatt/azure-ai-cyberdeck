using Microsoft.Extensions.Logging;

namespace cogdeck.Handlers
{
    internal class LanguageHandler : IHandler
    {
        public string MenuTitle => $"Rotate Language ({_languageManager.Get().Language}->{_languageManager.PeekNext().Language})";
        private readonly LanguageManager _languageManager;
        private readonly ILogger _logger;
        
        public LanguageHandler(
            ILogger<LanguageHandler> logger,
            LanguageManager languageManager)
        {
            _logger = logger;
            _languageManager = languageManager;
        }

        public Task<string> Execute(string input,  CancellationToken cancellationToken)
        {
            _languageManager.Cycle();            
            return Task.FromResult(input);
        }
    }
}
