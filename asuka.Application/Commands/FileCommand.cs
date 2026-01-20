// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using asuka.Application.Services.Downloader;
using asuka.Application.Services.ProviderManager;
using asuka.Application.Validators;
using asuka.Provider.Sdk;
using Cocona;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace asuka.Application.Commands;

internal sealed class FileCommand : CoconaConsoleAppBase
{
    private readonly IProviderManager _provider;
    private readonly IDownloaderBuilder _builder;
    private readonly ILogger<FileCommand> _logger;

    public FileCommand(IProviderManager provider, IDownloaderBuilder builder, ILogger<FileCommand> logger)
    {
        _provider = provider;
        _builder = builder;
        _logger = logger;
    }

    [Command("file", Aliases = ["f"], Description = "Download galleries from yaml (.yml) document")]
    public async Task RunAsync(
        [Argument(Description = "Path to the yaml (.yml) file to read")]
        [PathExists]
        [FileWithinSizeLimits]
        string file,
        
        [Option("pack", ['p'], Description = "Compress downloads to a CBZ archive")]
        bool pack,
        
        [Option("output", ['o'], Description = "Specify destination path for downloads")]
        string? output)
    {
        await AnsiConsole.Status()
            .StartAsync("Running...", async ctx =>
            {
                ctx.Status("Reading yaml document...");
                var lines = await Document.ReadAsync(file);
                
                // Loop through the jobs if possible
                foreach (var job in lines.Jobs)
                {
                    var client = _provider.GetProviderByAlias(job.Provider);
                    if (client == null)
                    {
                        _logger.LogWarning("Provider alias not found: {alias}", job.Provider);
                        
                        AnsiConsole.MarkupLine("[orange1]Unsupported: {0}[/]", Markup.Escape(job.Provider));
                        continue;
                    }
                    
                    // Build series
                    ctx.Status("Fetching chapters...");

                    try
                    {
                        var queue = new List<Series>();
                        foreach (var url in job.Urls)
                        {
                            Context.CancellationToken.ThrowIfCancellationRequested();
                        
                            var response = await client.GetSeries(url, Context.CancellationToken);
                            queue.Add(response);
                        }
                    
                        var series = new Series
                        {
                            Title = queue[0].Title,
                            Artists = queue[0].Artists,
                            Authors = queue[0].Authors,
                            Genres = queue[0].Genres,
                            Chapters = [],
                            Status = queue[0].Status
                        };
                    
                        var counter = 1;
                        foreach (var item in queue)
                        {
                            foreach (var chapter in item.Chapters)
                            {
                                series.Chapters.Add(new Chapter
                                {
                                    Id = counter,
                                    Pages = chapter.Pages,
                                });
                                counter++;
                            }
                        }
                    
                        ctx.Status("Starting download...");
                    
                        var instance = _builder.CreateDownloaderInstance(client, series);
                        instance.Configure(c =>
                        {
                            c.OutputPath = output;
                            c.Pack = pack;
                        });
                        instance.OnProgress = m => ctx.Status(Markup.Escape(m));
                        await instance.Start();
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Failed to fetch gallery with exception: {ex}", ex);
                        AnsiConsole.MarkupLine("[red3_1]Failed to fetch gallery. See logs for more details.[/]");
                    }
                }
                
                AnsiConsole.MarkupLine("[chartreuse1]All jobs finished.[/]");
            });
    }
}

internal sealed class Document
{
    public List<Job> Jobs { get; set; } = [];

    internal sealed class Job
    {
        public List<string> Urls { get; set; } = [];
        public string Provider { get; set; } = string.Empty;
    }

    public static async Task<Document> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var file = await File.ReadAllTextAsync(path, cancellationToken);
        
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        return deserializer.Deserialize<Document>(file);
    }
}
