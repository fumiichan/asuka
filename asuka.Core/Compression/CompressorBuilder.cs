using asuka.Core.Chaptering.Models;
using asuka.Core.Events;

namespace asuka.Core.Compression;

public class CompressorBuilder
{
    private Action<ProgressEvent> _onEachComplete;
    private Series _series;
    private string _output;

    public CompressorBuilder SetSeries(Series series)
    {
        _series = series;
        _output = _series.Output + ".cbz";

        return this;
    }

    public CompressorBuilder SetEachCompleteListener(Action<ProgressEvent> listener)
    {
        _onEachComplete = listener;
        return this;
    }

    public async Task Run(IEnumerable<CompressionItem> files)
    {
        var compressor = new Compressor();

        if (_onEachComplete is not null)
        {
            compressor.HandleProgress(_onEachComplete);
        }

        await compressor.Run(files, _output);
    }
}
