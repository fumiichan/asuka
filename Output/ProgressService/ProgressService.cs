using asuka.Configuration;
using asuka.Output.ProgressService.Providers;

namespace asuka.Output.ProgressService;

public class ProgressService : IProgressService
{
    private IProgressProvider _progressBar;
    private readonly IAppConfigManager _appConfig;

    public ProgressService(IAppConfigManager appConfig)
    {
        _appConfig = appConfig;
    }

    public void CreateMasterProgress(int totalTicks, string title)
    {
        _progressBar = ProgressProviderFactory
            .GetProvider(_appConfig.GetValue("tui.progress"), totalTicks, title, ProgressBarConfiguration.BarOption);
    }

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
