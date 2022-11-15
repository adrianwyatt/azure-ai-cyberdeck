using Azure;
using Azure.AI.TextAnalytics;
using cogdeck.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace cogdeck.Handlers
{
    internal class SentimentHandler : IHandler
    {
        public string MenuTitle => "Sentiment";
        private readonly StatusManager _statusManager;
        private readonly AzureCognitiveServicesOptions _options;
        private readonly ILogger _logger;

        private readonly TextAnalyticsClient _textAnalyticsClient;

        public SentimentHandler(
            ILogger<SpeechToTextHandler> logger,
            IOptions<AzureCognitiveServicesOptions> options,
            StatusManager statusManager)
        {
            _logger = logger;
            _statusManager = statusManager;
            _options = options.Value;

            _textAnalyticsClient = new TextAnalyticsClient(new Uri(_options.Endpoint), new AzureKeyCredential(_options.Key));
        }

        public Task<string> Execute(string input,  CancellationToken cancellationToken)
        {
            _statusManager.Status = "Analyzing sentiment...";
            
            Response<DocumentSentiment> response = _textAnalyticsClient.AnalyzeSentiment(
                document:input,
                cancellationToken:cancellationToken);

            _statusManager.Status = $"Sentimate is {response.Value.Sentiment}.";
            return Task.FromResult(input);
        }
    }
}
