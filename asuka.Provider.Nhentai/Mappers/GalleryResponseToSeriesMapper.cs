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
                    Pages = response.Images.Pages
                        .Select((x, i) =>
                        {
                            var pageNumber = i + 1;
                            var extension = x.Format switch
                            {
                                "j" => ".jpg",
                                "p" => ".png",
                                "g" => ".gif",
                                "w" => ".webp",
                                _ => ""
                            };

                            var pageNumberFormatted = pageNumber.ToString($"D{response.TotalPages.ToString().Length}");
                            var filename = $"{pageNumberFormatted}{extension}";
                            
                            return new ChapterImage
                            {
                                RemotePath = $"{response.MediaId}/{pageNumber}{extension}",
                                Filename = filename
                            };
                        })
                        .ToList()
                }
            ],
            Status = SeriesStatus.Completed
        };
    }
}
