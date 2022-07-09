using System;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Refit;
using asuka.Api;
using asuka.CommandParsers;
using asuka.Compression;
using asuka.Configuration;
using asuka.Downloader;
using asuka.Output;
using asuka.Services;
using ConfigurationManager = asuka.Configuration.ConfigurationManager;

namespace asuka;

internal class Program
{
    internal static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var services = new ServiceCollection();
        ConfigureServices(services, configuration);
        var serviceProvider = services.BuildServiceProvider();

        var app = serviceProvider.GetRequiredService<AsukaApplication>();
        await app.RunAsync(args, configuration);
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<AsukaApplication>();
        services.AddSingleton<IGalleryRequestService, GalleryRequestService>();
        services.AddSingleton<IConsoleWriter, ConsoleWriter>();
        services.AddSingleton<IGetCommandService, GetCommandService>();
        services.AddSingleton<IDownloadService, DownloadService>();
        services.AddSingleton<IRecommendCommandService, RecommendCommandService>();
        services.AddSingleton<ISearchCommandService, SearchCommandService>();
        services.AddSingleton<IRandomCommandService, RandomCommandService>();
        services.AddSingleton<IFileCommandService, FileCommandService>();
        services.AddSingleton<IPackArchiveToCbz, PackArchiveToCbz>();
        services.AddSingleton<IConfigurationManager, ConfigurationManager>();
        services.AddSingleton<IConfigureCommand, ConfigureCommand>();
        services.AddValidatorsFromAssemblyContaining<Program>();

        ConfigureRefit(services, configuration);
    }

    private static void ConfigureRefit(IServiceCollection services, IConfiguration configuration)
    {
        var cookies = new CloudflareBypass();

        if (cookies.CloudflareClearance == null || cookies.CsrfToken == null)
        {
            Colorful.Console.WriteLine("WARNING: Cookies are unset! Your request might fail. See README for more information.",
                Color.Red);
        }
        
        var configureRefit = new RefitSettings
        {
            ContentSerializer = new NewtonsoftJsonContentSerializer(),
            HttpMessageHandlerFactory = () =>
            {
                var handler = new HttpClientHandler();

                if (cookies.CloudflareClearance != null)
                {
                    handler.CookieContainer.Add(cookies.CloudflareClearance);
                }

                if (cookies.CsrfToken != null)
                {
                    handler.CookieContainer.Add(cookies.CsrfToken);
                }

                return handler;
            }
        };
        services.AddRefitClient<IGalleryApi>(configureRefit)
            .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryForeverAsync(
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (_, i, timeSpan) =>
                {
                    Colorful.Console.WriteLine($"Attempting in {timeSpan.Seconds}s... (Attempt: {i})", Color.Yellow);
                }))
            .ConfigureHttpClient(httpClient =>
            {
                httpClient.BaseAddress = new Uri(configuration["ApiBaseAddress"]);
                httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(cookies.UserAgent);
            });
        services.AddRefitClient<IGalleryImage>()
            .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryForeverAsync(
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (_, _, timeSpan) =>
                {
                    Colorful.Console.WriteLine($"Download will retry in {timeSpan.Seconds}s...", Color.Yellow);
                }))
            .ConfigureHttpClient(httpClient =>
            {
                httpClient.BaseAddress = new Uri(configuration["ImageBaseAddress"]);
            });
    }
}
