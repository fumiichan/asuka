using asuka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using asuka.Installers;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var services = new ServiceCollection();
services.InstallServices();
services.InstallRefitServices(configuration);
var serviceProvider = services.BuildServiceProvider();

var app = serviceProvider.GetRequiredService<AsukaApplication>();
await app.RunAsync(args);
