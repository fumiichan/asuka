namespace asuka.Output.ProgressService.Providers.Wrappers;

public interface IProgressWrapper
{
    IProgressWrapper Spawn(int maxTicks, string title, object options);
    void Tick(string message = null);
    void Tick(int newTickCount, string message = null);
}
