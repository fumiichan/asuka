using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace asuka.Core.Utilities;

public static class PathUtils
{
    #region Dictionaries of Character conversion
    private static readonly Dictionary<string, string> SymbolDictionary = new Dictionary<string, string>
                                                          {
                                                              {
                                                                  "#", "＃"
                                                              },
                                                              {
                                                                  "%", "％"
                                                              },
                                                              {
                                                                  "&", "＆"
                                                              },
                                                              {
                                                                  "{", "｛"
                                                              },
                                                              {
                                                                  "}", "｝"
                                                              },
                                                              {
                                                                  "\\", "／"
                                                              },
                                                              {
                                                                  "<", "＜"
                                                              },
                                                              {
                                                                  ">", "＞"
                                                              },
                                                              {
                                                                  "*", "＊"
                                                              },
                                                              {
                                                                  "?", "？"
                                                              },
                                                              {
                                                                  "/", "／"
                                                              },
                                                              {
                                                                  "$", "＄"
                                                              },
                                                              {
                                                                  "!", "！"
                                                              },
                                                              {
                                                                  "'", "＇"
                                                              },
                                                              {
                                                                  "\"", "＂"
                                                              },
                                                              {
                                                                  ":", "："
                                                              },
                                                              {
                                                                  "@", "＠"
                                                              },
                                                              {
                                                                  "+", "＋"
                                                              },
                                                              {
                                                                  "`", "｀"
                                                              },
                                                              {
                                                                  "|", "｜"
                                                              },
                                                              {
                                                                  "=", "＝"
                                                              },
                                                              {
                                                                  ".", "。"
                                                              },
                                                              {
                                                                  "\t", "_"
                                                              }
                                                          };
    #endregion
    /// <summary>
    /// Instead of removing known illegal file path characters, we can substitute them to their fixed-width
    /// alternatives.
    /// </summary>
    /// <param name="outputPath"></param>
    /// <param name="folderName"></param>
    /// <returns></returns>
    public static string NormalizeJoin(string outputPath, string folderName)
    {
        var normalizedFolderName = NormalizeName(folderName);
        var chapterRoot = Path.Combine(outputPath, normalizedFolderName);

        return chapterRoot;
    }

    public static string NormalizeName(string folderName)
    {
        var normalizedFolderName = $"{folderName}";
        normalizedFolderName = SymbolDictionary
            .Aggregate(normalizedFolderName, (current, pair) => current.Replace(pair.Key, pair.Value));

        return normalizedFolderName;
    }
}
