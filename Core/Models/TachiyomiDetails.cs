using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace asuka.Core.Models;

public record TachiyomiDetails
{
    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("author")]
    public required string Author { get; init; }

    [JsonPropertyName("artist")]
    public required string Artist { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("genre")]
    public required IReadOnlyList<string> Genres { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = "2";
}
