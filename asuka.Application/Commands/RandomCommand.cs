using System;
using System.Threading.Tasks;
using asuka.Application.Services.Downloader;
using asuka.Application.Services.ProviderManager;
using Cocona;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace asuka.Application.Commands;

internal sealed class RandomCommand : CoconaConsoleAppBase
{
    private readonly IProviderManager _provider;
    private readonly IDownloaderBuilder _builder;
    private readonly ILogger<RandomCommand> _logger;

    public RandomCommand(IProviderManager provider, IDownloaderBuilder builder, ILogger<RandomCommand> logger)
    {
        _provider = provider;
        _builder = builder;
        _logger = logger;
    }

    [Command("random", Aliases = ["r"], Description = "Randomly pick a gallery")]
    public async Task RunAsync(
        [Option("provider", Description = "Specify a provider to use")]
        string provider,
        
        [Option("pack", ['p'], Description = "Compress downloads to a CBZ archive")]
        bool pack,

        [Option("output", ['o'], Description = "Specify destination path for downloads")]
        string? output)
    {
        var client = _provider.GetProviderByAlias(provider);
        if (client == null)
        {
            AnsiConsole.MarkupLine("[red3_1]No provider with ID or alias of '{0}' found[/]", Markup.Escape(provider));
            return;
        }
        
        while (true)
        {
            if (Context.CancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Operation cancelled by the user.");
                break;
            }

            try
            {
                var random = await client.GetRandom(Context.CancellationToken);
                Console.WriteLine($"Title: {random.Title}");
                Console.WriteLine($"Artist: {string.Join(", ", random.Artists)}");
                Console.WriteLine($"Genres/Tags: {string.Join(", ", random.Genres)}");
                Console.WriteLine($"Total Chapters: {random.Chapters.Count}");

                var confirmation = AnsiConsole.Prompt(
                    new TextPrompt<bool>("Are you sure to download this one?")
                        .AddChoice(true)
                        .AddChoice(false)
                        .DefaultValue(true)
                        .WithConverter(c => c ? "y" : "n"));
                if (!confirmation)
                {
                    _logger.LogInformation("Picked no. Title: {code}", random.Title);
                    await Task.Delay(1000).ConfigureAwait(false);
                    continue;
                }

                _logger.LogInformation("Picked yes to download: {title}", random.Title);

                await AnsiConsole.Status()
                    .StartAsync("Running...", async ctx =>
                    {
                        var downloader = _builder.CreateDownloaderInstance(client, random);
                        downloader.Configure(c =>
                        {
                            c.OutputPath = output;
                            c.Pack = pack;
                        });
                        downloader.OnProgress = m => ctx.Status(Markup.Escape(m));
                        await downloader.Start(Context.CancellationToken);
                    });
                
                AnsiConsole.MarkupLine("[chartreuse1]All jobs finished.[/]");
                break;
            }
            catch (NotSupportedException)
            {
                AnsiConsole.MarkupLine("[red3_1]Unsupported by the provider.[/]");
                break;
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError("Fetching failed due to an exception: {ex}", ex);
                AnsiConsole.MarkupLine("[red3_1]Failed. See logs for more information.[/]");
            }
        }
    }
}
