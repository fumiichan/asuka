using asuka.Output.ProgressService.Providers;
using asuka.Output.ProgressService.Providers.Wrappers;

namespace asuka.Output.ProgressService;

public interface IProgressService
{
    void CreateMasterProgress(int totalTicks, string title);
    IProgressProvider NestToMaster(int totalTicks, string title);
    IProgressProvider GetMasterProgress();
    bool HasMasterProgress();
    IProgressProvider HookToInstance(IProgressProvider bar, int totalTicks, string title);
}
