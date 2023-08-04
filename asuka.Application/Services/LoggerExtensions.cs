using System;
using System.IO;
using System.Reflection;
using asuka.Application.Output.ConsoleWriter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace asuka.Application.Services;

public static class LoggerExtensions
{
    public static void InstallLogger(this IServiceCollection services)
    {
        var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly()!.Location);
        if (assemblyPath is null)
        {
            Console.Write("WARN: Logging is disabled because one of the requirements is not satisfied.");
            return;
        }

        var logDirectoryRoot = Path.Combine(assemblyPath, "logs");
        Directory.CreateDirectory(logDirectoryRoot);
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(Path.Combine(logDirectoryRoot, $"log-{DateTime.Now:yyyy-M-d-HH-m-ss}.log"))
            .CreateLogger();

        services.AddLogging(sc =>
        {
            sc.ClearProviders();
            sc.AddSerilog(dispose: true);
        });
        services.AddSingleton<IConsoleWriter, ConsoleWriter>();
    }
}
