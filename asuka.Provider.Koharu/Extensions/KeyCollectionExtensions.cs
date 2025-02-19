using asuka.Provider.Koharu.Contracts.Responses;

namespace asuka.Provider.Koharu.Extensions;

internal static class KeyCollectionExtensions
{
    public static string FindHighestKey(
        this Dictionary<string, GalleryInfoResponse.GalleryImageDetails>.KeyCollection keys)
    {
        var highest = 0;
        foreach (var key in keys)
        {
            if (!int.TryParse(key, out var value))
            {
                continue;
            }

            if (value > highest)
            {
                highest = value;
            }
        }

        return highest.ToString();
    }
}
