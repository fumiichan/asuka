using asuka.Application;
using asuka.Application.Installers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var services = new ServiceCollection();
services.InstallServicesInAssembly(configuration);
var serviceProvider = services.BuildServiceProvider();

var app = serviceProvider.GetRequiredService<AsukaApplication>();
await app.RunAsync(args);
