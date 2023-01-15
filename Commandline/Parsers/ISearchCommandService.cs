using System.Threading.Tasks;
using asuka.Commandline.Options;

namespace asuka.Commandline.Parsers;

public interface ISearchCommandService
{
    Task RunAsync(SearchOptions opts);
}
