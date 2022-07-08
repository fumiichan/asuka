using System.Net;

namespace asuka.Configuration;

public class CloudflareBypass : ConfigurationManager
{
    public Cookie CloudflareClearance
    {
        get
        {
            if (!string.IsNullOrEmpty(Configuration.CloudflareClearance))
                return new Cookie("cf_clearance", Configuration.CloudflareClearance)
                {
                    Domain = ".nhentai.net",
                    HttpOnly = true,
                    Secure = true
                };
            return null;
        }
    }

    public Cookie CsrfToken
    {
        get
        {
            if (!string.IsNullOrEmpty(Configuration.CsrfToken))
                return new Cookie("csrftoken", Configuration.CsrfToken)
                {
                    Domain = "nhentai.net",
                    HttpOnly = false,
                    Secure = false
                };
            return null;
        }
    }

    public string UserAgent => !string.IsNullOrEmpty(Configuration.UserAgent)
        ? Configuration.UserAgent
        : "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36";
}
