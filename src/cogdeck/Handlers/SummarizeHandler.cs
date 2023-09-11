using System.Reflection.Metadata.Ecma335;
using Azure;
using Azure.AI.TextAnalytics;
using cogdeck.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace cogdeck.Handlers
{
    internal class SummarizeHandler : IHandler
    {
        public string MenuTitle => "Summarize";
        private readonly StatusManager _statusManager;
        private readonly AzureCognitiveServicesOptions _options;
        private readonly ILogger _logger;

        private readonly TextAnalyticsClient _textAnalyticsClient;

        public SummarizeHandler(
            ILogger<SummarizeHandler> logger,
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
                _statusManager.Status = "Nothing to summarize.";
                return input;
            }

            _statusManager.Status = "Summarizing...";

            AbstractiveSummarizeOperation operation = await _textAnalyticsClient.AbstractiveSummarizeAsync(
                waitUntil: WaitUntil.Completed,
                documents: new List<string> { input },
                options: new AbstractiveSummarizeOptions() { SentenceCount = 3 },
                //language: "en", // TODO
                cancellationToken: cancellationToken);

            string summary = string.Empty;
            await foreach (AbstractiveSummarizeResultCollection resultCollection in operation.Value)
            {
                foreach (AbstractiveSummarizeResult result in resultCollection)
                {
                    summary = string.Join(" ", result.Summaries.Select(s => s.Text));
                }
            }

            _statusManager.Status = "Summarization complete.";
            return summary;
        }
    }
}
