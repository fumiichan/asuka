using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using asuka.Application.Services.Downloader;
using asuka.Application.Services.ProviderManager;
using asuka.ProviderSdk;
using Cocona;
using Microsoft.Extensions.Logging;
using Spectre.Console;

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
        await AnsiConsole.Status()
            .StartAsync("Running...", async ctx =>
            {
                var client = _provider.GetProviderByAlias(provider);
                if (client == null)
                {
                    AnsiConsole.MarkupLine("[red3_1]No provider with ID or alias of '{0}' found[/]", Markup.Escape(provider));
                    return;
                }

                try
                {
                    ctx.Status("Retrieving gallery information...");

                    var queue = new List<Series>();
                    foreach (var gallery in galleryIds)
                    {
                        ctx.Status($"Retrieving gallery: {Markup.Escape(gallery)}");
                        
                        var response = await client.GetSeries(gallery, Context.CancellationToken);
                        queue.Add(response);
                        
                        AnsiConsole.MarkupLine("[chartreuse1]Retrieved: {0}[/]", Markup.Escape(response.Title));
                    }

                    ctx.Status("Building series information...");
                    var series = new Series
                    {
                        Title = queue[0].Title,
                        Artists = queue[0].Artists,
                        Authors = queue[0].Authors,
                        Genres = queue[0].Genres,
                        Chapters = [],
                        Status = queue[0].Status
                    };

                    var counter = 1;
                    foreach (var item in queue)
                    {
                        foreach (var chapter in item.Chapters)
                        {
                            series.Chapters.Add(new Chapter
                            {
                                Id = counter,
                                Pages = chapter.Pages,
                            });
                            counter++;
                        }
                    }

                    ctx.Status("Starting download...");
                    var instance = _downloader.CreateDownloaderInstance(client, series);
                    instance.Configure(c =>
                    {
                        c.OutputPath = output;
                        c.Pack = pack;
                    });
                    instance.OnProgress = m => ctx.Status(Markup.Escape(m));
                    await instance.Start();
                }
                catch (OperationCanceledException)
                { }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to fetch gallery with exception: {ex}", ex);
                    AnsiConsole.MarkupLine("[red3_1]Failed to fetch gallery. See logs for more details.[/]");
                }
            });
    }
}
