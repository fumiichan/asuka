using System.Threading.Tasks;

namespace asuka.Commandline;

public interface ICommandLineParser
{
    public Task RunAsync(object options);
}
