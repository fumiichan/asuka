using System.Threading.Tasks;
using asuka.CommandOptions;

namespace asuka.CommandParsers;

public interface IRecommendCommandService
{
    Task RunAsync(RecommendOptions opts);
}
