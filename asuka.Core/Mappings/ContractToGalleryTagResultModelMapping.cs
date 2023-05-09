using asuka.Api.Responses;

namespace asuka.Core.Mappings;

public static class ContractToGalleryTagResultModelMapping
{
    public static IReadOnlyList<string> GetTagByGroup(
        this IEnumerable<GalleryTagResponse> response,
        string filter)
    {
        // In cases where tags are not present.
        if (response == null)
        {
            return new List<string>();
        }

        return response
            .Where(x => x.Type == filter)
            .Select(x => x.Name)
            .ToList();
    }
}
