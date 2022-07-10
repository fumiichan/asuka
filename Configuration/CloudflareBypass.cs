using System.Linq;
using System.Net;

namespace asuka.Configuration;

public class CloudflareBypass : ConfigurationManager
{
    public Cookie GetCookieByName(string name)
    {
        var cookie = Configuration.Cookies?.FirstOrDefault(x => x.Name == name);
        
        if (cookie == null)
        {
            return null;
        }

        return new Cookie(cookie.Name, cookie.Value)
        {
            Domain = cookie.Domain,
            HttpOnly = cookie.HttpOnly,
            Secure = cookie.Secure
        };
    }

    public string UserAgent => !string.IsNullOrEmpty(Configuration.UserAgent)
        ? Configuration.UserAgent
        : "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36";
}
