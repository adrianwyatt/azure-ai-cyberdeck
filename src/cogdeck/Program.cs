using System.Reflection;
using cogdeck;
using cogdeck.Configuration;
using cogdeck.Handlers;
using cogdeck.HostedServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Create the host builder
IHostBuilder builder = Host.CreateDefaultBuilder(args);

// Configure logging
builder.ConfigureLogging((context, builder) =>
{
    builder.ClearProviders();
    builder.AddDebug();
});

// Construct the configuration file path
string assemblyDirectory = Ensure.NotNullOrEmpty(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
string configurationFilePath = Path.Combine(assemblyDirectory, "configuration.json");

// Set up application configuration
builder.ConfigureAppConfiguration((builder) => builder
    .AddJsonFile(configurationFilePath) // Read the configuration file
    .AddEnvironmentVariables() // Read environment variables
    .AddUserSecrets<Program>()); // Read user secrets from "dotnet user-secrets"

// Configure application services
builder.ConfigureServices((context, services) =>
{
    // Setup configuration options
    IConfiguration configurationRoot = context.Configuration;
    services.Configure<AzureCognitiveServicesOptions>(configurationRoot.GetSection("AzureCognitiveServices"));
    services.Configure<AzureContentSafetyOptions>(configurationRoot.GetSection("ContentSafety"));
    services.Configure<LanguageOptions>(configurationRoot.GetSection("LanguageManager"));

    // Add managers
    services.AddSingleton<StatusManager>();
    services.AddSingleton<LanguageManager>();

    // Add handlers
    services.AddSingleton<IHandler, SpeechToTextHandler>();
    services.AddSingleton<IHandler, TextToSpeechHandler>();
    services.AddSingleton<IHandler, TranslatorHandler>();
    services.AddSingleton<IHandler, SentimentHandler>();
    services.AddSingleton<IHandler, SummarizeHandler>();
    services.AddSingleton<IHandler, ContentSafetyHandler>();
    services.AddSingleton<IHandler, OcrHandler>();
    services.AddSingleton<IHandler, ClearHandler>();

    // Add the primary hosted service to start the render/input loop.
    services.AddHostedService<ScreenRenderService>();
});

// Build and run the host
IHost host = builder.Build();
await host.RunAsync();
