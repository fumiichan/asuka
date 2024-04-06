using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using asuka.Application.Services.Downloader;
using asuka.Application.Services.ProviderManager;
using asuka.Application.Validators;
using Cocona;
using Microsoft.Extensions.Logging;

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
        var lines = await File.ReadAllLinesAsync(file, Encoding.UTF8, Context.CancellationToken);
        foreach (var url in lines)
        {
            var client = string.IsNullOrEmpty(provider)
                ? _provider.GetProviderForGalleryId(url)
                : _provider.GetProviderByAlias(provider);

            if (client == null)
            {
                Console.WriteLine($"Skipped '{url}' because no provider supports this.");
                continue;
            }

            try
            {
                var response = await client.GetSeries(url, Context.CancellationToken);

                var downloader = _builder.CreateDownloaderInstance();
                downloader.Configure(c =>
                {
                    c.OutputPath = output;
                    c.Pack = pack;
                });
 
                downloader.AddSeries(client, response);
                await downloader.Start(Context.CancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Operation canceled by the user.");
                Console.WriteLine("Operation canceled by the user.");
                
                break;
            }
            catch (Exception e)
            {
                _logger.LogError("Download failed for '{code}' due to an exception: {ex}", url, e);
                Console.WriteLine($"Download failed due to an exception: {e.Message}. See logs for more details.");
            }
        }
    }
}
