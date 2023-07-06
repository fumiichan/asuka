using asuka.Core.Chaptering.Models;
using asuka.Core.Events;
using asuka.Core.Requests;
using asuka.Core.Utilities;

namespace asuka.Core.Downloader;

public class DownloaderBuilder
{
    private IGalleryImageRequestService _imageRequestService;
    private Chapter _chapter;
    private Action<ProgressEvent> _onEachComplete;
    private string _output;

    public DownloaderBuilder SetImageRequestService(IGalleryImageRequestService service)
    {
        _imageRequestService = service;
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

    public DownloaderBuilder SetEachCompleteHandler(Action<ProgressEvent> listener)
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
 
        var downloader = new Downloader(_imageRequestService, _chapter, _output);
        downloader.HandleProgress(_onEachComplete);

        return downloader;
    }
}
