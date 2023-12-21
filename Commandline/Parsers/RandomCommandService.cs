using System;
using System.IO;
using System.Threading.Tasks;
using asuka.Api;
using asuka.Commandline.Options;
using asuka.Core.Compression;
using asuka.Core.Downloader;
using asuka.Core.Extensions;
using asuka.Core.Requests;
using asuka.Core.Utilities;
using asuka.Output.Progress;
using Sharprompt;

namespace asuka.Commandline.Parsers;

public class RandomCommandService : ICommandLineParser
{
    private readonly IGalleryRequestService _api;
    private readonly IGalleryImage _apiImage;
    private readonly IProgressFactory _progress;

    public RandomCommandService(
        IGalleryRequestService api,
        IGalleryImage apiImage,
        IProgressFactory progress)
    {
        _api = api;
        _apiImage = apiImage;
        _progress = progress;
    }

    public async Task RunAsync(object options)
    {
        var opts = (RandomOptions)options;
        var totalNumbers = await _api.GetTotalGalleryCountAsync();

        while (true)
        {
            var randomCode = new Random().Next(1, totalNumbers);
            var response = await _api.FetchSingleAsync(randomCode.ToString());

            Console.WriteLine(response.ToFormattedText());

            var prompt = Prompt.Confirm("Are you sure to download this one?", true);
            if (!prompt)
            {
                await Task.Delay(1000).ConfigureAwait(false);
                continue;
            }
            
            var mainProgress = _progress.Create(response.TotalPages,
                $"Downloading Manga: {response.Title.GetTitle()}");

            var output = Path.Combine(opts.Output, PathUtils.NormalizeName(response.Title.GetTitle()));
            var downloader = new DownloadBuilder(response, 1)
            {
                Output = output,
                Request = _apiImage,
                OnEachComplete = _ =>
                {
                    mainProgress.Tick();
                },
                OnComplete = async data =>
                {
                    await data.WriteMetadata(Path.Combine(output, "details.json"));
                    if (opts.Pack)
                    {
                        await Compress.ToCbz(output, mainProgress);
                    }
                }
            };

            await downloader.Start();
            break;
        }
    }
}
