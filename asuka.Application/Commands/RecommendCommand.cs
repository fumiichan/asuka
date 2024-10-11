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
            Console.WriteLine($"No provider with ID or alias of '{provider}' found");
            return;
        }

        try
        {
            var result = await client.GetRecommendations(gallery);

            // Select
            var selection = Prompt.MultiSelect("Select to download", result, result.Count,
                    textSelector: x => x.Title)
                .Select(x => (client, x))
                .ToList();

            _logger.LogInformation("Selected total of {count} galleries to download.", selection.Count);
            await Download(selection, output, pack);
        }
        catch (NotSupportedException)
        {
            _logger.LogError("Recommendations unsupported by provider. Provider ID {provider}", client.GetId());
            Console.WriteLine("This provider doesn't support fetching recommendations.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("Operation cancelled by the user.");
            Console.WriteLine("Operation cancelled by the user.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Operation failed due to an exception: {ex}", ex);
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
