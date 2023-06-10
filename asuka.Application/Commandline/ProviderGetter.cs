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
            .FirstOrDefault(x => x.ProviderFor() == proivderName);
    }
}
