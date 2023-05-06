using asuka.Core.Output.Progress;

namespace asuka.Application.Output.ProgressService.Providers;

public class VoidProgressProvider : IProgressProvider
{
    public IProgressProvider Spawn(int maxTicks, string title)
    {
        return new VoidProgressProvider();
    }

    public void Tick()
    {
    }

    public void Tick(string message)
    {
    }

    public void Tick(int newTickCount)
    {
    }

    public void Tick(int newTickCount, string message)
    {
    }
}
