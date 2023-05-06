using System.IO;
using asuka.Application.Commandline.Options;
using FluentValidation;

namespace asuka.Application.Validators;

public class CookieConfiguratorValidator : AbstractValidator<CookieConfigureOptions>
{
    public CookieConfiguratorValidator()
    {
        When(opts => !string.IsNullOrEmpty(opts.CookieFile), () =>
        {
            RuleFor(opts => opts.CookieFile)
                .Must(File.Exists)
                .WithMessage("Your cookie file cannot be found!");
        });
    }
}
