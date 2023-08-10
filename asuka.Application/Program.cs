using asuka.Application;
using asuka.Application.Services;
using asuka.Application.Services.Configuration;
using asuka.ProviderDependencyInjectionExtensions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.InstallConfigurationAsService();
services.InstallLogger();
services.InstallCoreModules();
services.InstallExternalProviders();
services.AddSingleton<AsukaApplication>();
services.InstallCommandParsers();
services.AddValidatorsFromAssemblyContaining<Program>();

var serviceProvider = services.BuildServiceProvider();

var app = serviceProvider.GetRequiredService<AsukaApplication>();

await app.RunAsync(args);
