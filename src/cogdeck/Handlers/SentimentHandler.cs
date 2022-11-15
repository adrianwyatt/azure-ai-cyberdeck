using Azure;
using Azure.AI.TextAnalytics;
using cogdeck.Configuration;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Reflection.Metadata;
using System.Xml.Linq;

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

            Uri endpoint = new Uri("https://cogdeck.cognitiveservices.azure.com/");
            AzureKeyCredential keyCredential = new AzureKeyCredential(_options.Key);
            _textAnalyticsClient = new TextAnalyticsClient(endpoint, keyCredential);
        }

        public Task<string> Execute(string input,  CancellationToken cancellationToken)
        {
            _statusManager.Status = "Analyzing sentiment...";
            
            Response<DocumentSentiment> response = _textAnalyticsClient.AnalyzeSentiment(
                document:input,
                cancellationToken:cancellationToken);

            DocumentSentiment docSentiment = response.Value;
            
            _logger.LogInformation($"Sentiment was {docSentiment.Sentiment}, with confidence scores: ");
            _logger.LogInformation($"  Positive confidence score: {docSentiment.ConfidenceScores.Positive}.");
            _logger.LogInformation($"  Neutral confidence score: {docSentiment.ConfidenceScores.Neutral}.");
            _logger.LogInformation($"  Negative confidence score: {docSentiment.ConfidenceScores.Negative}.");

            _statusManager.Status = $"Sentimate is {docSentiment.Sentiment}.";
            return Task.FromResult(input);
        }
    }
}
