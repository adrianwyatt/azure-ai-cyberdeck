using cogdeck.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace cogdeck.Handlers
{
    internal class OcrHandler : IHandler
    {
        public string MenuTitle => "Read Document";
        private readonly StatusManager _statusManager;
        private readonly AzureCognitiveServicesOptions _options;
        private readonly ILogger _logger;

        public OcrHandler(
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
            // Take snapshot
            // libcamera-jpeg -o filepath
            string filepath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "ocr-snaphot.jpg");

            ProcessStartInfo startInfo = new ProcessStartInfo() 
            {
                FileName = "libcamera-jpeg",
                Arguments = $"-o {filepath}"
            };

            Process process = Process.Start(startInfo);

            return input;
        }
    }
}