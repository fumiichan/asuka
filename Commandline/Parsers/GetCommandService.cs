using System.Threading.Tasks;
using asuka.Commandline.Options;
using asuka.Core.Compression;
using asuka.Core.Downloader;
using asuka.Core.Requests;
using asuka.Output;
using asuka.Output.Writer;
using FluentValidation;

namespace asuka.Commandline.Parsers;

public class GetCommandService : IGetCommandService
{
    private readonly IGalleryRequestService _api;
    private readonly IValidator<IRequiresInputOption> _validator;
    private readonly IDownloadService _download;
    private readonly IConsoleWriter _console;
    private readonly IPackArchiveToCbz _pack;

    public GetCommandService(
        IGalleryRequestService api,
        IValidator<IRequiresInputOption> validator,
        IDownloadService download,
        IConsoleWriter console,
        IPackArchiveToCbz pack)
    {
        _api = api;
        _validator = validator;
        _download = download;
        _console = console;
        _pack = pack;
    }

    public async Task RunAsync(GetOptions opts)
    {
        var validationResult = await _validator.ValidateAsync(opts);
        if (!validationResult.IsValid)
        {
            _console.ValidationErrors(validationResult.Errors);
            return;
        }

        var response = await _api.FetchSingleAsync(opts.Input.ToString());
        _console.WriteLine(response.ToReadable());

        if (opts.ReadOnly)
        {
            return;
        }
        
        var result = await _download.DownloadAsync(response, opts.Output);
        if (opts.Pack)
        {
            await _pack.RunAsync(result.DestinationPath, opts.Output, result.ProgressBar);
        }
    }
}
