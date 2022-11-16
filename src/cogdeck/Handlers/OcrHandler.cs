﻿using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using cogdeck.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Reflection;

namespace cogdeck.Handlers
{
    internal class OcrHandler : IHandler
    {
        public string MenuTitle => "Read from Camera (OCR)";
        private readonly StatusManager _statusManager;
        private readonly AzureCognitiveServicesOptions _options;
        private readonly ILogger _logger;

        private readonly DocumentAnalysisClient _client;

        public OcrHandler(
            ILogger<SpeechToTextHandler> logger,
            IOptions<AzureCognitiveServicesOptions> options,
            StatusManager statusManager)
        {
            _logger = logger;
            _statusManager = statusManager;
            _options = options.Value;

            _client = new DocumentAnalysisClient(new Uri(_options.Endpoint), new AzureKeyCredential(_options.Key));
        }

        public async Task<string> Execute(string input, CancellationToken cancellationToken)
        {
            // Take snapshot
            // libcamera-jpeg -o filepath
            string filePath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "ocr-snaphot.jpg");

            // TODO windows debug
            //filePath = @"C:\Users\adribona\OneDrive - Microsoft\Pictures\Camera Roll\WIN_20221115_15_51_12_Pro.jpg";
            _statusManager.Status = "Capturing image...";
            CaptureCameraStill(filePath);

            // Analyze the URL image 
            _statusManager.Status = "Anaylzing image...";
            AnalyzeDocumentOperation operation = await _client.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                "prebuilt-read",
                File.OpenRead(filePath));

            _statusManager.Status = "Anaylzing completed.";
            AnalyzeResult result = operation.Value;
            input = input += result.Content;

            Console.Clear();

            return input;
        }

        private static void CaptureCameraStill(string filepath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "libcamera-jpeg",
                Arguments = $"-o {filepath}"
            };

            Process process = Process.Start(startInfo);
            process.WaitForExit();
        }
    }
}