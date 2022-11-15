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
    //builder.AddSimpleConsole(options =>
    //{
    //    options.SingleLine = false;
    //    options.IncludeScopes = false;
    //    options.TimestampFormat = "hh:mm:ss ";
    //});
});

string configurationFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "configuration.json");

builder.ConfigureAppConfiguration((builder) => builder
    .AddJsonFile(configurationFilePath)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>());

builder.ConfigureServices((context, services) =>
{
    services.AddSingleton<IHandler, EchoHandler>();
    services.AddSingleton<IHandler, Echo2Handler>();

    

    // Add the primary hosted service to start the loop.
    services.AddHostedService<ScreenRenderService>();
});

IHost host = builder.Build();
await host.RunAsync();
