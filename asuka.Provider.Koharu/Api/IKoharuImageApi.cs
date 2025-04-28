using Refit;

namespace asuka.Provider.Koharu.Api;

internal interface IKoharuImageApi
{
    /// <summary>
    /// Retrieves the image
    /// </summary>
    /// <param name="route">Route to the image</param>
    /// <param name="query">Query string parameters</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Get("/data/{**route}")]
    public Task<HttpContent> GetImage(
        string route,
        ImageQuery query,
        CancellationToken cancellationToken = default);
}

internal class ImageQuery
{
    [AliasAs("w")]
    public int Width { get; init; }
}
