using System.Text.Json.Serialization;

namespace asuka.Provider.Koharu.Contracts.Responses;

internal sealed class GalleryRandomResponse
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("public_key")]
    public string PublicKey { get; init; } = string.Empty;
}
