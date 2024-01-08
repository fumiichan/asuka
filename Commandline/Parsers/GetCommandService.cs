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
using asuka.Output;
using asuka.Output.Progress;
using FluentValidation;

namespace asuka.Commandline.Parsers;

public class GetCommandService : ICommandLineParser
{
    private readonly IGalleryImage _apiImage;
    private readonly IGalleryRequestService _api;
    private readonly IValidator<GetOptions> _validator;
    private readonly IProgressFactory _progress;

    public GetCommandService(
        IGalleryImage apiImage,
        IGalleryRequestService api,
        IValidator<GetOptions> validator,
        IProgressFactory progress)
    {
        _apiImage = apiImage;
        _api = api;
        _validator = validator;
        _progress = progress;
    }

    private async Task DownloadTask(int input, bool pack, bool readOnly, string outputPath)
    {
        var response = await _api.FetchSingleAsync(input.ToString());
        Console.WriteLine(response.ToFormattedText());

        // Don't download.
        if (readOnly)
        {
            return;
        }

        var mainProgress = _progress.Create(response.TotalPages,
            $"Downloading Manga: {response.Title.GetTitle()}");

        var output = PathUtils.Join(outputPath, response.Title.GetTitle());
        var downloader = new DownloadBuilder(response, 1)
        {
            Request = _apiImage,
            Output = output,
            OnEachComplete = _ =>
            {
                mainProgress.Tick();
            },
            OnComplete = async gallery =>
            {
                await gallery.WriteMetadata(Path.Combine(output, "details.json"));
                if (pack)
                {
                    await Compress.ToCbz(output, mainProgress);
                }
            }
        };

        await downloader.Start();
    }

    public async Task RunAsync(object options)
    {
        var opts = (GetOptions)options;
        var validationResult = await _validator.ValidateAsync(opts);
        if (!validationResult.IsValid)
        {
            validationResult.Errors.PrintValidationExceptions();
            return;
        }

        foreach (var code in opts.Input)
        {
            await DownloadTask(code, opts.Pack, opts.ReadOnly, opts.Output);
        }
    }
}
