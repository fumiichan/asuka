using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace asuka.Application.Utilities;

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
    public static string Join(string? outputPath, string folderName)
    {
        var destinationPath = string.IsNullOrEmpty(outputPath)
            ? Environment.CurrentDirectory
            : outputPath;
        
        var normalizedFolderName = NormalizeName(folderName);
        var chapterRoot = Path.Combine(destinationPath, normalizedFolderName);

        return chapterRoot;
    }

    private static string NormalizeName(string folderName)
    {
        var normalizedFolderName = $"{folderName}";
        normalizedFolderName = SymbolDictionary
            .Aggregate(normalizedFolderName, (current, pair) => current.Replace(pair.Key, pair.Value));

        return normalizedFolderName;
    }
}
