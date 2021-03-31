using System.Threading.Tasks;
using asuka.CommandOptions;

namespace asuka.CommandParsers
{
    public interface IGetCommandService
    {
        Task RunAsync(GetOptions opts);
    }
}