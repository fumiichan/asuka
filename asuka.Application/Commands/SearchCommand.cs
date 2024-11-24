using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using asuka.Application.Services.Downloader;
using asuka.Application.Services.ProviderManager;
using asuka.ProviderSdk;
using Cocona;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace asuka.Application.Commands;

internal sealed class SearchCommand : CoconaConsoleAppBase
{
    private readonly IProviderManager _provider;
    private readonly IDownloaderBuilder _builder;
    private readonly ILogger<SearchCommand> _logger;

    public SearchCommand(IProviderManager provider, IDownloaderBuilder builder, ILogger<SearchCommand> logger)
    {
        _provider = provider;
        _builder = builder;
        _logger = logger;
    }

    [Command("search", Aliases = ["s"], Description = "Search something in the gallery")]
    public async Task RunAsync(
        [Option("provider", Description = "Specify a provider to use")]
        string provider,
        
        [Option("pack", ['p'], Description = "Compress downloads to a CBZ archive")]
        bool pack,

        [Option("output", ['o'], Description = "Specify destination path for downloads")]
        string? output,
        
        [Option("queries", ['q'], Description = "Search query")]
        string[]? queries,

        [Option("exclude", ['e'], Description = "Excluded tags/queries")]
        string[]? excluded,

        [Option("sort", ['s'], Description = "Sort options. Values vary across providers")]
        string? sort,
        
        [Option("page", Description = "Specify search page number")]
        int pageNumber = 1)
    {
        var client = _provider.GetProviderByAlias(provider);
        if (client == null)
        {
            AnsiConsole.MarkupLine("[red3_1]No provider with ID or alias of '{0}' found[/]", Markup.Escape(provider));
            return;
        }
        
        var searchArguments = new List<string>();
        if (queries != null)
        {
            searchArguments.AddRange(queries);
        }

        if (excluded != null)
        {
            searchArguments.AddRange(excluded.Select(exclude => $"-{exclude}"));
        }
        
        _logger.LogInformation("Search initiated with following arguments: {queries} on page {page}", searchArguments, pageNumber);
        var query = new SearchQuery
        {
            SearchQueries = searchArguments,
            PageNumber = pageNumber,
            Sort = sort ?? "popularity"
        };

        try
        {
            var responses = await client.Search(query, Context.CancellationToken);
            if (responses.Count < 1)
            {
                AnsiConsole.MarkupLine("[orange1]No results found.[/]");
                return;
            }

            // Select
            var selection = AnsiConsole.Prompt(
                new MultiSelectionPrompt<Series>()
                    .Title("Select to download")
                    .Required()
                    .InstructionsText(
                        "[grey](Press [blue]<space>[/] to pick, and [green]<enter>[/] to start downloading)[/]")
                    .AddChoices(responses)
                    .UseConverter(x => Markup.Escape(x.Title)));

            _logger.LogInformation("Selected galleries: {selected}", selection);
            await AnsiConsole.Status()
                .StartAsync("Running...", async ctx =>
                {
                    foreach (var series in responses)
                    {
                        ctx.Status($"Starting: {series.Title}...");
                        
                        var instance = _builder.CreateDownloaderInstance(client, series);
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
        catch (OperationCanceledException)
        {
            _logger.LogError("Operation cancelled by the user.");
        }
        catch (Exception ex)
        {
            _logger.LogError("An exception occured: {ex}", ex);
            Console.WriteLine($"An exception occured. Error: {ex.Message}. See logs for more information");
        }
    }
}
