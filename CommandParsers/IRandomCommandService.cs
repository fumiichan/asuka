using System.Threading.Tasks;
using asuka.CommandOptions;
using Microsoft.Extensions.Configuration;

namespace asuka.CommandParsers;

public interface IRandomCommandService
{
    Task RunAsync(RandomOptions opts, IConfiguration configuration);
}
