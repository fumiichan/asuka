using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentValidation;
using ShellProgressBar;
using asuka.CommandOptions;
using asuka.Configuration;
using asuka.Downloader;
using asuka.Mappings;
using asuka.Output;
using asuka.Services;
using asuka.Utils;
using Microsoft.Extensions.Configuration;

namespace asuka.CommandParsers;

public class RecommendCommandService : IRecommendCommandService
{
    private readonly IValidator<IRequiresInputOption> _validator;
    private readonly IGalleryRequestService _api;
    private readonly IDownloadService _download;
    private readonly IConsoleWriter _console;
    private readonly IConfigurationManager _configurationManager;

    public RecommendCommandService(
        IValidator<IRequiresInputOption> validator,
        IGalleryRequestService api,
        IDownloadService download,
        IConsoleWriter console,
        IConfigurationManager configurationManager)
    {
        _validator = validator;
        _api = api;
        _download = download;
        _console = console;
        _configurationManager = configurationManager;
    }

    public async Task RunAsync(RecommendOptions opts)
    {
        var validator = await _validator.ValidateAsync(opts);
        if (!validator.IsValid)
        {
            _console.ValidationErrors(validator.Errors);
            return;
        }

        var responses = await _api.FetchRecommendedAsync(opts.Input.ToString());

        var selection = responses.FilterByUserSelected();

        // Initialise the Progress bar.
        using var progress = new ProgressBar(selection.Count, $"[task] recommend from id: {opts.Input}",
            ProgressBarConfiguration.BarOption);

        var useTachiyomiLayout = opts.UseTachiyomiLayout || _configurationManager.Values.UseTachiyomiLayout;

        foreach (var response in selection)
        {

            await _download.DownloadAsync(response, opts.Output, opts.Pack, useTachiyomiLayout, progress);
            progress.Tick();
        }
    }
}
