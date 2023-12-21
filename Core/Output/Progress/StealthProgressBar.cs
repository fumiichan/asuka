namespace asuka.Core.Output.Progress;

public class StealthProgressBar : IProgressProvider
{
    public void Tick()
    {
        // Progress is left empty to print nothing.
    }

    public void Tick(int newMaxTicks)
    {
        // Progress is left empty to print nothing.
    }

    public void Tick(string message)
    {
        // Progress is left empty to print nothing.
    }

    public void Tick(int newMaxTicks, string message)
    {
        // Progress is left empty to print nothing.
    }

    public void Stop()
    {
        // Progress is left empty to print nothing.
    }

    public void Stop(string message)
    {
        // Progress is left empty to print nothing.
    }

    public IProgressProvider Spawn(int maxTicks, string message)
    {
        return new StealthProgressBar();
    }
}