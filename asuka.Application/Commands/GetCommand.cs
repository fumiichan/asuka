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

internal sealed class GetCommand : CoconaConsoleAppBase
{
    private readonly IProviderManager _provider;
    private readonly IDownloaderBuilder _builder;
    private readonly ILogger<GetCommand> _logger;

    public GetCommand(IProviderManager provider, IDownloaderBuilder builder, ILogger<GetCommand> logger)
    {
        _provider = provider;
        _builder = builder;
        _logger = logger;
    }

    [Command("get", Aliases = ["g"], Description = "Download galleries")]
    public async Task RunAsync(
        [Argument(Description = "List of galleries to download")]
        string[] galleryIds,
        
        [Option("provider", Description = "Specify a provider to use")]
        string? provider,

        [Option("pack", ['p'], Description = "Compress downloads to a CBZ archive")]
        bool pack,

        [Option("output", ['o'], Description = "Specify destination path for downloads")]
        string? output)
    {
        await AnsiConsole.Status()
            .StartAsync("Running...", async ctx =>
            {
                ctx.Status("Retrieving gallery information...");
                
                var queue = new List<(MetaInfo, Series)>();
                foreach (var code in galleryIds)
                {
                    if (Context.CancellationToken.IsCancellationRequested)
                    {
                        AnsiConsole.MarkupLine("[orange1]Cancelled.[/]");
                        break;
                    }
                    
                    // Find appropriate provider
                    var client = string.IsNullOrEmpty(provider)
                        ? _provider.GetProviderForGalleryId(code)
                        : _provider.GetProviderByAlias(provider);
                    
                    if (client == null)
                    {
                        AnsiConsole.MarkupLine("[orange1]No such provider or unsupported: {0}[/]", code);
                        continue;
                    }
                    
                    try
                    {
                        var response = await client.GetSeries(code, Context.CancellationToken);
                        queue.Add((client, response));
                        
                        AnsiConsole.MarkupLine("[chartreuse1]Retrieved: {0}[/]", Markup.Escape(response.Title));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Fetching failed due to an exception: Input = {code}, Exception = {ex}", code, ex);
                        AnsiConsole.MarkupLine("[red3_1]Failed to fetch: {0}. See logs for more information.[/]", code);
                    }
                }

                ctx.Status("Starting download...");
                foreach (var (client, series) in queue)
                {
                    if (Context.CancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    try
                    {
                        var instance = _builder.CreateDownloaderInstance(client, series);
                        instance.Configure(c =>
                        {
                            c.OutputPath = output;
                            c.Pack = pack;
                        });
                        instance.OnProgress = m => ctx.Status(Markup.Escape(m));
                        await instance.Start();
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Fetching failed due to an exception: Series = {series}, Exception = {ex}", series, ex);
                        AnsiConsole.MarkupLine("[red3_1]Failed to download: {0}. See logs for more information.[/]", Markup.Escape(series.Title));
                    }
                }
                
                AnsiConsole.MarkupLine("[chartreuse1]All jobs finished.[/]");
            });
    }
}
