using System.Net;
using Azure;
using Azure.AI.ContentSafety;
using Azure.AI.TextAnalytics;
using cogdeck.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static System.Net.Mime.MediaTypeNames;

namespace cogdeck.Handlers
{
    internal class ContentSafetyHandler : IHandler
    {
        public string MenuTitle => "Content Safety";
        private readonly StatusManager _statusManager;
        private readonly AzureContentSafetyOptions _options;
        private readonly ILogger _logger;

        private readonly ContentSafetyClient _contentSafetyClient;

        public ContentSafetyHandler(
            ILogger<ContentSafetyHandler> logger,
            IOptions<AzureContentSafetyOptions> options,
            StatusManager statusManager)
        {
            _logger = logger;
            _statusManager = statusManager;
            _options = options.Value;

            _contentSafetyClient = new ContentSafetyClient(new Uri(_options.Endpoint), new AzureKeyCredential(_options.Key));
        }

        public async Task<string> Execute(string input,  CancellationToken cancellationToken)
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
