using asuka.Core.Chaptering.Models;
using asuka.Core.Events;
using asuka.Core.Utilities;
using Microsoft.Extensions.Logging;

namespace asuka.Core.Downloader;

public class DownloaderBuilder
{
    private Chapter _chapter;
    private Action<ProgressEventArgs> _onEachComplete;
    private string _output;
    private ILogger _attachedLogger = new DummyLogging();

    public DownloaderBuilder AttachLogger(ILogger logger)
    {
        _attachedLogger = logger;
        return this;
    }

    public DownloaderBuilder SetChapter(Chapter chapter)
    {
        _chapter = chapter;
        return this;
    }

    public DownloaderBuilder SetOutput(string output)
    {
        if (_chapter == null)
        {
            return this;
        }

        _output = PathUtils.NormalizeJoin(output, $"ch{_chapter.Id}");
        return this;
    }

    public DownloaderBuilder SetEachCompleteHandler(Action<ProgressEventArgs> listener)
    {
        _onEachComplete = listener;
        return this;
    }

    public Downloader Build()
    {
        if (!Directory.Exists(_output))
        {
            Directory.CreateDirectory(_output);
        }
 
        var downloader = new Downloader(_chapter, _output, _attachedLogger);
        downloader.HandleProgress(_onEachComplete);

        return downloader;
    }
}
