namespace asuka.Configuration;

public record CookieMetadata
{
    public string Name { get; set; }
    public string Value { get; set; }
    public string Domain { get; set; }
    public bool HttpOnly { get; set; }
    public bool Secure { get; set; }
}

public record CookieStore
{
    public CookieMetadata CloudflareClearance { get; set; }
    public CookieMetadata CsrfToken { get; set; }
}

public record RequestOptions
{
    public CookieStore Cookies { get; set; }
    public string UserAgent { get; set; }
}

public record Addresses
{
    public string ApiBaseAddress { get; set; }
    public string ImageBaseAddress { get; set; }
}

public record ApplicationSettingsModel
{
    public Addresses BaseAddresses { get; set; }
    public RequestOptions RequestOptions { get; set; }
}
