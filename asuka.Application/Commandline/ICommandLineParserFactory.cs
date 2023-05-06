namespace asuka.Application.Commandline;

public interface ICommandLineParserFactory
{
    public ICommandLineParser GetInstance(CommandLineParserTokens token);
}
