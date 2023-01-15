using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace asuka.Installers;

public interface IInstaller
{
    void ConfigureService(IServiceCollection services, IConfiguration configuration);
}
