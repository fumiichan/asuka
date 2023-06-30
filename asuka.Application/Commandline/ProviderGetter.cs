using System.Collections.Generic;
using System.Linq;
using asuka.Core.Requests;

namespace asuka.Application.Commandline;

public static class ProviderGetter
{
    public static IGalleryRequestService GetFirst(this IEnumerable<IGalleryRequestService> providers,
        string proivderName)
    {
        return providers
            .FirstOrDefault(x => x.ProviderFor().For == proivderName);
    }

    public static IGalleryRequestService GetFirstByHostname(this IEnumerable<IGalleryRequestService> providers,
        string url)
    {
        return providers
            .FirstOrDefault(x => url.StartsWith(x.ProviderFor().Base));
    }

    public static IGalleryRequestService GetWhatMatches(this IEnumerable<IGalleryRequestService> providers,
        string url,
        string defaultedProvider)
    {
        if (!string.IsNullOrEmpty(url))
        {
            return providers.GetFirstByHostname(url);
        }

        if (!string.IsNullOrEmpty(defaultedProvider))
        {
            return providers.GetFirst(defaultedProvider);
        }

        return null;
    }
}
