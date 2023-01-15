using System.Threading.Tasks;
using asuka.Commandline.Options;

namespace asuka.Commandline.Parsers;

public interface IGetCommandService
{
    Task RunAsync(GetOptions opts);
}
