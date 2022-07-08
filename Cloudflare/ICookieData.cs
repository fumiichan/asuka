namespace asuka.Cloudflare;

public interface ICookieData
{
    public string CloudflareClearance { get; set; }
    public string CsrfToken { get; set; }
    public string Session { get; set; }
}
