using System.Threading.Tasks;
using asuka.CommandOptions;

namespace asuka.CommandParsers;

public interface IConfigureCommand
{
    Task RunAsync(ConfigureOptions opts);
}
