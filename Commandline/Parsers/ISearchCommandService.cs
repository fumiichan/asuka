using System.Threading.Tasks;
using asuka.CommandOptions;
using Microsoft.Extensions.Configuration;

namespace asuka.CommandParsers;

public interface ISearchCommandService
{
    Task RunAsync(SearchOptions opts);
}
