using System;
using System.Threading.Tasks;
using Sharprompt;
using asuka.CommandOptions;
using asuka.Downloader;
using asuka.Output;
using asuka.Services;
using Microsoft.Extensions.Configuration;

namespace asuka.CommandParsers;

public class RandomCommandService : IRandomCommandService
{
    private readonly IDownloadService _download;
    private readonly IGalleryRequestService _api;
    private readonly IConsoleWriter _console;

    public RandomCommandService(IDownloadService download, IGalleryRequestService api, IConsoleWriter console)
    {
        _download = download;
        _api = api;
        _console = console;
    }

    public async Task RunAsync(RandomOptions opts, IConfiguration configuration)
    {
        var totalNumbers = await _api.GetTotalGalleryCountAsync();

        while (true)
        {
            var randomCode = new Random().Next(1, totalNumbers);
            var response = await _api.FetchSingleAsync(randomCode.ToString());

            _console.WriteLine(response.ToReadable());

            var prompt = Prompt.Confirm("Are you sure to download this one?", true);
            if (!prompt)
            {
                await Task.Delay(1000).ConfigureAwait(false);
                continue;
            }

            var useTachiyomiLayout = opts.UseTachiyomiLayout || bool.Parse(configuration["UseTachiyomiFolderStructure"]);
            await _download.DownloadAsync(response, opts.Output, opts.Pack, useTachiyomiLayout);
            break;
        }
    }
}
