using asuka.Provider.Sdk;
using asuka.Provider.Sdk.Extensions;
using Cocona;
using System.Text.Json;
using System.Text.Json.Serialization;

var app = CoconaApp.Create();
app.AddCommand(async (string path) =>
{
    // JSON files
    var jsonFiles = Directory.GetFiles(path, "details.json", SearchOption.AllDirectories);
    
    // Process
    foreach (var json in jsonFiles)
    {
        try
        {
            var file = await File.ReadAllTextAsync(json);
            var document = JsonSerializer.Deserialize<OldJsonFormat>(file);

            if (document == null)
            {
                Console.WriteLine("Failed to parse file: " + json);
                continue;
            }
            
            // Convert document to ComicInfo format
            var series = new Series
            {
                Title = document.Title,
                Authors = document.Author.Split(", ").ToList(),
                Artists = document.Artist.Split(", ").ToList(),
                Genres = document.Genres.ToList(),
                Chapters = [],
                Status = SeriesStatus.Completed
            };
            
            var outputPath = Path.Combine(Path.GetDirectoryName(json)!, "ComicInfo.xml");
            await series.WriteMetadata(outputPath);
            
            Console.WriteLine("Migrated: " + json);
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to migrate file: " + json);
            Console.WriteLine(e);
        }
    }
});

await app.RunAsync();

internal sealed class OldJsonFormat
{
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;
    
    [JsonPropertyName("author")]
    public string Author { get; init; } = string.Empty;
    
    [JsonPropertyName("artist")]
    public string Artist { get; init; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;
    
    [JsonPropertyName("genre")]
    public IEnumerable<string> Genres { get; init; } = [];
    
    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;
}
