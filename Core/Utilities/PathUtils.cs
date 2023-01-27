using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace asuka.Core.Utilities;

public struct NormalizeJoinDetails
{
    public string ChapterRoot { get; init; }
    public string ChapterPath { get; init; }
}

public static class PathUtils
{
    /// <summary>
    /// Instead of removing known illegal file path characters, we can substitute them to their fixed-width
    /// alternatives.
    /// </summary>
    /// <param name="outputPath"></param>
    /// <param name="folderName"></param>
    /// <param name="chapter"></param>
    /// <returns></returns>
    public static NormalizeJoinDetails NormalizeJoin(string outputPath, string folderName, int chapter = -1)
    {
        #region Dictionaries of Character conversion
        var characterMap = new Dictionary<string, string>
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
            }
        };
        #endregion

        var normalizedFolderName = $"{folderName}";
        normalizedFolderName = characterMap
            .Aggregate(normalizedFolderName, (current, pair) => current.Replace(pair.Key, pair.Value));

        var chapterRoot = Path.Combine(outputPath, chapter == -1 ? normalizedFolderName : $"{normalizedFolderName}");

        return new NormalizeJoinDetails
        {
            ChapterPath = chapter == -1 ? chapterRoot : Path.Combine(chapterRoot, $"ch{chapter}"),
            ChapterRoot = chapterRoot
        };
    }
}
