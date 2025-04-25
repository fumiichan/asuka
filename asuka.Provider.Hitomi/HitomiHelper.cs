using System.Text.RegularExpressions;
using asuka.Provider.Hitomi.Contracts.Responses;

namespace asuka.Provider.Hitomi;

internal static partial class HitomiHelper
{
    public static async Task<GgResult> GetGgCode(HttpClient client, CancellationToken cancellationToken = default)
    {
        using var request = await client.GetAsync("gg.js", cancellationToken);
        request.EnsureSuccessStatusCode();
        
        var response = await request.Content.ReadAsStringAsync(cancellationToken);
        
        // the m stuff
        var mDict = new Dictionary<int, int>();
        var mFilter = TheMStuffRegex().Matches(response);

        var k = new List<int>();
        foreach (Match m in mFilter)
        {
            var ky = m.Groups[1].Value;
            var vl = m.Groups[2].Value;

            if (int.TryParse(ky, out var kk))
            {
                k.Add(kk);
                if (int.TryParse(vl, out var v))
                {
                    foreach (var n in k)
                    {
                        mDict[n] = v;
                    }
                    k.Clear();
                }
            }
        }

        // the b code something
        var bFilter = GgCodeRegex().Match(response);
        
        // the d code something
        var dFilter = TheDStuffRegex().Match(response).Groups[1].Value;

        return new GgResult
        {
            M = mDict,
            D = !string.IsNullOrEmpty(dFilter) ? int.Parse(dFilter) : 0,
            B = int.Parse(bFilter.Groups[1].Value.TrimEnd('/'))
        };
    }

    public static List<string> MergeTags(this GalleryInformation info)
    {
        var tags = new List<string>();
        tags.AddRange(info.Tags?.Select(x => x.Name) ?? []);
        tags.AddRange(info.Parodies?.Select(x => x.Name) ?? []);

        return tags;
    }

    [GeneratedRegex(@"(?:var\s|default:)\s*o\s*=\s*(\d+)")]
    private static partial Regex TheDStuffRegex();

    [GeneratedRegex(@"case\s+(\d+):(?:\s*o\s*=\s*(\d+))?")]
    private static partial Regex TheMStuffRegex();

    [GeneratedRegex(@"b:\s*[\""'](.+)[\""']")]
    private static partial Regex GgCodeRegex();
}

internal sealed class GgResult
{
    public required Dictionary<int, int> M { get; init; }
    public required int B { get; init; }
    public required int D { get; init; }
}
