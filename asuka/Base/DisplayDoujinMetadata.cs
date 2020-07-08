using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using asuka.Model;

namespace asuka.Base
{
  public static class DisplayDoujinMetadata
  {
    /// <summary>
    /// Displays the information of doujin to the console.
    /// </summary>
    /// <param name="data">The nhentai response data.</param>
    public static void Display (Response data)
    {
      Console.WriteLine(BuildInfo(data));
    }

    /// <summary>
    /// Generates a info file
    /// </summary>
    /// <param name="data">nhentai response data</param>
    /// <param name="filePath">path to save the info file</param>
    public static void GenerateInfoFile (Response data, string filePath)
    {
      string info = BuildInfo(data);
      File.WriteAllText(filePath, info);
    }

    /// <summary>
    /// Groups nhentai tags to their own group.
    /// </summary>
    /// <param name="Tags">List of Tags</param>
    /// <param name="Type">Type of tag to group</param>
    /// <returns>List of tags</returns>
    private static string[] TagGrouper (List<Tag> Tags, string Type)
    {
      List<Tag> Group = Tags.Where(tag => tag.Type == Type).ToList();
      string[] group = Group.Select(value => value.Name).ToArray();

      return group;
    }

    private static string BuildInfo (Response data)
    {
      StringBuilder output = new StringBuilder();

      output.AppendLine("Title ================================================");
      output.AppendLine("Japanese: " + data.Title.Japanese);
      output.AppendLine("English: " + data.Title.English);
      output.AppendLine("Pretty: " + data.Title.Pretty);
      output.AppendLine("======================================================");

      // Group each tags to display.
      string[] Artists = TagGrouper(data.Tags, "artist");
      string[] Parodies = TagGrouper(data.Tags, "parody");
      string[] Characters = TagGrouper(data.Tags, "character");
      string[] Tags = TagGrouper(data.Tags, "tag");
      string[] Categories = TagGrouper(data.Tags, "category");
      string[] Languages = TagGrouper(data.Tags, "language");
      string[] Groups = TagGrouper(data.Tags, "group");

      output.AppendLine($"Artist: {string.Join(", ", Artists)}");
      output.AppendLine($"Parodies: {string.Join(", ", Parodies)}");
      output.AppendLine($"Characters: {string.Join(", ", Parodies)}");
      output.AppendLine("======================================================");
      output.AppendLine($"Tags: {string.Join(", ", Tags)}");
      output.AppendLine($"Categories: {string.Join(", ", Categories)}");
      output.AppendLine($"Languages: {string.Join(", ", Languages)}");
      output.AppendLine($"Groups: {string.Join(", ", Groups)}");
      output.AppendLine("======================================================");
      output.AppendLine($"Total Pages: {data.TotalPages}");
      output.AppendLine($"URL: https://nhentai.net/g/{data.Id}");

      return output.ToString();
    }
  }
}
