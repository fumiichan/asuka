using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using asuka.Application.Commandline.Options;
using asuka.Application.Commandline.Parsers.Common;
using asuka.Application.Output.ConsoleWriter;
using asuka.Application.Output.Progress;
using asuka.Application.Services;
using asuka.Application.Services.Configuration;
using asuka.Application.Utilities;
using asuka.Core.Chaptering;
using asuka.Core.Downloader;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace asuka.Application.Commandline.Parsers;

public class FileCommandService : ICommandLineParser
{
    private readonly ProviderResolverService _provider;
    private readonly IProgressProviderFactory _progressFactory;
    private readonly IValidator<FileCommandOptions> _validator;
    private readonly ILogger<FileCommandService> _logger;
    private readonly IConsoleWriter _console;
    private readonly AsukaConfiguration _config;

    public FileCommandService(
        ProviderResolverService provider,
        IProgressProviderFactory progressFactory,
        IValidator<FileCommandOptions> validator,
        ILogger<FileCommandService> logger,
        IConsoleWriter console,
        AsukaConfiguration config)
    {
        _provider = provider;
        _progressFactory = progressFactory;
        _validator = validator;
        _logger = logger;
        _console = console;
        _config = config;
    }

    public async Task Run(object options)
    {
        var opts = (FileCommandOptions)options;
        _logger.LogInformation("FileCommandService called with args: {@Opts}", opts);
        
        var validationResult = await _validator.ValidateAsync(opts);
        if (!validationResult.IsValid)
        {
            _logger.LogError("FileCommandService failed with errors: {@Errors}", validationResult.Errors);

            validationResult.Errors.PrintErrors(_console);
            return;
        }

        await ExecuteCommand(opts);
    }

    private async Task ExecuteCommand(FileCommandOptions opts)
    {
        try
        {
            var textFile = await File.ReadAllLinesAsync(opts.FilePath, Encoding.UTF8)
                .ConfigureAwait(false);
            
            var progress = _progressFactory.Create(textFile.Length, "downloading from text file...");
            _logger.LogInformation("Text file is loaded with total length of {Length}", textFile.Length);
            
            foreach (var url in textFile)
            {
                _logger.LogInformation("Download started for {Url}", url);
                await DownloadEach(opts, url, progress);
            }

            progress.Close();
        }
        catch (Exception e)
        {
            _logger.LogError("Unable to read file due to an exception: {@Exception}", e);
            _console.WriteError("File cannot be loaded. It maybe located at inaccessable path or does not exist.");
        }
    }

    private async Task DownloadEach(FileCommandOptions opts, string url, IProgressProvider progress)
    {
        // Allows dynamic provider switching to allow multiple providers to be in single file
        var provider = _provider.GetProviderByUrl(url);
        if (provider is null)
        {
            _logger.LogError("Unable to find provider of {Url}", url);

            SpawnProgressForFailure(progress, url);
            progress.Tick();
            return;
        }
        
        var response = await provider.Api.FetchSingle(url);
        if (response is null)
        {
            _logger.LogInformation("Api request failed for url {Url}", url);
            
            SpawnProgressForFailure(progress, url);
            return;
        }
 
        _logger.LogInformation("Api provider fetch success: {@Response}", response);
        var series = new SeriesBuilder()
            .AddChapter(response, provider.ImageApi)
            .SetOutput(opts.Output)
            .Build(_config.DefaultLanguageTitle.ToString());

        _logger.LogInformation("Series data built: {@Series}", series);

        // Create progress bar
        var internalProgress = progress.Spawn(response.TotalPages, $"downloading: {response.Id}");

        var downloader = new DownloaderBuilder()
            .AttachLogger(_logger)
            .SetChapter(series.Chapters[0])
            .SetOutput(series.Output)
            .SetEachCompleteHandler(e =>
            {
                internalProgress.Tick($"{e.Message}: {response.Id}");
            })
            .Build();

        // Start downloading
        await downloader.Start();

        // Compression
        if (opts.Pack)
        {
            await CompressAction.Compress(series, opts.Output, internalProgress, _logger);
        }

        // Cleanup
        progress.Tick();
        internalProgress.Close();
    }

    private void SpawnProgressForFailure(IProgressProvider progress, string url)
    {
        var failProgress = progress.Spawn(1, $"skipped due to error: {url}");
        failProgress.Tick();
        failProgress.Close();
    }
}
