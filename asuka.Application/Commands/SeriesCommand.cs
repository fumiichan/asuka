using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using asuka.Application.Services.Downloader;
using asuka.Application.Services.ProviderManager;
using asuka.ProviderSdk;
using Cocona;
using Microsoft.Extensions.Logging;

namespace asuka.Application.Commands;

internal sealed class SeriesCommand : CoconaConsoleAppBase
{
    private readonly IProviderManager _provider;
    private readonly IDownloaderBuilder _downloader;
    private readonly ILogger<SeriesCommand> _logger;

    public SeriesCommand(IProviderManager provider, IDownloaderBuilder downloader, ILogger<SeriesCommand> logger)
    {
        _provider = provider;
        _downloader = downloader;
        _logger = logger;
    }

    [Command("series", Aliases = ["sc"], Description = "Merge galleries together to form a single series (Single provider only)")]
    public async Task RunAsync(
        [Argument(Description = "Galleries to merge (in order)")]
        string[] galleryIds,
        
        [Option("provider", Description = "Specify a provider to use")]
        string provider,

        [Option("pack", ['p'], Description = "Compress downloads to a CBZ archive")]
        bool pack,

        [Option("output", ['o'], Description = "Specify destination path for downloads")]
        string? output)
    {
        Console.WriteLine("Retrieving metadata...");

        var client = _provider.GetProviderByAlias(provider);
        if (client == null)
        {
            Console.WriteLine($"No provider with ID or alias of '{provider}' found");
            return;
        }

        try
        {
            var series = await RetrieveMetadata(client, galleryIds);

            var downloader = _downloader.CreateDownloaderInstance();
            downloader.Configure(c =>
            {
                c.OutputPath = output;
                c.Pack = pack;
            });
            
            downloader.AddSeries(client, series);
            await downloader.Start(Context.CancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("Operation canceled by user.");
            Console.WriteLine("Operation canceled by user.");
        }
        catch (Exception ex)
        {
            _logger.LogError("An exception occured: {ex}", ex);
            Console.WriteLine($"An exception occured while downloading series. Error: {ex.Message}. See logs for more information.");
        }
    }

    [Ignore]
    private async Task<Series> RetrieveMetadata(MetaInfo client, IEnumerable<string> galleryIds)
    {
        var chapters = new List<Series>();
        foreach (var gallery in galleryIds)
        {
            try
            {
                var data = await client.GetSeries(gallery, Context.CancellationToken);
                chapters.Add(data);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to fetch gallery information of {gallery} with exception: {ex}", gallery, ex);
                Console.WriteLine($"Skipped {gallery} due to an exception.");
            }
        }

        var series = new Series
        {
            Title = chapters[0].Title,
            Artists = chapters[0].Artists,
            Authors = chapters[0].Authors,
            Genres = chapters[0].Genres,
            Chapters = [],
            Status = chapters[0].Status
        };

        var chapterCounter = 1;
        foreach (var item in chapters)
        {
            foreach (var chapter in item.Chapters)
            {
                series.Chapters.Add(new Chapter
                {
                    Id = chapterCounter,
                    Pages = chapter.Pages
                });

                chapterCounter += 1;
            }
        }

        return series;
    }
}
