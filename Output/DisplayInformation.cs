using System.Collections.Generic;
using System.Text;
using asuka.Models;

namespace asuka.Output
{
  public static class DisplayInformation
  {
    public static string ToReadable(this GalleryResult result)
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

    private static string SafeJoin(IEnumerable<string> strings)
    {
      return strings == null ? string.Empty : string.Join(", ", strings);
    }
  }
}