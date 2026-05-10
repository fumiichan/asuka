using asuka.Provider.Nhentai.Contracts;
using asuka.Provider.Nhentai.Extensions;
using asuka.Provider.Sdk;

namespace asuka.Provider.Nhentai.Mappers;

internal static class GalleryResponseToSeriesMapper
{
    public static Series ToSeries(this GalleryResponse response)
    {
        var artists = response.Tags
            .Where(x => x.Type == "artist")
            .Select(x => x.Name)
            .ToList();
        var tags = response.Tags
            .Where(x => x.Type == "tag")
            .Select(x => x.Name)
            .ToList();

        return new Series
        {
            Title = response.GetTitle(),
            Artists = artists,
            Authors = artists,
            Genres = tags,
            Chapters =
            [
                new Chapter
                {
                    Id = 1,
                    Pages = response.Pages
                        .Select((x, i) =>
                        {
                            var fileName = Path.GetFileName(x.Path);
                            fileName = fileName.PadLeft(response.NumPages.ToString().Length, '0');
                            
                            return new ChapterImage
                            {
                                RemotePath = x.Path,
                                Filename = fileName,
                            };
                        })
                        .ToList()
                }
            ],
            Status = SeriesStatus.Completed
        };
    }
}
