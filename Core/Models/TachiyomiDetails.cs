using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace asuka.Core.Models;

public record TachiyomiDetails
{
    [JsonPropertyName("title")]
    public string Title { get; init; }

    [JsonPropertyName("author")]
    public string Author { get; init; }

    [JsonPropertyName("artist")]
    public string Artist { get; init; }

    [JsonPropertyName("description")]
    public string Description { get; init; }

    [JsonPropertyName("genre")]
    public IReadOnlyList<string> Genres { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = "2";
}
