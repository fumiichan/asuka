using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using asuka.Application.Services.Downloader;
using asuka.Application.Services.ProviderManager;
using asuka.Application.Validators;
using Cocona;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace asuka.Application.Commands;

internal sealed class FileCommand : CoconaConsoleAppBase
{
    private readonly IProviderManager _provider;
    private readonly IDownloaderBuilder _builder;
    private readonly ILogger<FileCommand> _logger;

    public FileCommand(IProviderManager provider, IDownloaderBuilder builder, ILogger<FileCommand> logger)
    {
        _provider = provider;
        _builder = builder;
        _logger = logger;
    }

    [Command("file", Aliases = ["f"], Description = "Download galleries from text file")]
    public async Task RunAsync(
        [Argument(Description = "Path to the text file to read")]
        [PathExists]
        [FileWithinSizeLimits]
        string file,
        
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
                ctx.Status("Reading text file...");
                var lines = await File.ReadAllLinesAsync(file, Encoding.UTF8, Context.CancellationToken);
                
                // Avoid lines that is not a full URL.
                var queue = lines.Where(x =>
                {
                    return Uri.TryCreate(x, UriKind.Absolute, out var uri)
                           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
                }).ToList();
                AnsiConsole.MarkupLine("Found total of {0} URLs.", queue.Count);

                ctx.Status("Downloading list...");
                foreach (var url in queue)
                {
                    if (Context.CancellationToken.IsCancellationRequested)
                    {
                        AnsiConsole.MarkupLine("[orange1]Cancelled.[/]");
                        break;
                    }
                    
                    var client = string.IsNullOrEmpty(provider)
                        ? _provider.GetProviderForGalleryId(url)
                        : _provider.GetProviderByAlias(provider);
                    
                    if (client == null)
                    {
                        AnsiConsole.MarkupLine("[orange1]Unsupported: {0}[/]", Markup.Escape(url));
                        continue;
                    }
                    
                    try
                    {
                        var response = await client.GetSeries(url, Context.CancellationToken);
                        var instance = _builder.CreateDownloaderInstance(client, response);
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
                        _logger.LogError("Fetching failed due to an exception: Series = {series}, Exception = {ex}", url, ex);
                        AnsiConsole.MarkupLine("[red3_1]Failed to download due to an exception: {0}[/]", Markup.Escape(url));
                    }
                }
                
                AnsiConsole.MarkupLine("[chartreuse1]All jobs finished.[/]");
            });
    }
}
