using System;
using System.IO;
using asuka.Application.Commands;
using asuka.Application.Services.Downloader;
using asuka.Application.Services.ProviderManager;
using Cocona;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

var builder = CoconaApp.CreateBuilder();

// Logging stuff for diagnostics
var logRoot = Path.Combine(AppContext.BaseDirectory, "logs");
if (!Directory.Exists(logRoot))
{
    Directory.CreateDirectory(logRoot);
}

Log.Logger = new LoggerConfiguration()
    .WriteTo
    .File(
        Path.Combine(logRoot, "log.txt"),
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}[{Level}][{SourceContext}] {Message}{NewLine}{Exception}")
    .CreateLogger();

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddSerilog(dispose: true);
});

// Services
builder.Services.AddSingleton<IProviderManager, ProviderManager>();
builder.Services.AddSingleton<IDownloaderBuilder, DownloadBuilder>();

var app = builder.Build();

// Commands
app.AddCommands<FileCommand>();
app.AddCommands<GetCommand>();
app.AddCommands<RandomCommand>();
app.AddCommands<RecommendCommand>();
app.AddCommands<SearchCommand>();
app.AddCommands<ProvidersCommand>();
app.AddCommands<InfoCommand>();
app.AddCommands<SeriesCommand>();

await app.RunAsync();
