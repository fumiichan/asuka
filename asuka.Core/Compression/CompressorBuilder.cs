using asuka.Core.Chaptering.Models;
using asuka.Core.Events;
using Microsoft.Extensions.Logging;

namespace asuka.Core.Compression;

public class CompressorBuilder
{
    private Action<ProgressEventArgs> _onEachComplete;
    private string _output;
    private ILogger _attachedLogger = new DummyLogging();

    public CompressorBuilder SetSeries(Series series)
    {
        _output = series.Output + ".cbz";
        return this;
    }

    public CompressorBuilder AttachLogger(ILogger logger)
    {
        _attachedLogger = logger;
        return this;
    }

    public CompressorBuilder SetEachCompleteListener(Action<ProgressEventArgs> listener)
    {
        _onEachComplete = listener;
        return this;
    }

    public async Task Run(IEnumerable<CompressionItem> files)
    {
        var compressor = new Compressor(_attachedLogger);

        if (_onEachComplete is not null)
        {
            compressor.HandleProgress(_onEachComplete);
        }

        await compressor.Run(files, _output);
    }
}
