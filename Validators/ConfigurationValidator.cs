using System.Text.RegularExpressions;
using asuka.Commandline.Options;
using FluentValidation;

namespace asuka.Validators;

public class ConfigurationValidator : AbstractValidator<ConfigureOptions>
{
    public ConfigurationValidator()
    {
        When(opts => opts.ReadConfigMode, () =>
        {
            RuleFor(opts => opts.Key)
                .Must(x => !string.IsNullOrEmpty(x))
                .WithMessage("Invalid key");
        });

        When(opts => opts.SetConfigMode, () =>
        {
            RuleFor(opts => opts.Key)
                .Must(x => !string.IsNullOrEmpty(x))
                .WithMessage("Invalid key.");

            RuleFor(opts => opts.Value)
                .Must(x => !string.IsNullOrEmpty(x))
                .WithMessage("Invalid value");
        });
    }
}
