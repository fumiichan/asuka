using System.Threading.Tasks;
using asuka.Commandline.Options;

namespace asuka.Commandline.Parsers;

public interface IConfigureCommand
{
    Task RunAsync(ConfigureOptions opts);
}
