using System.Collections.Generic;
using System.Linq;
using asuka.Core.Extensions;
using asuka.Core.Models;
using Sharprompt;

namespace asuka.Core.Mappings;

public static class ContractToUserSelectedModelMapping
{
    public static IReadOnlyList<GalleryResult> FilterByUserSelected(
        this IReadOnlyList<GalleryResult> response)
    {
        var selection = Prompt.MultiSelect("Select to download", response, response.Count,
            textSelector: result => result.Title.GetTitle());

        return selection.ToList();
    }
}
