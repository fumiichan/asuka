using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentValidation;
using asuka.CommandOptions;
using asuka.Downloader;
using asuka.Output;
using asuka.Services;

namespace asuka.CommandParsers
{
    public class GetCommandService : IGetCommandService
    {
        private readonly IGalleryRequestService _api;
        private readonly IValidator<IRequiresInputOption> _validator;
        private readonly IDownloadService _download;
        private readonly IConsoleWriter _console;

        public GetCommandService(
            IGalleryRequestService api,
            IValidator<IRequiresInputOption> validator,
            IDownloadService download,
            IConsoleWriter console)
        {
            _api = api;
            _validator = validator;
            _download = download;
            _console = console;
        }

        public async Task RunAsync(GetOptions opts)
        {
            var validationResult = await _validator.ValidateAsync(opts);
            if (!validationResult.IsValid)
            {
                _console.ErrorLine("Invalid gallery code.");
                return;
            }

            var response = await _api.FetchSingleAsync(opts.Input.ToString());
            _console.WriteLine(response.ToReadable());

            // Do not continue if the user specifies to read only.
            if (opts.ReadOnly)
            {
                return;
            }

            await _download.DownloadAsync(response, opts.Output, opts.Pack);
        }
    }
}