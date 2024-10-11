using System;
using asuka.Application.Services.ProviderManager;
using Cocona;

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
        foreach (var (id, version) in _provider.GetAllRegisteredProviders())
        {
            Console.WriteLine($"{id}\t\t{version}");
        }
    }
}
