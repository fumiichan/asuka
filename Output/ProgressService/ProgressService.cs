using asuka.Configuration;

namespace asuka.Output.ProgressService;

public class ProgressService : Core.Output.Progress.ProgressService
{
    private readonly IConfigurationManager _configuration;

    public ProgressService(IConfigurationManager configuration) : base()
    {
        _configuration = configuration;
    }
 
    public override void CreateMasterProgress(int totalTicks, string title)
    {
        _progressBar = ProgressProviderFactory
            .GetProvider(_configuration.GetValue("tui.progress"), totalTicks, title, ProgressBarConfiguration.BarOption);
    }
}
