using System.Collections.Generic;
using System.Threading.Tasks;

namespace asuka.Configuration;

public interface IAppConfigManager
{
    void SetValue(string key, string value);
    string GetValue(string key);
    IReadOnlyList<(string, string)> GetAllValues();
    Task Reset();
    Task Flush();
}
