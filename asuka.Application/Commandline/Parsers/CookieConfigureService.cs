using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using asuka.Application.Commandline.Options;
using asuka.Application.Configuration;
using asuka.Application.Output.Writer;
using FluentValidation;

namespace asuka.Application.Commandline.Parsers;

public class CookieConfigureService : ICommandLineParser
{
    private readonly IRequestConfigurator _requestConfigurator;
    private readonly IConsoleWriter _console;
    private readonly IValidator<CookieConfigureOptions> _validator;

    public CookieConfigureService(
        IRequestConfigurator requestConfigurator,
        IConsoleWriter console,
        IValidator<CookieConfigureOptions> validator)
    {
        _requestConfigurator = requestConfigurator;
        _console = console;
        _validator = validator;
    }

    public async Task Run(object options)
    {
        var opts = (CookieConfigureOptions)options;

        var validationResult = await _validator.ValidateAsync(opts);
        if (!validationResult.IsValid)
        {
            _console.ValidationErrors(validationResult.Errors);
            return;
        }
        
        // Read cookies
        var file = await File.ReadAllTextAsync(opts.CookieFile);
        var cookieData = JsonSerializer.Deserialize<CookieDump[]>(file);

        var cloudflare = cookieData.FirstOrDefault(x => x.Name == "cf_clearance");
        var csrf = cookieData.FirstOrDefault(x => x.Name == "csrftoken");

        await _requestConfigurator.ApplyCookies(cloudflare, csrf);
        await _requestConfigurator.ApplyUserAgent(opts.UserAgent);
    }
}
