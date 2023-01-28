using ShellProgressBar;

namespace asuka.Output.ProgressService;

public class ProgressService : IProgressService
{
    private IProgressBar _progressBar;

    public void CreateMasterProgress(int totalTicks, string title)
    {
        _progressBar = new ProgressBar(totalTicks, title, ProgressBarConfiguration.BarOption);
    }

    public IProgressBar GetMasterProgress()
    {
        return _progressBar;
    }

    public bool HasMasterProgress()
    {
        return _progressBar is not null;
    }

    public IProgressBar NestToMaster(int totalTicks, string title)
    {
        if (HasMasterProgress())
        {
            return _progressBar.Spawn(totalTicks, title, ProgressBarConfiguration.BarOption);
        }

        CreateMasterProgress(totalTicks, title);
        return GetMasterProgress();
    }

    public IProgressBar HookToInstance(IProgressBar bar, int totalTicks, string title)
    {
        return bar.Spawn(totalTicks, title, ProgressBarConfiguration.BarOption);
    }
}
