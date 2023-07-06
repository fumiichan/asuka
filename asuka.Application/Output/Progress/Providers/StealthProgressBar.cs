namespace asuka.Application.Output.Progress.Providers;

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

    public void Close()
    {
        // Progress is left empty to print nothing.
    }

    public bool IsClosed()
    {
        return false;
    }

    public IProgressProvider Spawn(int maxTicks, string message)
    {
        return new StealthProgressBar();
    }
}
