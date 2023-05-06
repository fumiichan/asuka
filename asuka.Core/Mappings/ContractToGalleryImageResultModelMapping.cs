using asuka.Api.Responses;
using asuka.Core.Models;

namespace asuka.Core.Mappings;

public static class ContractToGalleryImageResultModelMapping
{
    public static IReadOnlyList<GalleryImageResult> ToGalleryImageResult(
        this IReadOnlyList<GalleryImageResponse> response)
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
                ServerFilename = $"{pageNumber}{extension}",
                Filename = filename
            };
        }).ToList();
    }
}
