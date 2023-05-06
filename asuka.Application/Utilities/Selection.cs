using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using asuka.Core.Extensions;
using asuka.Core.Models;
using Sharprompt;

namespace asuka.Application.Utilities;

public static class Selection
{
    public static Task<List<GalleryResult>> MultiSelect(IReadOnlyList<GalleryResult> responses)
    {
        return Task.Run(() =>
        {
            var selection = Prompt.MultiSelect("Select to download", responses, responses.Count, textSelector: (result) => result.Title.GetTitle());
            return selection.ToList();
        });
    }
}
