using System;
using System.IO;
using System.Linq;
using System.Reflection;
using asuka.Application;
using asuka.Application.Commandline;
using asuka.Application.Commandline.Parsers;
using asuka.Application.Utilities;
using asuka.Core.Chaptering;
using asuka.Core.Compression;
using asuka.Core.Configuration;
using asuka.Core.Downloader;
using asuka.Core.Output.Progress;
using asuka.Core.Requests;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();

// Install loggers
var asukaLogger = LoggerFactory.Create(logger =>
{
    logger.ClearProviders();
    logger.AddConsole();
}).CreateLogger("asuka.Application");
services.AddSingleton(asukaLogger);

services.AddSingleton<IProgressService, asuka.Application.Output.ProgressService.ProgressService>();
services.AddScoped<IDownloader, Downloader>();
services.AddScoped<IPackArchiveToCbz, PackArchiveToCbz>();
services.AddSingleton<IConfigurationManager, ConfigurationManager>();
services.AddTransient<ISeriesFactory, SeriesFactory>();

// Inject all DLLs that matches the Request Requirements
var assemblyDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
var dlls = Directory.GetFiles(assemblyDir, "asuka.Providers.*.dll");

foreach (var dll in dlls)
{
    Type[] types;
    try
    {
        types = Assembly.LoadFrom(dll).GetTypes();
    }
    catch
    {
        continue;
    }
    
    // Load all classes inheriting IGalleryImageRequestService and
    // IGalleryRequestService
    var allImageRequestServices = ActivateTypes
        .GetAllMatchingTypes<IGalleryImageRequestService>(types
            .Where(t => t.IsClass &&
                        t.GetInterfaces().Contains(typeof(IGalleryImageRequestService))));
    var allRequestServices = ActivateTypes
        .GetAllMatchingTypes<IGalleryRequestService>(types
            .Where(t => t.IsClass &&
                        t.GetInterfaces().Contains(typeof(IGalleryRequestService))));
    
    // Inject them to the DI
    foreach (var installableImageRequestService in allImageRequestServices)
    {
        services.AddSingleton(installableImageRequestService);
    }
    
    foreach (var installableRequestService in allRequestServices)
    {
        services.AddSingleton(installableRequestService);
    }
}

services.AddSingleton<AsukaApplication>();

// Command parsers
services.AddScoped<ICommandLineParser, GetCommandService>();
services.AddScoped<ICommandLineParser, RecommendCommandService>();
services.AddScoped<ICommandLineParser, SearchCommandService>();
services.AddScoped<ICommandLineParser, RandomCommandService>();
services.AddScoped<ICommandLineParser, FileCommandService>();
services.AddScoped<ICommandLineParser, ConfigureCommand>();
services.AddScoped<ICommandLineParser, SeriesCreatorCommandService>();
services.AddScoped<ICommandLineParserFactory, CommandLineParserFactory>();

// Validators
services.AddValidatorsFromAssemblyContaining<Program>();

var serviceProvider = services.BuildServiceProvider();

var app = serviceProvider.GetRequiredService<AsukaApplication>();

// Count all providers available
var imageRequestServices = serviceProvider.GetServices<IGalleryImageRequestService>();
var apiRequestServices = serviceProvider.GetServices<IGalleryRequestService>();

if (!imageRequestServices.Any() || !apiRequestServices.Any())
{
    Console.WriteLine("No provider services found. Exiting...");
    return;
}

await app.RunAsync(args);
