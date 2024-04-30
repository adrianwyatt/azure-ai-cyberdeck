using System.Text;
using cogdeck.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace cogdeck.Handlers
{
    /// <summary>
    /// Handles the "Translate" command.
    /// </summary>
    internal class TranslatorHandler : IHandler
    {
        /// <summary>
        /// The global endpoint for the Translator service.
        /// </summary>
        private const string _translatorGlobalEndpoint = "https://api.cognitive.microsofttranslator.com";

        public string MenuTitle => $"Translate ({_languageManager.Get().Language}->{_languageManager.PeekNext().Language})";
        private readonly StatusManager _statusManager;
        private readonly AzureAiServicesOptions _options;

        private readonly LanguageManager _languageManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslatorHandler"/> class.
        /// </summary>
        public TranslatorHandler(
            IOptions<AzureAiServicesOptions> options,
            StatusManager statusManager,
            LanguageManager languageManager)
        {
            _statusManager = statusManager;
            _options = options.Value;
            _languageManager = languageManager;
        }

        /// <summary>
        /// Translates the input
        /// </summary>
        public async Task<string> Execute(string input, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(input))
            {
                _statusManager.Status = "Nothing to translate.";
                return input;
            }

            _statusManager.Status = "Translating...";

            // Input and output languages are defined as parameters.
            string route = $"/translate?api-version=3.0&from={_languageManager.Get().Language}&to={_languageManager.PeekNext().Language}";
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
            TranslatorResponse[]? translationResponses = JsonConvert.DeserializeObject<TranslatorResponse[]>(result);

            // If the translation failed, return the original input, otherwise return the translated text and cycle the language.
            string? translatedText = translationResponses?[0]?.translations?[0].text;
            if (string.IsNullOrEmpty(translatedText))
            {
                _statusManager.Status = "Translation failed.";
                return input;
            }
            else
            {
                _statusManager.Status = "Translation complete.";
                _languageManager.Cycle();
                return translatedText;
            }
        }

        /// <summary>
        /// Represents the response from the Translator service.
        /// </summary>
        private record TranslatorResponse
        {
            /// <summary>
            /// Gets or sets the translations.
            /// </summary>
            public Translation[]? translations { get; set; }
        }

        /// <summary>
        /// Represents a translation.
        /// </summary>
        private record Translation
        {
            /// <summary>
            /// Gets or sets the translated text.
            /// </summary>
            public string? text { get; set; }

            /// <summary>
            /// Gets or sets the language.
            /// </summary>
            public string? to { get; set; }
        }
    }
}
