using System.Text.Json.Serialization;

namespace asuka.Application.Services.Configuration;

public enum ProgressTypes
{
    Progress,
    Text,
    Stealth
}

public enum MangaLanguages
{
    Japanese,
    English,
    Pretty
}

public class AsukaConfiguration
{
    public ProgressTypes ProgressType { get; set; } = ProgressTypes.Progress;
    public MangaLanguages DefaultLanguageTitle { get; set; } = MangaLanguages.Japanese;
}
