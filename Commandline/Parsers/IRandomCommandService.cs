using System.Threading.Tasks;
using asuka.Commandline.Options;

namespace asuka.Commandline.Parsers;

public interface IRandomCommandService
{
    Task RunAsync(RandomOptions opts);
}
