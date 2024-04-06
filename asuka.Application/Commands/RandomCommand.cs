using System;
using System.Threading.Tasks;
using asuka.Application.Services.Downloader;
using asuka.Application.Services.ProviderManager;
using Cocona;
using Microsoft.Extensions.Logging;
using Sharprompt;

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
            Console.WriteLine($"No provider with ID or alias of '{provider}' found");
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
                var prompt = Prompt.Confirm("Are you sure to download this one?", true);
                if (!prompt)
                {
                    _logger.LogInformation("Picked no. Title: {code}", random.Title);
                    await Task.Delay(1000).ConfigureAwait(false);
                    continue;
                }

                _logger.LogInformation("Picked yes to download: {title}", random.Title);

                var downloader = _builder.CreateDownloaderInstance();
                downloader.Configure(c =>
                {
                    c.OutputPath = output;
                    c.Pack = pack;
                });

                downloader.AddSeries(client, random);
                await downloader.Start(Context.CancellationToken);

                break;
            }
            catch (NotSupportedException)
            {
                _logger.LogError("Unsupported by the provider. Provider ID: {provider}", client.GetId());
                Console.WriteLine("Unsupported by the provider.");

                break;
            }
            catch (OperationCanceledException)
            {
                _logger.LogError("Operation cancelled by the user.");
                Console.WriteLine("Operation cancelled by the user.");

                break;
            }
        }
    }
}
