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
        // Progress is left empty to print nothing.
    }

    public void Tick(string message)
    {
        // Progress is left empty to print nothing.
    }

    public void Tick(int newTickCount)
    {
        // Progress is left empty to print nothing.
    }

    public void Tick(int newTickCount, string message)
    {
        // Progress is left empty to print nothing.
    }
}
