namespace asuka.Output.ProgressService.Providers;

public interface IProgressProvider
{
    IProgressProvider Spawn(int maxTicks, string title, object options);
    void Tick(string message = null);
    void Tick(int newTickCount, string message = null);
}
