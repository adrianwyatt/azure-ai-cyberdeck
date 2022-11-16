using cogdeck.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Net;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Azure.Storage.Blobs;
using Azure.Identity;
using Azure.Storage.Sas;
using Azure.Storage;
using Azure.Storage.Blobs.Models;

namespace cogdeck.Handlers
{
    internal class OcrHandler : IHandler
    {
        public string MenuTitle => "Read Document";
        private readonly StatusManager _statusManager;
        private readonly AzureCognitiveServicesOptions _options;
        private readonly ILogger _logger;

        private readonly ComputerVisionClient _client;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _blobContainerClient;

        public OcrHandler(
            ILogger<SpeechToTextHandler> logger,
            IOptions<AzureCognitiveServicesOptions> options,
            StatusManager statusManager)
        {
            _logger = logger;
            _statusManager = statusManager;
            _options = options.Value;

            _client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(_options.Key))
            {
                Endpoint = _options.Endpoint
            };


            string connectionString = $"DefaultEndpointsProtocol=https;AccountName={_options.StorageAccountName};AccountKey={_options.StorageAccountKey};EndpointSuffix=core.windows.net";
            _blobServiceClient = new BlobServiceClient(connectionString);

            string containerName = Assembly.GetExecutingAssembly().GetName().Name;
            _blobContainerClient = _blobServiceClient.GetBlobContainers().Any(b => b.Name.Equals(containerName, StringComparison.OrdinalIgnoreCase))
                ? _blobServiceClient.GetBlobContainerClient(containerName)
                : _blobServiceClient.CreateBlobContainer(containerName);
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
            CaptureCameraStill(filePath);

            _blobContainerClient.DeleteBlobIfExists("ocr-capture.jpg");
            _blobContainerClient.UploadBlob("ocr-capture.jpg", File.OpenRead(filePath));
                            
            AccountSasBuilder sasBuilder = new AccountSasBuilder()
            {
                Services = AccountSasServices.Blobs | AccountSasServices.Files,
                ResourceTypes = AccountSasResourceTypes.Service,
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1),
                Protocol = SasProtocol.Https
            };

            sasBuilder.SetPermissions(AccountSasPermissions.Read);

            // Use the key to get the SAS token.
            StorageSharedKeyCredential key = new StorageSharedKeyCredential(_options.StorageAccountName, _options.StorageAccountKey);
            string sasToken = sasBuilder.ToSasQueryParameters(key).ToString();


            string imageUrl = $"https://{_options.StorageAccountName}.blob.core.windows.net/cogdeck/ocr-capture.jpg?{sasToken}";

            // Analyze the URL image 
            ImageAnalysis results = await _client.AnalyzeImageAsync(imageUrl) ;


            Console.Clear();

            // TODO

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