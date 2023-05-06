using asuka.Output.ProgressService;

namespace asuka.Core.Output.Progress;

public abstract class ProgressService : IProgressService
{
    protected IProgressProvider _progressBar;

    public abstract void CreateMasterProgress(int totalTicks, string title);

    public IProgressProvider GetMasterProgress()
    {
        return _progressBar;
    }

    public bool HasMasterProgress()
    {
        return _progressBar is not null;
    }

    public IProgressProvider NestToMaster(int totalTicks, string title)
    {
        if (HasMasterProgress())
        {
            return _progressBar.Spawn(totalTicks, title, ProgressBarConfiguration.BarOption);
        }

        CreateMasterProgress(totalTicks, title);
        return GetMasterProgress();
    }

    public IProgressProvider HookToInstance(IProgressProvider bar, int totalTicks, string title)
    {
        return bar.Spawn(totalTicks, title, ProgressBarConfiguration.BarOption);
    }
}
