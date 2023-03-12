using asuka.Commandline.Options;
using FluentValidation;

namespace asuka.Validators;

public class RecommendedOptionValidator : AbstractValidator<RecommendOptions>
{
    public RecommendedOptionValidator()
    {
        RuleFor(opts => opts.Input)
            .GreaterThan(0)
            .WithMessage("Enter a valid gallery code.");
    }
}
