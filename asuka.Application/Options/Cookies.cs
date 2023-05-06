using System.Net;

namespace asuka.Application.Options;

public class Cookies
{
    public Cookie CsrfToken { get; init; }
    public Cookie Cloudflare { get; init; }
}
