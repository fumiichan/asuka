using System;
using System.Threading.Tasks;
using asuka.Commandline;
using asuka.Commandline.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using asuka.Installers;
using CommandLine;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var services = new ServiceCollection();
services.InstallServicesInAssembly(configuration);
var serviceProvider = services.BuildServiceProvider();

async Task RunCommand(object options, string serviceKey)
{
    var service = serviceProvider.GetKeyedService<ICommandLineParser>($"Cmd_{serviceKey}");
    if (service == null)
    {
        Console.WriteLine($"Unknown command");
        return;
    }

    await service.RunAsync(options);
}

var parser = Parser.Default
    .ParseArguments<GetOptions, RecommendOptions, SearchOptions, RandomOptions, FileCommandOptions, ConfigureOptions, SeriesCreatorCommandOptions, CookieConfigureOptions>(args);

try
{
    await parser.MapResult(
        async (GetOptions opts) => { await RunCommand(opts, "Get"); },
        async (RecommendOptions opts) => { await RunCommand(opts, "Recommend"); },
        async (SearchOptions opts) => { await RunCommand(opts, "Search"); },
        async (RandomOptions opts) => { await RunCommand(opts, "Random"); },
        async (FileCommandOptions opts) => { await RunCommand(opts, "File"); },
        async (ConfigureOptions opts) => { await RunCommand(opts, "Configure"); },
        async (SeriesCreatorCommandOptions opts) => { await RunCommand(opts, "Series"); },
        async (CookieConfigureOptions opts) => { await RunCommand(opts, "Cookie"); },
        errors =>
        {
            foreach (var error in errors)
            {
                Console.WriteLine($"An error occured. Type: {error.Tag}");
            }

            return Task.FromResult(1);
        });

    return 0;
}
catch (Exception e)
{
    Console.WriteLine($"An error occured. Message: {e.Message}");
    return 1;
}
