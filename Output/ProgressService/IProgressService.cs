using ShellProgressBar;

namespace asuka.Output.ProgressService;

public interface IProgressService
{
    void CreateMasterProgress(int totalTicks, string title);
    IProgressBar NestToMaster(int totalTicks, string title);
    IProgressBar GetMasterProgress();
    bool HasMasterProgress();
}
