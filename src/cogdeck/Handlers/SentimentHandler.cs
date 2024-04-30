using Azure;
using Azure.AI.TextAnalytics;
using cogdeck.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace cogdeck.Handlers
{
    /// <summary>
    /// Handles the "Sentiment" command.
    /// </summary>
    internal class SentimentHandler : IHandler
    {
        public string MenuTitle => "Sentiment";
        private readonly StatusManager _statusManager;
        private readonly AzureAiServicesOptions _options;

        private readonly TextAnalyticsClient _textAnalyticsClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SentimentHandler"/> class.
        /// </summary>
        public SentimentHandler(
            IOptions<AzureAiServicesOptions> options,
            StatusManager statusManager)
        {
            _statusManager = statusManager;
            _options = options.Value;

            _textAnalyticsClient = new TextAnalyticsClient(new Uri(_options.Endpoint), new AzureKeyCredential(_options.Key));
        }

        /// <summary>
        /// Analyzes the sentiment of the input and updates the status.
        /// </summary>
        public async Task<string> Execute(string input, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                _statusManager.Status = "Nothing to analyze sentiment.";
                return input;
            }

            _statusManager.Status = "Analyzing sentiment...";

            // Analyze the sentiment of the input
            Response<DocumentSentiment> response = await _textAnalyticsClient.AnalyzeSentimentAsync(
                document: input,
                //language:"en", // TODO
                options: new AnalyzeSentimentOptions { IncludeOpinionMining = false },
                cancellationToken: cancellationToken);

            // Update the status with the sentiment
            _statusManager.Status = $"Sentimate is {response.Value.Sentiment}.";
            return input;
        }
    }
}
