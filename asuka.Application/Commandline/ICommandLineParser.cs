using System.Threading.Tasks;

namespace asuka.Application.Commandline;

public interface ICommandLineParser
{
    public Task RunAsync(object options);
}
