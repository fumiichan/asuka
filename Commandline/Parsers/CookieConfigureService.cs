using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using asuka.Commandline.Options;
using asuka.Configuration;
using asuka.Output;
using FluentValidation;

namespace asuka.Commandline.Parsers;

public class CookieConfigureService : ICommandLineParser
{
    private readonly IRequestConfigurator _requestConfigurator;
    private readonly IValidator<CookieConfigureOptions> _validator;

    public CookieConfigureService(
        IRequestConfigurator requestConfigurator,
        IValidator<CookieConfigureOptions> validator)
    {
        _requestConfigurator = requestConfigurator;
        _validator = validator;
    }

    public async Task RunAsync(object options)
    {
        var opts = (CookieConfigureOptions)options;

        var validationResult = await _validator.ValidateAsync(opts);
        if (!validationResult.IsValid)
        {
            validationResult.Errors.PrintValidationExceptions();
            return;
        }
        
        // Read cookies
        var file = await File.ReadAllTextAsync(opts.CookieFile);
        var cookieData = JsonSerializer.Deserialize<CookieDump[]>(file);

        if (cookieData != null)
        {
            var cloudflare = cookieData.FirstOrDefault(x => x.Name == "cf_clearance");
            var csrf = cookieData.FirstOrDefault(x => x.Name == "csrftoken");

            if (cloudflare != null && csrf != null)
            {
                await _requestConfigurator.ApplyCookies(cloudflare, csrf);
            }

            if (!string.IsNullOrEmpty(opts.UserAgent))
            {
                await _requestConfigurator.ApplyUserAgent(opts.UserAgent);
            }

            return;
        }
        
        Console.WriteLine("An error occured at reading the cookie you provided.");
    }
}
