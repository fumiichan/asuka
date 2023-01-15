using System.Threading.Tasks;
using asuka.Commandline.Options;

namespace asuka.Commandline.Parsers;

public interface IFileCommandService
{
    Task RunAsync(FileCommandOptions opts);
}
