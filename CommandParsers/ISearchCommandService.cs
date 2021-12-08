using System.Threading.Tasks;
using asuka.CommandOptions;

namespace asuka.CommandParsers;

public interface ISearchCommandService
{
    Task RunAsync(SearchOptions opts);
}
