namespace asuka.Core.Output.Progress;

public interface IProgressService
{
    void CreateMasterProgress(int totalTicks, string title);
    IProgressProvider NestToMaster(int totalTicks, string title);
    IProgressProvider GetMasterProgress();
    bool HasMasterProgress();
    IProgressProvider HookToInstance(IProgressProvider bar, int totalTicks, string title);
}
