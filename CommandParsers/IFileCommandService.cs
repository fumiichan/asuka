using System.Threading.Tasks;
using asuka.CommandOptions;

namespace asuka.CommandParsers;

public interface IFileCommandService
{
    Task RunAsync(FileCommandOptions opts);
}
