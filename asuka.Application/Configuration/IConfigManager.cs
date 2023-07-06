using System.Collections.Generic;
using System.Threading.Tasks;

namespace asuka.Application.Configuration;

public interface IConfigManager
{
    void SetValue(string key, string value);
    string GetValue(string key);
    IEnumerable<(string, string)> GetAllValues();
    Task Reset();
    Task Flush();
}
