namespace asuka.Commandline;

public interface ICommandLineParserFactory
{
    public ICommandLineParser GetInstance(CommandLineParserTokens token);
}
