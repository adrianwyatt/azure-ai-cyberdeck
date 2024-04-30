using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using cogdeck.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace cogdeck.Handlers
{
    /// <summary>
    /// Handles the "Camera Capture (OCR)" command.
    /// </summary>
    internal class OcrHandler : IHandler
    {
        public string MenuTitle => "Camera Capture (OCR)";
        private readonly StatusManager _statusManager;
        private readonly AzureAiServicesOptions _options;
        private readonly ILogger _logger;

        private readonly DocumentAnalysisClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="OcrHandler"/> class.
        /// </summary>
        public OcrHandler(
            ILogger<OcrHandler> logger,
            IOptions<AzureAiServicesOptions> options,
            StatusManager statusManager)
        {
            _logger = logger;
            _statusManager = statusManager;
            _options = options.Value;

            _client = new DocumentAnalysisClient(new Uri(_options.Endpoint), new AzureKeyCredential(_options.Key));
        }

        /// <summary>
        /// Captures an image and analyzes it with optical character recogniztion (OCR).
        /// </summary>
        public async Task<string> Execute(string input, CancellationToken cancellationToken)
        {
            string assemblyDirectory = Ensure.NotNullOrEmpty(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            string filePath = Path.Combine(assemblyDirectory, "ocr-snaphot.jpg");

            _statusManager.Status = "Capturing image...";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // TODO: Use a common image capture library for Windows, until then - use a pre-captured image.
                filePath = Path.Combine(assemblyDirectory, "Handlers", "ocr_test_image.jpg");
                if (!File.Exists(filePath))
                {
                    _statusManager.Status = "No image found.";
                    return input;
                }
            }
            else
            {
                CaptureCameraStillRPi(filePath);
            }

            // Analyze the URL image 
            _statusManager.Status = "Anaylzing image...";
            AnalyzeDocumentOperation operation = await _client.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                "prebuilt-read",
                File.OpenRead(filePath));

            // Get the result
            _statusManager.Status = "Analysis complete.";
            AnalyzeResult result = operation.Value;
            input = input += result.Content;

            Console.Clear();

            return input;
        }

        /// <summary>
        /// Captures a still image from the camera on a Raspberry Pi or compatible linux-based device.
        /// </summary>
        /// <param name="filepath"></param>
        private static void CaptureCameraStillRPi(string filepath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "libcamera-jpeg",
                Arguments = $"-o {filepath}"
            };

            Process? process = Process.Start(startInfo);
            process?.WaitForExit();
        }
    }
}