using asuka.Core.Output.Progress;

namespace asuka.Output.Progress;

public interface IProgressFactory
{
    IProgressProvider Create(int maxTicks, string message);
    IProgressProvider Create(ProgressTypes type, int maxTicks, string message);
}
