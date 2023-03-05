using System.Runtime.InteropServices.JavaScript;

namespace asuka.Output.ProgressService.Providers;

public class VoidProgressProvider : IProgressProvider
{
    public IProgressProvider Spawn(int maxTicks, string title, object options)
    {
        return new VoidProgressProvider();
    }

    public void Tick(string message = null)
    {
    }

    public void Tick(int newTickCount, string message = null)
    {
    }
}
