using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using asuka.Application.Services.Downloader;
using asuka.Application.Services.ProviderManager;
using asuka.ProviderSdk;
using Cocona;
using Microsoft.Extensions.Logging;
using Sharprompt;

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
            _logger.LogError("No provider with ID or alias of '{provider}' found", provider);
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
                _logger.LogWarning("No results found. Count: {count}", responses.Count);
                return;
            }

            // Select
            var selection = Prompt.MultiSelect("Select to download", responses, responses.Count,
                    textSelector: result => result.Title)
                .Select(x => (client, x))
                .ToList();

            _logger.LogInformation("Selected total of {count} galleries to download.", selection.Count);
            await Download(selection, output, pack);
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
    
    [Ignore]
    private async Task Download(List<(MetaInfo, Series)> queue, string? output, bool pack)
    {
        var downloader = _builder.CreateDownloaderInstance();
        downloader.Configure(c =>
        {
            c.OutputPath = output;
            c.Pack = pack;
        });

        downloader.AddSeriesRange(queue);
        await downloader.Start(Context.CancellationToken);
    }
}
