using System.Collections.Generic;
using System.Linq;
using asuka.Core.Models;
using Sharprompt;

namespace asuka.Core.Mappings;

public static class ContractToUserSelectedModelMapping
{
    public static IReadOnlyList<GalleryResult> FilterByUserSelected(
        this IReadOnlyList<GalleryResult> response)
    {
        var selection = Prompt.MultiSelect("Select to download", response, response.Count,
            textSelector: (result) =>
            {
                var title = string.IsNullOrEmpty(result.Title.Japanese)
                    ? (string.IsNullOrEmpty(result.Title.English) ? result.Title.Pretty : result.Title.English)
                    : result.Title.Japanese;
                return title;
            });

        return selection.ToList();
    }
}
