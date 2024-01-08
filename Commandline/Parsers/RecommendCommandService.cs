using System.IO;
using System.Threading.Tasks;
using asuka.Api;
using asuka.Commandline.Options;
using asuka.Core.Compression;
using asuka.Core.Downloader;
using asuka.Core.Extensions;
using asuka.Core.Mappings;
using asuka.Core.Requests;
using asuka.Core.Utilities;
using asuka.Output;
using asuka.Output.Progress;
using FluentValidation;

namespace asuka.Commandline.Parsers;

public class RecommendCommandService : ICommandLineParser
{
    private readonly IValidator<RecommendOptions> _validator;
    private readonly IGalleryRequestService _api;
    private readonly IGalleryImage _apiImage;
    private readonly IProgressFactory _progress;

    public RecommendCommandService(
        IValidator<RecommendOptions> validator,
        IGalleryRequestService api,
        IGalleryImage apiImage,
        IProgressFactory progress)
    {
        _validator = validator;
        _api = api;
        _apiImage = apiImage;
        _progress = progress;
    }

    public async Task RunAsync(object options)
    {
        var opts = (RecommendOptions)options;
        var validator = await _validator.ValidateAsync(opts);
        if (!validator.IsValid)
        {
            validator.Errors.PrintValidationExceptions();
            return;
        }

        var responses = await _api.FetchRecommendedAsync(opts.Input.ToString());
        var selection = responses.FilterByUserSelected();

        // Initialise the Progress bar.
        var mainProgress = _progress.Create(selection.Count, $"Downloading {opts.Input} recommendations...");

        foreach (var response in selection)
        {
            var childProgress = mainProgress
                .Spawn(response.TotalPages, $"Downloading {response.Title.GetTitle()}")!;
            var output = PathUtils.Join(opts.Output, response.Title.GetTitle());
            var downloader = new DownloadBuilder(response, 1)
            {
                Request = _apiImage,
                Output = output,
                OnEachComplete = _ =>
                {
                    childProgress.Tick();
                },
                OnComplete = async data =>
                {
                    await data.WriteMetadata(Path.Combine(output, "details.json"));
                    if (opts.Pack)
                    {
                        await Compress.ToCbz(output, childProgress);
                    }
                }
            };

            await downloader.Start();
            mainProgress.Tick();
        }
    }
}
