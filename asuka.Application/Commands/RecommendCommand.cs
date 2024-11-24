using System;
using System.Threading.Tasks;
using asuka.Application.Services.Downloader;
using asuka.Application.Services.ProviderManager;
using asuka.ProviderSdk;
using Cocona;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace asuka.Application.Commands;

internal sealed class RecommendCommand : CoconaConsoleAppBase
{
    private readonly IProviderManager _provider;
    private readonly IDownloaderBuilder _builder;
    private readonly ILogger<RecommendCommand> _logger;

    public RecommendCommand(IProviderManager provider, IDownloaderBuilder builder, ILogger<RecommendCommand> logger)
    {
        _provider = provider;
        _builder = builder;
        _logger = logger;
    }

    [Command("recommend", Aliases = ["rc"], Description = "Download recommendation from specified gallery.")]
    public async Task RunAsync(
        [Argument]
        string gallery,
        
        [Option("provider", Description = "Specify a provider to use")]
        string provider,

        [Option("pack", ['p'], Description = "Compress downloads to a CBZ archive")]
        bool pack,

        [Option("output", ['o'], Description = "Specify destination path for downloads")]
        string? output)
    {
        var client = string.IsNullOrEmpty(provider)
            ? _provider.GetProviderForGalleryId(gallery)
            : _provider.GetProviderByAlias(provider);

        if (client == null)
        {
            AnsiConsole.MarkupLine("[red3_1]No provider with ID or alias of '{0}' found[/]", Markup.Escape(provider));
            return;
        }

        try
        {
            var result = await client.GetRecommendations(gallery);

            // Select
            var selection = AnsiConsole.Prompt(
                new MultiSelectionPrompt<Series>()
                    .Title("Select to download")
                    .Required()
                    .InstructionsText(
                        "[grey](Press [blue]<space>[/] to pick, and [green]<enter>[/] to start downloading)[/]")
                    .AddChoices(result)
                    .UseConverter(x => Markup.Escape(x.Title)));

            _logger.LogInformation("Selection: {selection}", selection);
            AnsiConsole.MarkupLine("Selected total of {} of galleries to download.", selection.Count);

            await AnsiConsole.Status()
                .StartAsync("Running...", async ctx =>
                {
                    foreach (var item in selection)
                    {
                        var instance = _builder.CreateDownloaderInstance(client, item);
                        instance.Configure(c =>
                        {
                            c.OutputPath = output;
                            c.Pack = pack;
                        });
                        instance.OnProgress = m => ctx.Status(Markup.Escape(m));
                        await instance.Start();
                    }
                });
        }
        catch (NotSupportedException)
        {
            AnsiConsole.MarkupLine("[red3_1]Unsupported by the provider.[/]");
        }
        catch (OperationCanceledException)
        {
            AnsiConsole.MarkupLine("[yellow]Cancelled.[/]");
        }
        catch (Exception ex)
        {
            _logger.LogError("Operation failed due to an exception: {ex}", ex);
            AnsiConsole.MarkupLine("[red3_1]An exception occurred. See logs for more information.[/]");
        }
        
        AnsiConsole.MarkupLine("[chartreuse1]All jobs finished.[/]");
    }
}
