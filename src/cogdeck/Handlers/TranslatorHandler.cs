﻿using cogdeck.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace cogdeck.Handlers
{
    internal class TranslatorHandler : IHandler
    {
        public string MenuTitle => "Translate";
        private readonly StatusManager _statusManager;
        private readonly AzureCognitiveServicesOptions _options;
        private readonly ILogger _logger;

        private static readonly string _translatorGlobalEndpoint = "https://api.cognitive.microsofttranslator.com";
        

        public TranslatorHandler(
            ILogger<SpeechToTextHandler> logger,
            IOptions<AzureCognitiveServicesOptions> options,
            StatusManager statusManager)
        {
            _logger = logger;
            _statusManager = statusManager;
            _options = options.Value;

        }

        public async Task<string> Execute(string input, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(input))
            {
                _statusManager.Status = "Nothing to translate.";
                return input;
            }
            
            _statusManager.Status = "Translating...";

            // Input and output languages are defined as parameters.
            string route = $"/translate?api-version=3.0&from=en&to={_options.TranslateToLanguage}";
            object[] body = new object[] { new { Text = input } };
            string requestBody = JsonConvert.SerializeObject(body);

            using HttpClient client = new HttpClient();
            using HttpRequestMessage request = new HttpRequestMessage();
            
            // Build the request.
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(_translatorGlobalEndpoint + route);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            request.Headers.Add("Ocp-Apim-Subscription-Key", _options.Key);
            request.Headers.Add("Ocp-Apim-Subscription-Region", _options.Region);

            // Send the request and get response.
            HttpResponseMessage response = await client.SendAsync(request);
            
            // Read response as a string.
            string result = await response.Content.ReadAsStringAsync();
            TranslatorResponse[] translationResponses = JsonConvert.DeserializeObject<TranslatorResponse[]>(result);
            
            _statusManager.Status = "Translation complete.";
            return translationResponses[0].translations[0].text;
        }
    }

    internal class TranslatorResponse
    {
        public Translation[] translations { get; set; }
    }

    internal class Translation
    {
        public string text { get; set; }
        public string to { get; set; }
       }
}
