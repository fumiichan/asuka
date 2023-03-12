namespace asuka.Output.ProgressService.Providers.Wrappers;

public interface IProgressWrapper
{
    IProgressWrapper Spawn(int maxTicks, string title, object options);
    void Tick();
    void Tick(string message);
    void Tick(int newTickCount);
    void Tick(int newTickCount, string message);
}
