using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using asuka.Api;
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
        var contentSerializerSettings = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var cloudflare = CreateCookieFromConfig(configuration, "CloudflareClearance");
        var csrf = CreateCookieFromConfig(configuration, "CsrfToken");

        // Warn about cookies.
        if (cloudflare == null || csrf == null)
        {
            Console.WriteLine("Cookies might be unset! Run \"asuka cookie\" command to set your cookies!");
        }

        var configureRefit = new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(contentSerializerSettings),
            HttpMessageHandlerFactory = () =>
            {
                var handler = new HttpClientHandler();

                if (cloudflare != null)
                {
                    handler.CookieContainer.Add(cloudflare);
                }

                if (csrf != null)
                {
                    handler.CookieContainer.Add(csrf);
                }

                return handler;
            }
        };

        var apiBaseAddress = configuration.GetSection("BaseAddresses")
            .GetValue<string>("ApiBaseAddress") ?? "https://nhentai.net";
        var imageBaseAddress = configuration.GetSection("BaseAddresses")
            .GetValue<string>("ImageBaseAddress") ?? "https://i.nhentai.net";
        var userAgent = configuration.GetSection("RequestOptions")
            .GetValue<string>("UserAgent");

        services.AddRefitClient<IGalleryApi>(configureRefit)
            .AddTransientHttpErrorPolicy(ConfigureErrorPolicyBuilder)
            .ConfigureHttpClient(httpClient =>
            {
                httpClient.BaseAddress = new Uri(apiBaseAddress);
                httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);
            });
        services.AddRefitClient<IGalleryImage>()
            .AddTransientHttpErrorPolicy(ConfigureErrorPolicyBuilder)
            .ConfigureHttpClient(httpClient =>
            {
                httpClient.BaseAddress = new Uri(imageBaseAddress);
                httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);
            });
    }
    
    private static IAsyncPolicy<HttpResponseMessage> ConfigureErrorPolicyBuilder(
        PolicyBuilder<HttpResponseMessage> builder)
    {
        var delay = Backoff.DecorrelatedJitterBackoffV2(
            TimeSpan.FromSeconds(1), 5);
        return builder.WaitAndRetryAsync(delay, (_, span) =>
        {
            Console.WriteLine($"Retrying in {span.Seconds}");
        });
    }
    
    private static Cookie? CreateCookieFromConfig(IConfiguration configuration, string name)
    {
        var option = configuration
            .GetSection("RequestOptions")
            .GetSection("Cookies")
            .GetSection(name);

        if (string.IsNullOrEmpty(option.GetValue<string>("Name")))
        {
            return null;
        }

        var cookieName = option.GetValue<string>("Name");
        var cookieValue = option.GetValue<string>("Value");
        var cookieDomain = option.GetValue<string>("Domain");
        var cookieHttpOnlyFlag = option.GetValue<bool>("HttpOnly");
        var cookieSecureFlag = option.GetValue<bool>("Secure");

        if (string.IsNullOrEmpty(cookieName) || string.IsNullOrEmpty(cookieValue) || string.IsNullOrEmpty(cookieDomain))
        {
            return null;
        }

        return new Cookie(cookieName, cookieValue)
        {
            Domain = cookieDomain,
            HttpOnly = cookieHttpOnlyFlag,
            Secure = cookieSecureFlag
        };
    }
}
