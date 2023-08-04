using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using asuka.Sdk.Providers.Models;

namespace asuka.Sdk.Providers.Extensions;

public static class GalleryResultExtensions
{
    public static string BuildReadableInformation(this GalleryResult result)
    {
        var builder = new StringBuilder();

        builder.AppendLine("Title =========================================");
        builder.AppendLine($"Japanese: {result.Title.Japanese}");
        builder.AppendLine($"English: {result.Title.English}");
        builder.AppendLine($"Pretty: {result.Title.Pretty}");

        builder.AppendLine("Tags ==========================================");
        builder.AppendLine($"Artists: {SafeJoin(result.Artists)}");
        builder.AppendLine($"Parodies: {SafeJoin(result.Parodies)}");
        builder.AppendLine($"Characters: {SafeJoin(result.Characters)}");
        builder.AppendLine($"Categories: {SafeJoin(result.Categories)}");
        builder.AppendLine($"Groups: {SafeJoin(result.Groups)}");
        builder.AppendLine($"Tags: {SafeJoin(result.Tags)}");
        builder.AppendLine($"Language: {SafeJoin(result.Languages)}");

        builder.AppendLine("===============================================");
        builder.AppendLine($"Total Pages: {result.TotalPages}");
        builder.AppendLine($"URL: https://nhentai.net/g/{result.Id}\n");

        return builder.ToString();
    }

    private static string SafeJoin(IReadOnlyList<string> strings)
    {
        if (strings is null)
        {
            return "";
        }

        return string.Join(", ", strings);
    }
    
    
    private sealed record TachiyomiDetails
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
    
    public static async Task WriteJsonMetadata(this GalleryResult result, string output)
    {
        var metaPath = Path.Combine(output, "details.json");
        var serializerOptions = new JsonSerializerOptions { WriteIndented = true };

        var description = new StringBuilder();
        description.AppendLine($"Source: {result.Url}");

        var json = new TachiyomiDetails
        {
            Title = result.Title.GetTitle(),
            Artist = string.Join(", ", result.Artists),
            Author = string.Join(", ", result.Artists),
            Description = description.ToString(),
            Genres = result.Tags
        };
        var metadata = JsonSerializer.Serialize(json, serializerOptions);

        await File.WriteAllTextAsync(metaPath, metadata).ConfigureAwait(false);
    }

    public static async Task WriteTextMetadata(this GalleryResult result, string output)
    {
        var metadataPath = Path.Combine(output, "info.txt");
        await File.WriteAllTextAsync(metadataPath, result.BuildReadableInformation())
            .ConfigureAwait(false);
    }
}
