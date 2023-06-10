using asuka.Core.Models;
using asuka.Providers.Nhentai.Api.Responses;

namespace asuka.Providers.Nhentai.Contracts;

public static class ApiContracts
{
    /// <summary>
    /// Contract for transfoming GalleryImageResult into GalleryImageResponse to be
    /// consumed by the Core.
    /// </summary>
    /// <param name="response"></param>
    /// <param name="mediaId"></param>
    /// <returns></returns>
    public static IReadOnlyList<GalleryImageResult> ToGalleryImageResult(
        this IReadOnlyList<GalleryImageResponse> response, string mediaId)
    {
        return response.Select((value, index) =>
        {
            var extension = value.Type switch
            {
                "j" => ".jpg",
                "p" => ".png",
                "g" => ".gif",
                _ => ""
            };

            var pageNumber = index + 1;
            var pageNumberFormatted = pageNumber.ToString($"D{response.Count.ToString().Length}");
            var filename = $"{pageNumberFormatted}{extension}";

            return new GalleryImageResult
            {
                ServerFilename = $"{mediaId}/{pageNumber}{extension}",
                Filename = filename
            };
        }).ToList();
    }
    
    /// <summary>
    /// Contract for transforming GalleryResponse to GalleryResult to be consumed by the
    /// Core services.
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public static GalleryResult ToGalleryResult(this GalleryResponse response)
    {
        return new GalleryResult
        {
            Id = response.Id,
            MediaId = response.MediaId,
            Title = new GalleryTitleResult
            {
                Japanese = response.Title.Japanese,
                English = response.Title.English,
                Pretty = response.Title.Pretty
            },
            Images = response.Images.Images.ToGalleryImageResult(response.MediaId.ToString()),
            Artists = response.Tags.GetTagByGroup("artist"),
            Parodies = response.Tags.GetTagByGroup("parody"),
            Characters = response.Tags.GetTagByGroup("character"),
            Tags = response.Tags.GetTagByGroup("tag"),
            Categories = response.Tags.GetTagByGroup("category"),
            Languages = response.Tags.GetTagByGroup("language"),
            TotalPages = response.TotalPages
        };
    }

    /// <summary>
    /// Filters a specific tag and transforms into list of tag names instead of Tag objects.
    /// Useful for displaying information or exporting details into JSON file.
    /// </summary>
    /// <param name="response"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    public static IReadOnlyList<string> GetTagByGroup(
        this IEnumerable<GalleryTagResponse> response,
        string filter)
    {
        return response
            .Where(x => x.Type == filter)
            .Select(x => x.Name)
            .ToList();
    }
}
