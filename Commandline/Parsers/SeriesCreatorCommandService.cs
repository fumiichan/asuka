using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using asuka.Api;
using asuka.Commandline.Options;
using asuka.Core.Requests;
using asuka.Downloader;
using asuka.Output;
using asuka.Output.Progress;
using FluentValidation;

namespace asuka.Commandline.Parsers;

public class SeriesCreatorCommandService : ICommandLineParser
{
    private readonly IGalleryRequestService _api;
    private readonly IGalleryImage _apiImage;
    private readonly IValidator<SeriesCreatorCommandOptions> _validator;
    private readonly IProgressFactory _progress;

    public SeriesCreatorCommandService(
        IGalleryRequestService api,
        IGalleryImage apiImage,
        IValidator<SeriesCreatorCommandOptions> validator,
        IProgressFactory progress)
    {
        _api = api;
        _apiImage = apiImage;
        _validator = validator;
        _progress = progress;
    }

    private async Task HandleArrayTask(IList<string> codes, string output, bool pack)
    {
        var mainProgressBar = _progress.Create(codes.Count, $"Downloading series...");
        var downloader = new SeriesDownloaderBuilder()
        {
            GalleryImage = _apiImage,
            Output = output,
            Progress = mainProgressBar,
            Pack = pack,
        };
        
        foreach (var code in codes)
        {
            try
            {
                var galleryResponse = await _api.FetchSingleAsync(code);
                downloader.AddChapter(galleryResponse);
            }
            catch
            {
                Console.WriteLine($"Skipping: {code} because of an error.");
            }
        }

        await downloader.Start();
    }

    public async Task RunAsync(object options)
    {
        var opts = (SeriesCreatorCommandOptions)options;

        var validationResult = await _validator.ValidateAsync(opts);
        if (!validationResult.IsValid)
        {
            validationResult.Errors.PrintValidationExceptions();
            return;
        }

        var list = opts.FromList.ToList();
        await HandleArrayTask(list, opts.Output, opts.Pack);
    }
}
