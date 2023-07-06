namespace asuka.Application.Output.Progress;

public interface IProgressProviderFactory
{
    IProgressProvider Create(int maxTicks, string message);
}
