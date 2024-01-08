using asuka.Core.Models;

namespace asuka.Core.Extensions;

public static class GalleryTitleResultExtension
{
    public static string GetTitle(this GalleryTitleResult title)
    {
        if (!string.IsNullOrEmpty(title.Japanese))
        {
            return title.Japanese;
        }
        if (!string.IsNullOrEmpty(title.English))
        {
            return title.English;
        }
        return !string.IsNullOrEmpty(title.Pretty) ? title.Pretty : "Unknown title";
    }

    public static string? GetTitleByLanguage(this GalleryTitleResult title, TitleLanguages language)
    {
        if (!string.IsNullOrEmpty(title.Japanese) && language == TitleLanguages.Japanese)
        {
            return title.Japanese;
        }
        if (!string.IsNullOrEmpty(title.English) && language == TitleLanguages.English)
        {
            return title.English;
        }

        if (!string.IsNullOrEmpty(title.Pretty) && language == TitleLanguages.Pretty)
        {
            return title.Pretty;
        }

        return null;
    }
}