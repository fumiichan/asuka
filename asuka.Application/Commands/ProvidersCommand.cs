using System;
using asuka.Application.Services.ProviderManager;
using Cocona;
using Spectre.Console;

namespace asuka.Application.Commands;

internal sealed class ProvidersCommand
{
    private readonly IProviderManager _provider;

    public ProvidersCommand(IProviderManager provider)
    {
        _provider = provider;
    }

    [Command("provider", Description = "Lists/Manages the providers currently installed")]
    public void Run()
    {
        var table = new Table();
        table.AddColumn("Provider ID");
        table.AddColumn("Version");
        table.AddColumn("Aliases");
        
        foreach (var provider in _provider.GetAllRegisteredProviders())
        {
            table.AddRow(
                Markup.Escape(provider.Id),
                Markup.Escape(provider.Version.ToString()),
                Markup.Escape(string.Join(", ", provider.Aliases)));
        }
        
        AnsiConsole.Write(table);
    }
}
