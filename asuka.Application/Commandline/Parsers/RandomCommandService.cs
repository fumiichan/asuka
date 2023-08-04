using System.Threading.Tasks;
using asuka.Application.Commandline.Options;
using asuka.Application.Commandline.Parsers.Common;
using asuka.Application.Output.ConsoleWriter;
using asuka.Application.Output.Progress;
using asuka.Application.Services;
using asuka.Core.Chaptering;
using asuka.Core.Downloader;
using asuka.Sdk.Providers.Extensions;
using Microsoft.Extensions.Logging;
using Sharprompt;

namespace asuka.Application.Commandline.Parsers;

public class RandomCommandService : ICommandLineParser
{
    private readonly ProviderResolverService _provider;
    private readonly IProgressProviderFactory _progressFactory;
    private readonly ILogger<RandomCommandService> _logger;
    private readonly IConsoleWriter _console;

    public RandomCommandService(
        ProviderResolverService provider,
        IProgressProviderFactory progressFactory,
        ILogger<RandomCommandService> logger,
        IConsoleWriter console)
    {
        _provider = provider;
        _progressFactory = progressFactory;
        _logger = logger;
        _console = console;
    }
    
    public async Task Run(object options)
    {
        var opts = (RandomOptions)options;
        _logger.LogInformation("RandomCommandService called with args: {@Opts}", opts);
        
        // Find appropriate provider.
        var provider = _provider.GetProviderByName(opts.Provider);
        if (provider is null)
        {
            _console.WriteError($"No such {opts.Provider} found.");
            return;
        }

        await ExecuteCommand(opts, provider);
    }

    private async Task ExecuteCommand(RandomOptions opts, Provider provider)
    {
        while (true)
        {
            var response = await provider.Api.GetRandom();
            if (response is null)
            {
                _console.WriteError("Unable to fetch random ID.");
                return;
            }

            _console.Write(response.BuildReadableInformation());

            var prompt = Prompt.Confirm("Are you sure to download this one?", true);
            if (!prompt)
            {
                await Task.Delay(1000).ConfigureAwait(false);
                continue;
            }

            var series = new SeriesBuilder()
                .AddChapter(response, provider.ImageApi)
                .SetOutput(opts.Output)
                .Build();
            _logger.LogInformation("Series built: {@Series}", series);

            var progress = _progressFactory.Create(response.TotalPages, $"downloading: {response.Id}");
            var downloader = new DownloaderBuilder()
                .AttachLogger(_logger)
                .SetChapter(series.Chapters[0])
                .SetOutput(series.Output)
                .SetEachCompleteHandler(e =>
                {
                    progress.Tick($"{e.Message}: {response.Id}");
                })
                .Build();

            await downloader.Start();
            await series.Chapters[0].Data.WriteJsonMetadata(series.Output);
        
            if (opts.Pack)
            {
                await CompressAction.Compress(series, opts.Output, progress, _logger);
            }
        
            progress.Close();
            break;
        }
    }
}
