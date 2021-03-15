using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using asukav2.Models;

namespace asukav2.Lib
{
  public static class DisplayOutputLibrary
  {
    /// <summary>
    /// Groups nhentai tags to their own group. It just map the list of tag objects to
    /// a string.
    /// </summary>
    /// <param name="tags"></param>
    /// <param name="type"></param>
    /// <returns>List of tags based by type.</returns>
    private static IEnumerable<string> GroupTags(IEnumerable<Tag> tags, string type)
    {
      return tags.Where(tag => tag.Type == type)
        .Select(tag => tag.Name)
        .ToArray();
    }

    /// <summary>
    /// Construct a user-readable information.
    /// </summary>
    /// <param name="info">Doujin information fetched from servers</param>
    /// <returns>A string that contains user readable information.</returns>
    public static async Task<string> StringifyResponseAsync(ResponseModel info)
    {
      var task = await Task.Run(() =>
      {
        var builder = new StringBuilder();

        builder.AppendLine("Title ========================================");
        builder.AppendLine($"Japanese: {info.Title.Japanese}");
        builder.AppendLine($"English: {info.Title.English}");
        builder.AppendLine($"Pretty: {info.Title.Pretty}");
        builder.AppendLine("Tags =========================================");

        var artists = GroupTags(info.Tags, "artist");
        var parodies = GroupTags(info.Tags, "parody");
        var characters = GroupTags(info.Tags, "character");
        var tags = GroupTags(info.Tags, "tag");
        var categories = GroupTags(info.Tags, "category");
        var languages = GroupTags(info.Tags, "language");
        var groups = GroupTags(info.Tags, "group");

        builder.AppendLine($"Artists: {string.Join(", ", artists)}");
        builder.AppendLine($"Parodies: {string.Join(", ", parodies)}");
        builder.AppendLine($"Characters: {string.Join(", ", characters)}");
        builder.AppendLine($"Categories: {string.Join(", ", categories)}");
        builder.AppendLine($"Groups: {string.Join(", ", groups)}");
        builder.AppendLine($"Tags: {string.Join(", ", tags)}");
        builder.AppendLine($"Language: {string.Join(", ", languages)}");

        builder.AppendLine("==============================================");

        builder.AppendLine($"Total Pages: {info.TotalPages}");
        builder.AppendLine($"URL: https://nhentai.net/g/{info.Id}");

        return builder.ToString();
      });

      return task;
    }
  }
}
