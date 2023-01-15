using System.Threading.Tasks;
using asuka.Commandline.Options;

namespace asuka.Commandline.Parsers;

public interface IRecommendCommandService
{
    Task RunAsync(RecommendOptions opts);
}
