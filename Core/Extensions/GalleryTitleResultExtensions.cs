using asuka.Core.Models;

namespace asuka.Core.Extensions;

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
}
