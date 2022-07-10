using System.IO;
using asuka.CommandOptions;
using FluentValidation;

namespace asuka.Validators;

public class ConfigurationValidator : AbstractValidator<ConfigureOptions>
{
    public ConfigurationValidator()
    {
        When(opts => !string.IsNullOrEmpty(opts.Theme), () =>
        {
            RuleFor(opts => opts.Theme)
                .Matches(@"(light|dark)")
                .WithMessage("Invalid Configuration value. Must be either light or dark");
        });

        When(opts => !string.IsNullOrEmpty(opts.UseTachiyomiLayoutToggle), () =>
        {
            RuleFor(opts => opts.UseTachiyomiLayoutToggle)
                .Matches(@"(True|False)")
                .WithMessage("Invalid values. Use True or False as values. (Case sensitive)");
        });

        When(opts => !string.IsNullOrEmpty(opts.SetDefaultCookies), () =>
        {
            RuleFor(opts => opts.SetDefaultCookies)
                .Must(File.Exists)
                .WithMessage("The Cookie file you specified cannot be found.");
        });
    }
}
