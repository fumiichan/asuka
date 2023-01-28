using System.Threading.Tasks;

namespace asuka.Configuration;

public interface IConfigurationManager
{
    Task SetCookies(string path);
    void SetUserAgent(string userAgent);
    void ToggleTachiyomiLayout(bool value);
    void Reset();
    void ChangeColourTheme(string value);
    Task Flush();
    ConfigurationData Values { get; }
}
