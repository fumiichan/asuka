using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentValidation;
using asuka.CommandOptions;
using asuka.Configuration;
using asuka.Downloader;
using asuka.Output;
using asuka.Services;
using Microsoft.Extensions.Configuration;

namespace asuka.CommandParsers;

public class GetCommandService : IGetCommandService
{
    private readonly IGalleryRequestService _api;
    private readonly IValidator<IRequiresInputOption> _validator;
    private readonly IDownloadService _download;
    private readonly IConsoleWriter _console;
    private readonly IConfigurationManager _configurationManager;

    public GetCommandService(
        IGalleryRequestService api,
        IValidator<IRequiresInputOption> validator,
        IDownloadService download,
        IConsoleWriter console,
        IConfigurationManager configurationManager)
    {
        _api = api;
        _validator = validator;
        _download = download;
        _console = console;
        _configurationManager = configurationManager;
    }

    public async Task RunAsync(GetOptions opts)
    {
        var validationResult = await _validator.ValidateAsync(opts);
        if (!validationResult.IsValid)
        {
            _console.ErrorLine("Invalid gallery code.");
            return;
        }

        var response = await _api.FetchSingleAsync(opts.Input.ToString());
        _console.WriteLine(response.ToReadable());

        if (opts.ReadOnly)
        {
            return;
        }

        var useTachiyomiLayout = opts.UseTachiyomiLayout || _configurationManager.Values.UseTachiyomiLayout;
        await _download.DownloadAsync(response, opts.Output, opts.Pack, useTachiyomiLayout, null);
    }
}
