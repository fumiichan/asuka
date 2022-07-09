using System.Threading.Tasks;

namespace asuka.Configuration;

public interface IConfigurationManager
{
    Task SetCookiesAsync(string path);
    Task SetUserAgentAsync(string userAgent);
    Task ToggleTachiyomiLayoutAsync(bool value);
    ConfigurationData Values { get; }
}
