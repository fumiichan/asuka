namespace asuka.Output.ProgressService.Providers;

public interface IProgressProvider
{
    IProgressProvider Spawn(int maxTicks, string title, object options);
    void Tick();
    void Tick(string message);
    void Tick(int newTickCount);
    void Tick(int newTickCount, string message);
}
