using System.Threading.Tasks;

namespace asuka.Configuration;

public interface IConfigurationManager
{
    void ToggleTachiyomiLayout(bool value);
    void Reset();
    void ChangeColourTheme(string value);
    Task Flush();
    ConfigurationData Values { get; }
}
