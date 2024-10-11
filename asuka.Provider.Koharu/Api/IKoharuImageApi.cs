using Refit;

namespace asuka.Provider.Koharu.Api;

internal interface IKoharuImageApi
{
    /// <summary>
    /// Retrieves the image
    /// </summary>
    /// <param name="id">ID of the gallery</param>
    /// <param name="publicKey">Public Key of the gallery</param>
    /// <param name="hash1">Whatever this is (1)</param>
    /// <param name="hash2">Whatever this is (2)</param>
    /// <param name="file"></param>
    /// <param name="query">Query string parameters</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Get("/data/{id}/{publicKey}/{hash1}/{hash2}/{file}")]
    public Task<HttpContent> GetImage(
        string id,
        string publicKey,
        string hash1,
        string hash2,
        string file,
        ImageQuery query,
        CancellationToken cancellationToken = default);
}

internal class ImageQuery
{
    [AliasAs("w")]
    public int Width { get; init; }
}
