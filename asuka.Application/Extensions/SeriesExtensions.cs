using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using asuka.Provider.Sdk;

namespace asuka.Application.Extensions;

internal static class SeriesExtensions
{
// ReSharper disable UnusedAutoPropertyAccessor.Local
#pragma warning disable CS8618
    private class TachiyomiDetails
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
        public IEnumerable<string> Genres { get; init; }
        
        [JsonPropertyName("status")]
        public string Status { get; init; }
    }
#pragma warning restore CS8618
// ReSharper restore UnusedAutoPropertyAccessor.Local

    /// <summary>
    /// Save metadata
    /// </summary>
    /// <param name="series"></param>
    /// <param name="writePath"></param>
    public static async Task WriteMetadata(this Series series, string writePath)
    {
        var metadata = new TachiyomiDetails
        {
            Title = series.Title,
            Artist = string.Join(", ", series.Artists),
            Author = string.Join(", ", series.Authors),
            Description = "",
            Genres = series.Genres,
            Status = series.Status == SeriesStatus.Completed ? "2" : "1"
        };

        var serialized = JsonSerializer.Serialize(metadata);
        await File.WriteAllTextAsync(writePath, serialized, Encoding.UTF8);
    }
}
