using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using asuka.Application.Services.Downloader;
using asuka.Application.Services.ProviderManager;
using asuka.ProviderSdk;
using Cocona;
using Microsoft.Extensions.Logging;

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
        
        Console.WriteLine("Retrieving metadata...");
        var queue = await RetrieveMetadata(galleryIds, provider);

        // Download
        await Download(queue, output, pack);
    }

    [Ignore]
    private async Task<List<(MetaInfo, Series)>> RetrieveMetadata(IEnumerable<string> galleryIds, string? provider)
    {
        var queue = new List<(MetaInfo, Series)>();
        foreach (var code in galleryIds)
        {
            var client = string.IsNullOrEmpty(provider)
                ? _provider.GetProviderForGalleryId(code)
                : _provider.GetProviderByAlias(provider);

            if (client == null)
            {
                Console.WriteLine($"Skipped {code} due to no provider supports this.");
                continue;
            }

            try
            {
                var response = await client.GetSeries(code, Context.CancellationToken);
                queue.Add((client, response));
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Operation canceled by the user.");
                return [];
            }
            catch (Exception ex)
            {
                _logger.LogError("Fetching failed due to an exception: {ex}", ex);
                Console.WriteLine($"Item skipped due to an exception: {code}. See logs for more details.");
            }
        }

        return queue;
    }

    [Ignore]
    private async Task Download(List<(MetaInfo, Series)> queue, string? output, bool pack)
    {
        // Don't start if cancelled.
        if (Context.CancellationToken.IsCancellationRequested)
        {
            return;
        }
        
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
