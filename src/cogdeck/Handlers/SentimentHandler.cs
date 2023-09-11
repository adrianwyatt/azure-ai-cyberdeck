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
            ILogger<SentimentHandler> logger,
            IOptions<AzureCognitiveServicesOptions> options,
            StatusManager statusManager)
        {
            _logger = logger;
            _statusManager = statusManager;
            _options = options.Value;

            _textAnalyticsClient = new TextAnalyticsClient(new Uri(_options.Endpoint), new AzureKeyCredential(_options.Key));
        }

        public async Task<string> Execute(string input,  CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                _statusManager.Status = "Nothing to analyze sentiment.";
                return input;
            }

            _statusManager.Status = "Analyzing sentiment...";
            
            Response<DocumentSentiment> response = await _textAnalyticsClient.AnalyzeSentimentAsync(
                document:input,
                //language:"en", // TODO
                options:new AnalyzeSentimentOptions {  IncludeOpinionMining = false },                
                cancellationToken:cancellationToken);

            _statusManager.Status = $"Sentimate is {response.Value.Sentiment}.";
            return input;
        }
    }
}
