using System.Threading.Tasks;
using asuka.CommandOptions;

namespace asuka.CommandParsers;

public interface IRandomCommandService
{
    Task RunAsync(RandomOptions opts);
}
