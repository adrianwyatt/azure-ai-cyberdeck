using Azure;
using Azure.AI.TextAnalytics;
using cogdeck.Configuration;
using Microsoft.Extensions.Options;

namespace cogdeck.Handlers
{
    /// <summary>
    /// Handles the "Summarize" command.
    /// </summary>
    internal class SummarizeHandler : IHandler
    {
        public string MenuTitle => "Summarize";
        private readonly StatusManager _statusManager;
        private readonly AzureAiServicesOptions _options;

        private readonly TextAnalyticsClient _textAnalyticsClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SummarizeHandler"/> class.
        /// </summary>
        public SummarizeHandler(
            IOptions<AzureAiServicesOptions> options,
            StatusManager statusManager)
        {
            _statusManager = statusManager;
            _options = options.Value;

            // Create a new TextAnalyticsClient
            _textAnalyticsClient = new TextAnalyticsClient(new Uri(_options.Endpoint), new AzureKeyCredential(_options.Key));
        }

        /// <summary>
        /// Summarizes the input.
        /// </summary>
        public async Task<string> Execute(string input, CancellationToken cancellationToken)
        {
            // If the input is empty, update the status and return the input
            if (string.IsNullOrWhiteSpace(input))
            {
                _statusManager.Status = "Nothing to summarize.";
                return input;
            }

            _statusManager.Status = "Summarizing...";

            // Summarize the input
            AbstractiveSummarizeOperation operation = await _textAnalyticsClient.AbstractiveSummarizeAsync(
                waitUntil: WaitUntil.Completed,
                documents: new List<string> { input },
                options: new AbstractiveSummarizeOptions() { SentenceCount = 3 },
                //language: "en", // TODO
                cancellationToken: cancellationToken);

            // Iterate over the results and join them together.
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
