using System;
using System.Drawing;
using System.Net.Http;
using System.Text.Json;
using asuka.Configuration;
using asuka.Core.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Refit;

namespace asuka.Installers.Refit;

public class ConfigureRefitService : IInstaller
{
    public void ConfigureService(IServiceCollection services, IConfiguration configuration)
    {
        var cookies = new CloudflareBypass();

        var cloudflareClearance = cookies.GetCookieByName("cf_clearance");
        var csrfToken = cookies.GetCookieByName("csrftoken");

        if (cloudflareClearance == null || csrfToken == null)
        {
            Colorful.Console.WriteLine("WARNING: Cookies are unset! Your request might fail.", Color.Red);
        }

        var contentSerializerSettings = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var configureRefit = new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(contentSerializerSettings),
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

        var apiBaseAddress = configuration.GetSection("BaseAddresses")["ApiBaseAddress"]
                             ?? "https://nhentai.net";
        var imageBaseAddress = configuration.GetSection("BaseAddresses")["ImageBaseAddress"]
                               ?? "https://i.nhentai.net";
        
        services.AddRefitClient<IGalleryApi>(configureRefit)
            .AddTransientHttpErrorPolicy(ConfigureErrorPolicyBuilder)
            .ConfigureHttpClient(httpClient =>
            {
                httpClient.BaseAddress = new Uri(apiBaseAddress);
                httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(cookies.UserAgent);
            });
        services.AddRefitClient<IGalleryImage>()
            .AddTransientHttpErrorPolicy(ConfigureErrorPolicyBuilder)
            .ConfigureHttpClient(httpClient =>
            {
                httpClient.BaseAddress = new Uri(imageBaseAddress);
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
