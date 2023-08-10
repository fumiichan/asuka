using asuka.Sdk.Providers.Models;

namespace asuka.Sdk.Providers.Extensions;

public static class GalleryTitleResultExtensions
{
    public static string GetTitle(this GalleryTitleResult result)
    {
        if (!string.IsNullOrEmpty(result.Japanese))
        {
            return result.Japanese;
        }
        if (!string.IsNullOrEmpty(result.English))
        {
            return result.English;
        }
        return !string.IsNullOrEmpty(result.Pretty) ? result.Pretty : "Unknown title";
    }
    
    public static string GetTitle(this GalleryTitleResult result, string language)
    {
        if (!string.IsNullOrEmpty(result.Japanese) && language == "Japanese")
        {
            return result.Japanese;
        }
        if (!string.IsNullOrEmpty(result.English) && language == "English")
        {
            return result.English;
        }
        if (!string.IsNullOrEmpty(result.Pretty) && language == "Pretty")
        {
            return result.Pretty;
        }

        return result.GetTitle();
    }
}
