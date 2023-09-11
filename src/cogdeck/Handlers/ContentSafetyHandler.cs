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

            _statusManager.Status = $"Hate: {response.Value.HateResult?.Severity ?? 0}, " + 
                $"SelfHarm: {response.Value.SelfHarmResult?.Severity ?? 0}, " +
                $"Sexual: {response.Value.SexualResult?.Severity ?? 0}, " +
                $"Violence: {response.Value.ViolenceResult?.Severity ?? 0}"; 
            return input;
        }
    }
}
