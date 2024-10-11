using asuka.Provider.Koharu.Contracts.Responses;
using asuka.ProviderSdk;

namespace asuka.Provider.Koharu.Mappers;

internal static class GalleryInfoToSeries
{
    public static Series ToSeries(this GalleryInfoResponse response, GalleryContentsResponse contents, int width)
    {
        var data = new Series
        {
            Title = response.Title,
            Artists = response.Tags
                .Where(tag => tag.Namespace == GalleryTag.Artist)
                .Select(tag => tag.Name)
                .ToList(),
            Authors = [],
            Genres = response.Tags
                .Where(tag => tag.Namespace != GalleryTag.Artist)
                .Select(tag => tag.Name)
                .ToList(),
            Chapters =
                [
                    new Chapter
                    {
                        Id = 1,
                        Pages = contents.ToChapterImages(width)
                    }
                ]
        };
        
        return data;
    }

    private static List<Chapter.ChapterImages> ToChapterImages(this GalleryContentsResponse contents, int width)
    {
        var chapterImages = new List<Chapter.ChapterImages>();

        var pages = contents.Entries.Count;
        for (var i = 0; i < pages; i++)
        {
            var path = contents.Entries[i].Path;
            
            var filenameFormat = (i + 1).ToString($"D{pages.ToString().Length}");
            var extension = Path.GetExtension(path);
            
            chapterImages.Add(new Chapter.ChapterImages
            {
                Filename = $"{filenameFormat}{extension}",
                ImageRemotePath = $"{contents.Base}{path}?w={width}"
            });
        }
        
        return chapterImages;
    }
}
