using System;
using System.Drawing;
using System.Net.Http;
using asuka.Api;
using asuka.CommandParsers;
using asuka.Compression;
using asuka.Configuration;
using asuka.Downloader;
using asuka.Output;
using asuka.Services;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Refit;
using ConfigurationManager = asuka.Configuration.ConfigurationManager;

namespace asuka.Installers;

public static class ConfigureServices
{
    public static void InstallServices(this IServiceCollection services)
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
    }

    public static void InstallRefitServices(this IServiceCollection services, IConfiguration configuration)
    {
        var cookies = new CloudflareBypass();

        var cloudflareClearance = cookies.GetCookieByName("cf_clearance");
        var csrfToken = cookies.GetCookieByName("csrftoken");

        if (cloudflareClearance == null || csrfToken == null)
        {
            Colorful.Console.WriteLine("WARNING: Cookies are unset! Your request might fail.", Color.Red);
        }

        var configureRefit = new RefitSettings
        {
            ContentSerializer = new NewtonsoftJsonContentSerializer(),
            HttpMessageHandlerFactory = () =>
            {
                var handler = new HttpClientHandler();

                if (cloudflareClearance != null)
                {
                    handler.CookieContainer.Add(cloudflareClearance);
                }

                if (csrfToken != null)
                {
                    handler.CookieContainer.Add(csrfToken);
                }

                return handler;
            }
        };
        
        services.AddRefitClient<IGalleryApi>(configureRefit)
            .AddTransientHttpErrorPolicy(ConfigureErrorPolicyBuilder)
            .ConfigureHttpClient(httpClient =>
            {
                httpClient.BaseAddress = new Uri(configuration["ApiBaseAddress"]);
                httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(cookies.UserAgent);
            });
        services.AddRefitClient<IGalleryImage>()
            .AddTransientHttpErrorPolicy(ConfigureErrorPolicyBuilder)
            .ConfigureHttpClient(httpClient =>
            {
                httpClient.BaseAddress = new Uri(configuration["ImageBaseAddress"]);
                httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(cookies.UserAgent);
            });
    }
    
    private static IAsyncPolicy<HttpResponseMessage> ConfigureErrorPolicyBuilder(
        PolicyBuilder<HttpResponseMessage> builder)
    {
        var delay = Backoff.DecorrelatedJitterBackoffV2(
            TimeSpan.FromSeconds(1), 5);
        return builder.WaitAndRetryAsync(delay, (_, span) =>
        {
            Colorful.Console.WriteLine($"Retrying in {span.Seconds}");
        });
    }
}
