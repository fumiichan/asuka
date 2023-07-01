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
        var listOfProvider = providers.ToList();
        if (!string.IsNullOrEmpty(url))
        {
            var urlMatchingProvider = listOfProvider.GetFirstByHostname(url);
            if (urlMatchingProvider is not null)
            {
                return urlMatchingProvider;
            }

            return listOfProvider.GetFirst(defaultedProvider);
        }

        return null;
    }
}
