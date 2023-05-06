namespace asuka.Core.Configuration;

public interface IConfigurationManager
{
    void SetValue(string key, string value);
    string GetValue(string key);
    IReadOnlyList<(string, string)> GetAllValues();
    Task Reset();
    Task Flush();
}
