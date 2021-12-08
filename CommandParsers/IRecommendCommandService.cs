using System.Threading.Tasks;
using asuka.CommandOptions;
using Microsoft.Extensions.Configuration;

namespace asuka.CommandParsers;

public interface IRecommendCommandService
{
    Task RunAsync(RecommendOptions opts, IConfiguration configuration);
}
