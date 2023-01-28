using System.IO;
using asuka.Commandline.Options;
using FluentValidation;

namespace asuka.Validators;

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
