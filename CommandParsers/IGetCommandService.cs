using System.Threading.Tasks;
using asuka.CommandOptions;
using Microsoft.Extensions.Configuration;

namespace asuka.CommandParsers;

public interface IGetCommandService
{
    Task RunAsync(GetOptions opts, IConfiguration configuration);
}
