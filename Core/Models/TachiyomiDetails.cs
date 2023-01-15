using System.Collections.Generic;
using Newtonsoft.Json;

namespace asuka.Models;

public record TachiyomiDetails
{
    [JsonProperty("title")]
    public string Title { get; init; }

    [JsonProperty("author")]
    public string Author { get; init; }

    [JsonProperty("artist")]
    public string Artist { get; init; }

    [JsonProperty("description")]
    public string Description { get; init; }

    [JsonProperty("genre")]
    public IReadOnlyList<string> Genres { get; init; }

    [JsonProperty("status")]
    public string Status { get; init; } = "2";
}
