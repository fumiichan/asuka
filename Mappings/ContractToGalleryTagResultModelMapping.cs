using System.Collections.Generic;
using System.Linq;
using asuka.Api.Responses;

namespace asuka.Mappings
{
    public static class ContractToGalleryTagResultModelMapping
    {
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
}
