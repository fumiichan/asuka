using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using asuka.Model;

namespace asuka.Base
{
  class DisplayDoujinMetadata
  {
    /// <summary>
    /// Displays the information of doujin to the console.
    /// </summary>
    /// <param name="data">The nhentai response data.</param>
    public static void Display (Response data)
    {
      Console.WriteLine("Title ================================================");
      Console.WriteLine("Japanese: " + data.Title.Japanese);
      Console.WriteLine("English: " + data.Title.English);
      Console.WriteLine("Pretty: " + data.Title.Pretty);
      Console.WriteLine("======================================================");

      // Group each tags to display.
      string[] Artists = TagGrouper(data.Tags, "artist");
      string[] Parodies = TagGrouper(data.Tags, "parody");
      string[] Characters = TagGrouper(data.Tags, "character");
      string[] Tags = TagGrouper(data.Tags, "tag");
      string[] Categories = TagGrouper(data.Tags, "category");
      string[] Languages = TagGrouper(data.Tags, "language");
      string[] Groups = TagGrouper(data.Tags, "group");

      Console.WriteLine("Artist: " + string.Join(", ", Artists));
      Console.WriteLine("Parodies: " + string.Join(", ", Parodies));
      Console.WriteLine("Characters: " + string.Join(", ", Characters));
      Console.WriteLine("======================================================");
      Console.WriteLine("Tags: " + string.Join(", ", Tags));
      Console.WriteLine("Categories: " + string.Join(", ", Categories));
      Console.WriteLine("Languages: " + string.Join(", ", Languages));
      Console.WriteLine("Groups: " + string.Join(", ", Groups));
      Console.WriteLine("======================================================");
      Console.WriteLine("Total Pages: " + data.TotalPages.ToString());
      Console.WriteLine("URL: https://nhentai.net/g/" + data.Id.ToString());
      Console.WriteLine("");
    }

    /// <summary>
    /// Generates a info file
    /// </summary>
    /// <param name="data">nhentai response data</param>
    /// <param name="filePath">path to save the info file</param>
    public static void GenerateInfoFile (Response data, string filePath)
    {
      using var writer = new StreamWriter(filePath);
      TextWriter oldConsoleOutput = Console.Out;

      Console.SetOut(writer);

      Display(data);

      Console.SetOut(oldConsoleOutput);
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
  }
}
