using asuka.Application.Commandline.Options;
using FluentValidation;

namespace asuka.Application.Validators;

public class RecommendedOptionValidator : AbstractValidator<RecommendOptions>
{
    public RecommendedOptionValidator()
    {
        RuleFor(opts => opts.Input)
            .NotEmpty()
            .WithMessage("URLs/Code should not be empty.");
    }
}
