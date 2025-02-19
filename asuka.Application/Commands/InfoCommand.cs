using System;
using System.Threading.Tasks;
using asuka.Application.Services.ProviderManager;
using Cocona;
using Microsoft.Extensions.Logging;

namespace asuka.Application.Commands;

internal sealed class InfoCommand : CoconaConsoleAppBase
{
    private readonly IProviderManager _provider;
    private readonly ILogger<InfoCommand> _logger;

    public InfoCommand(IProviderManager provider, ILogger<InfoCommand> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    [Command("info", Aliases = ["i"], Description = "View gallery information")]
    public async Task RunAsync(
        [Argument(Description = "List of galleries to download")]
        string galleryId,

        [Option("provider", Description = "Specify a provider to use")]
        string? provider)
    {
        var client = string.IsNullOrEmpty(provider)
            ? _provider.GetProviderForGalleryId(galleryId)
            : _provider.GetProviderByAlias(provider);

        if (client == null)
        {
            Console.WriteLine($"'{galleryId}' has no providers that supports this ID.");
            return;
        }

        try
        {
            var result = await client.GetSeries(galleryId, Context.CancellationToken);
            
            Console.WriteLine($"Title: {result.Title}");
            Console.WriteLine($"Artist: {string.Join(", ", result.Artists)}");
            Console.WriteLine($"Genres/Tags: {string.Join(", ", result.Genres)}");
            Console.WriteLine($"Total Chapters: {result.Chapters.Count}");
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("Operation canceled by user.");
            Console.WriteLine("Operation canceled by user.");
        }
        catch (Exception ex)
        {
            _logger.LogError("An exception occured. Exception: {ex}", ex);
            Console.WriteLine($"An exception occured. Error: {ex.Message}. See logs for more details");
        }
    }
}
