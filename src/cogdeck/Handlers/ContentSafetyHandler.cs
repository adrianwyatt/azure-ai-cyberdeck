using Azure;
using Azure.AI.ContentSafety;
using cogdeck.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace cogdeck.Handlers
{
    /// <summary>
    /// Handles the "Content Safety" command.
    /// </summary>
    internal class ContentSafetyHandler : IHandler
    {
        public string MenuTitle => "Content Safety";
        private readonly StatusManager _statusManager;
        private readonly AzureContentSafetyOptions _options;

        private readonly ContentSafetyClient _contentSafetyClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentSafetyHandler"/> class.
        /// </summary>
        public ContentSafetyHandler(
            IOptions<AzureContentSafetyOptions> options,
            StatusManager statusManager)
        {
            _statusManager = statusManager;
            _options = options.Value;

            _contentSafetyClient = new ContentSafetyClient(new Uri(_options.Endpoint), new AzureKeyCredential(_options.Key));
        }

        /// <summary>
        /// Analyzes the content safety of the input and updates the status.
        /// </summary>
        public async Task<string> Execute(string input, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                _statusManager.Status = "Nothing to analyze content safety.";
                return input;
            }

            _statusManager.Status = "Analyzing safety...";

            Response<AnalyzeTextResult> response = await _contentSafetyClient.AnalyzeTextAsync(new AnalyzeTextOptions(input));

            _statusManager.Status = string.Join(", ", response.Value.CategoriesAnalysis.Select(c => $"{c.Category}: {c.Severity}"));
            return input;
        }
    }
}
