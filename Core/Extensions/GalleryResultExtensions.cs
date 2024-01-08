using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using asuka.Core.Models;
using asuka.Output;

namespace asuka.Core.Extensions;

public static class GalleryResultExtensions
{
    public static async Task WriteMetadata(this GalleryResult result, string destination)
    {
        var serializerOptions = new JsonSerializerOptions { WriteIndented = true };
        var metadata = JsonSerializer
            .Serialize(CreateTachiyomiDetails(result), serializerOptions);

        await File.WriteAllTextAsync(destination, metadata).ConfigureAwait(false);
    }

    public static string ToFormattedText(this GalleryResult result)
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

    private static TachiyomiDetails CreateTachiyomiDetails(GalleryResult result)
    {
        return new TachiyomiDetails
        {
            Title = result.Title.GetTitle(),
            Author = SafeJoin(result.Artists),
            Artist = SafeJoin(result.Artists),
            Description = $"Source: https://nhentai.net/g/{result.Id}",
            Genres = result.Tags
        };
    }
    
    private static string SafeJoin(IEnumerable<string>? strings)
    {
        return strings == null ? string.Empty : string.Join(", ", strings);
    }
}