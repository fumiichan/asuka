using System;
using System.Linq;
using asuka.Application;
using asuka.Application.Services;
using asuka.Core.Requests;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.InstallLogger();
services.InstallCoreModules();
services.InstallExternalProviders();
services.AddSingleton<AsukaApplication>();
services.InstallCommandParsers();
services.AddValidatorsFromAssemblyContaining<Program>();

var serviceProvider = services.BuildServiceProvider();

var app = serviceProvider.GetRequiredService<AsukaApplication>();

// Count all providers available
var imageRequestServices = serviceProvider.GetServices<IGalleryImageRequestService>();
var apiRequestServices = serviceProvider.GetServices<IGalleryRequestService>();

if (!imageRequestServices.Any() || !apiRequestServices.Any())
{
    Console.WriteLine("No provider services found. Exiting...");
    return;
}

await app.RunAsync(args);
