using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using asuka.Application.Commandline.Options;
using asuka.Application.Configuration;
using asuka.Application.Utilities;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace asuka.Application.Commandline.Parsers;

public class CookieConfigureService : ICommandLineParser
{
    private readonly IRequestConfigurator _requestConfigurator;
    private readonly IValidator<CookieConfigureOptions> _validator;
    private readonly ILogger _logger;

    public CookieConfigureService(
        IRequestConfigurator requestConfigurator,
        IValidator<CookieConfigureOptions> validator,
        ILogger logger)
    {
        _requestConfigurator = requestConfigurator;
        _validator = validator;
        _logger = logger;
    }

    public async Task Run(object options)
    {
        var opts = (CookieConfigureOptions)options;

        var validationResult = await _validator.ValidateAsync(opts);
        if (!validationResult.IsValid)
        {
            validationResult.Errors.PrintErrors(_logger);
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
