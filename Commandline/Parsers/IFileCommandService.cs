using System.Threading.Tasks;
using asuka.CommandOptions;
using Microsoft.Extensions.Configuration;

namespace asuka.CommandParsers;

public interface IFileCommandService
{
    Task RunAsync(FileCommandOptions opts);
}
