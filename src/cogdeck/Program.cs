using cogdeck;
using cogdeck.Configuration;
using cogdeck.Handlers;
using cogdeck.HostedServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;

IHostBuilder builder = Host.CreateDefaultBuilder(args);

builder.ConfigureLogging((context, builder) =>
{
    builder.ClearProviders();
    builder.AddDebug();
});

string configurationFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "configuration.json");

builder.ConfigureAppConfiguration((builder) => builder
    .AddJsonFile(configurationFilePath)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>());

builder.ConfigureServices((context, services) =>
{
    // Setup configuration options
    IConfiguration configurationRoot = context.Configuration;
    services.Configure<AzureCognitiveServicesOptions>(configurationRoot.GetSection("AzureCognitiveServices"));

    services.AddSingleton<StatusManager>();
    services.AddSingleton<LanguageManager>();
    
    services.AddSingleton<IHandler, SpeechToTextHandler>();
    services.AddSingleton<IHandler, TextToSpeechHandler>();
    services.AddSingleton<IHandler, TranslatorHandler>();
    services.AddSingleton<IHandler, SentimentHandler>();
    services.AddSingleton<IHandler, OcrHandler>();
    //services.AddSingleton<IHandler, RandomHandler>();
    //services.AddSingleton<IHandler, LanguageHandler>();
    services.AddSingleton<IHandler, ClearHandler>();

    // Add the primary hosted service to start the loop.
    services.AddHostedService<ScreenRenderService>();
});

IHost host = builder.Build();
await host.RunAsync();
