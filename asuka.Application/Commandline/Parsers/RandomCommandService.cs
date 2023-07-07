using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using asuka.Application.Commandline.Options;
using asuka.Application.Commandline.Parsers.Common;
using asuka.Application.Output.Progress;
using asuka.Core.Chaptering;
using asuka.Core.Downloader;
using asuka.Core.Extensions;
using asuka.Core.Requests;
using Microsoft.Extensions.Logging;
using Sharprompt;

namespace asuka.Application.Commandline.Parsers;

public class RandomCommandService : ICommandLineParser
{
    private readonly IEnumerable<IGalleryRequestService> _apis;
    private readonly IEnumerable<IGalleryImageRequestService> _imageApis;
    private readonly IProgressProviderFactory _progressFactory;
    private readonly ILogger _logger;

    public RandomCommandService(
        IEnumerable<IGalleryRequestService> apis,
        IEnumerable<IGalleryImageRequestService> imageApis,
        IProgressProviderFactory progressFactory,
        ILogger logger)
    {
        _apis = apis;
        _imageApis = imageApis;
        _progressFactory = progressFactory;
        _logger = logger;
    }
    
    public async Task Run(object options)
    {
        var opts = (RandomOptions)options;
        
        // Find appropriate provider.
        var provider = _apis.GetFirst(opts.Provider);
        var imageProvider = _imageApis.FirstOrDefault(x => x.ProviderFor().For == opts.Provider);
        if (provider is null)
        {
            _logger.LogError("No such {ProviderName} found.", opts.Provider);
            return;
        }

        await ExecuteCommand(opts, provider, imageProvider);
    }

    private async Task ExecuteCommand(RandomOptions opts, IGalleryRequestService provider, IGalleryImageRequestService imageProvider)
    {
        while (true)
        {
            var response = await provider.GetRandom();

            _logger.LogInformation("{readable}", response.BuildReadableInformation());

            var prompt = Prompt.Confirm("Are you sure to download this one?", true);
            if (!prompt)
            {
                await Task.Delay(1000).ConfigureAwait(false);
                continue;
            }

            var series = new SeriesBuilder()
                .AddChapter(response, opts.Provider)
                .SetOutput(opts.Output)
                .Build();

            var progress = _progressFactory.Create(response.TotalPages, $"downloading: {response.Id}");
            var downloader = new DownloaderBuilder()
                .SetImageRequestService(imageProvider)
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
                await CompressAction.Compress(series, progress, opts.Output);
            }
        
            progress.Close();
            break;
        }
    }
}
