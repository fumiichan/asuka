using System;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Refit;
using asuka.Api;
using asuka.Cloudflare;
using asuka.CommandParsers;
using asuka.Compression;
using asuka.Downloader;
using asuka.Output;
using asuka.Services;

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
        services.AddSingleton<IConfigureCommand, ConfigureCommand>();
        services.AddValidatorsFromAssemblyContaining<Program>();

        ConfigureRefit(services, configuration);
    }

    private static void ConfigureRefit(IServiceCollection services, IConfiguration configuration)
    {
        var cookies = FetchCookiesFromFile.load();
        
        var configureRefit = new RefitSettings
        {
            ContentSerializer = new NewtonsoftJsonContentSerializer(),
            HttpMessageHandlerFactory = () =>
            {
                var handler = new HttpClientHandler();

                if (cookies?.getCloudflareClearance() != null)
                {
                    handler.CookieContainer.Add(cookies.getCloudflareClearance());
                }

                if (cookies?.getCsrfToken() != null)
                {
                    handler.CookieContainer.Add(cookies.getCsrfToken());
                }

                if (cookies?.getSession() != null)
                {
                    handler.CookieContainer.Add(cookies.getSession());
                }
                
                return handler;
            }
        };
        services.AddRefitClient<IGalleryApi>(configureRefit)
            .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryForeverAsync(
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                ((_, i, _) =>
                {
                    Colorful.Console.WriteLine($"Attempting in {i}s...", Color.Yellow);
                })))
            .ConfigureHttpClient(httpClient =>
            {
                httpClient.BaseAddress = new Uri(configuration["ApiBaseAddress"]);
                httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36");
            });
        services.AddRefitClient<IGalleryImage>()
            .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryForeverAsync(
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (_, timeSpan, _) =>
                {
                    Colorful.Console.WriteLine($"Download will retry in {timeSpan}", Color.Yellow);
                }))
            .ConfigureHttpClient(httpClient =>
            {
                httpClient.BaseAddress = new Uri(configuration["ImageBaseAddress"]);
            });
    }
}
