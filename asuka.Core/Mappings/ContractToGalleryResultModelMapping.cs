using asuka.Api.Responses;
using asuka.Core.Models;

namespace asuka.Core.Mappings;

public static class ContractToGalleryResultModelMapping
{
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
            Images = response.Images.Images.ToGalleryImageResult(),
            Artists = response.Tags.GetTagByGroup("artist"),
            Parodies = response.Tags.GetTagByGroup("parody"),
            Characters = response.Tags.GetTagByGroup("character"),
            Tags = response.Tags.GetTagByGroup("tag"),
            Categories = response.Tags.GetTagByGroup("category"),
            Languages = response.Tags.GetTagByGroup("language"),
            TotalPages = response.TotalPages
        };
    }
}
