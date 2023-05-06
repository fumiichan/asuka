using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace asuka.Application.Configuration;

public class RequestConfigurator : IRequestConfigurator
{
    private readonly string _appSettingsPath;

    public RequestConfigurator()
    {
        var assemblyDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
        _appSettingsPath = Path.Combine(assemblyDir, "appsettings.json");
    }

    private async Task<ApplicationSettingsModel> ReadSettings()
    {
        var file = await File.ReadAllTextAsync(_appSettingsPath);
        return JsonSerializer.Deserialize<ApplicationSettingsModel>(file);
    }

    private async Task WriteSettings(ApplicationSettingsModel settings)
    {
        var jsonConfig = JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        await File.WriteAllTextAsync(_appSettingsPath, jsonConfig);
    }
    
    public async Task ApplyCookies(CookieDump clearance, CookieDump csrf)
    {
        var config = await ReadSettings();

        config.RequestOptions.Cookies.CloudflareClearance = new CookieMetadata
        {
            Name = clearance.Name,
            Domain = clearance.Domain,
            HttpOnly = clearance.HttpOnly,
            Secure = clearance.Secure,
            Value = clearance.Value
        };

        config.RequestOptions.Cookies.CsrfToken = new CookieMetadata
        {
            Name = csrf.Name,
            Domain = csrf.Domain,
            HttpOnly = csrf.HttpOnly,
            Secure = csrf.Secure,
            Value = csrf.Value
        };

        await WriteSettings(config);
    }

    public async Task ApplyUserAgent(string userAgent)
    {
        var config = await ReadSettings();
        
        config.RequestOptions.UserAgent = userAgent;

        await WriteSettings(config);
    }

    public async Task ChangeBaseAddresses(string apiEndpoint, string imageEndpoint)
    {
        var config = await ReadSettings();

        config.BaseAddresses = new Addresses
        {
            ApiBaseAddress = apiEndpoint,
            ImageBaseAddress = imageEndpoint
        };

        await WriteSettings(config);
    }
}
