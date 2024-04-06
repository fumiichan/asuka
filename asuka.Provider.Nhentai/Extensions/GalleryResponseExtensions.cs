using asuka.Provider.Nhentai.Contracts;

namespace asuka.Provider.Nhentai.Extensions;

internal static class GalleryResponseExtensions
{
    public static string GetTitle(this GalleryResponse gallery)
    {
        if (!string.IsNullOrEmpty(gallery.Title.Japanese))
        {
            return gallery.Title.Japanese;
        }

        if (!string.IsNullOrEmpty(gallery.Title.English))
        {
            return gallery.Title.English;
        }

        return string.IsNullOrEmpty(gallery.Title.Pretty)
            ? $"{gallery.Id} - Unknown title"
            : gallery.Title.Pretty;
    }
}
