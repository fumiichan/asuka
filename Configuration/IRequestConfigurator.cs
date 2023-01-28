using System.Threading.Tasks;

namespace asuka.Configuration;

public interface IRequestConfigurator
{
    Task ApplyCookies(CookieDump clearance, CookieDump csrf);
    Task ApplyUserAgent(string userAgent);
    Task ChangeBaseAddresses(string apiEndpoint, string imageEndpoint);
}
