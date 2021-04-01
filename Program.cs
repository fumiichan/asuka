using System;
using System.Drawing;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Refit;
using asuka.Api;
using asuka.CommandParsers;
using asuka.Downloader;
using asuka.Output;
using asuka.Services;

namespace asuka
{
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
            await app.RunAsync(args);
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
            services.AddValidatorsFromAssemblyContaining<Program>();
            
            // Configure refit
            var configureRefit = new RefitSettings
            {
                ContentSerializer = new NewtonsoftJsonContentSerializer()
            };
            services.AddRefitClient<IGalleryApi>(configureRefit)
                .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(new []
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(20),
                    TimeSpan.FromSeconds(50)
                }))
                .ConfigureHttpClient(httpClient =>
                {
                    httpClient.BaseAddress = new Uri(configuration["ApiBaseAddress"]);
                });
            services.AddRefitClient<IGalleryImage>()
                .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryForeverAsync(
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, context) =>
                    {
                        Colorful.Console.WriteLine($"Download will retry in {timeSpan}", Color.Yellow);
                        Colorful.Console.WriteLine($"Exception: {exception.Exception.Message}", Color.Yellow);
                    }))
                .ConfigureHttpClient(httpClient =>
                {
                    httpClient.BaseAddress = new Uri(configuration["ImageBaseAddress"]);
                });
        }
    }
}