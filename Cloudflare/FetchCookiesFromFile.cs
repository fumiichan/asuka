using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using Console = Colorful.Console;

namespace asuka.Cloudflare;

public class FetchCookiesFromFile
{
    private readonly string _cloudflareClearance;
    private readonly string _csrfToken;
    private readonly string _session;

    private FetchCookiesFromFile(ICookieData data)
    {
        _cloudflareClearance = data.CloudflareClearance;
        _csrfToken = data.CsrfToken;

        if (!string.IsNullOrEmpty(data.Session))
        {
            _session = data.Session;
        }
    }

    private static Cookie CraftCookie(string name,
        string value,
        string domain = "nhentai.net",
        bool httpOnly = true,
        bool secure = true)
    {
        return new Cookie(name, value)
        {
            Domain = domain,
            HttpOnly = httpOnly,
            Secure = secure
        };
    }

    public Cookie getCloudflareClearance()
    {
        if (!string.IsNullOrEmpty(this._cloudflareClearance))
            return CraftCookie("cf_clearance", _cloudflareClearance, ".nhentai.net");
        Console.WriteLine("No cf_clearance cookie. Downloading may fail.", Color.Yellow);
        return null;
    }

    public Cookie getCsrfToken()
    {
        if (!string.IsNullOrEmpty(this._csrfToken))
            return CraftCookie("csrftoken", _csrfToken, secure: false, httpOnly: false);
        Console.WriteLine("No csrftoken cookie. Downloading may fail.", Color.Yellow);
        return null;
    }

    public Cookie getSession()
    {
        return !string.IsNullOrEmpty(this._csrfToken) ? CraftCookie("sessionid", _session, secure: false) : null;
    }

    public static FetchCookiesFromFile load()
    {
        var file = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ".asuka/cookies.txt");
        if (!File.Exists(file))
        {
            Console.WriteLine("File to load cookies cannot be found. Your requests might fail");
            return null;
        }

        var cookies = File.ReadAllText(file, Encoding.UTF8);
        var cookieData = new CookieData();

        foreach (var line in cookies.Split('\n'))
        {
            var data = line.Split();

            switch (data[5])
            {
                case "cf_clearance":
                    cookieData.CloudflareClearance = data[6];
                    break;
                case "csrftoken":
                    cookieData.CsrfToken = data[6];
                    break;
                case "sessionid":
                    cookieData.Session = data[6];
                    break;
            }
        }

        if (!string.IsNullOrEmpty(cookieData.CloudflareClearance) || !string.IsNullOrEmpty(cookieData.CsrfToken))
            return new FetchCookiesFromFile(cookieData);
        Console.WriteLine("Cookies found but no cf_clearance and csrftoken found. Ignoring cookies...");
        return null;
    }
}
