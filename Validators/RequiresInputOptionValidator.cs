using asuka.Commandline.Options;
using FluentValidation;

namespace asuka.Validators;

public class RequiresInputOptionValidator : AbstractValidator<IRequiresInputOption>
{
    public RequiresInputOptionValidator()
    {
        RuleFor(opts => opts.Input)
            .GreaterThan(0)
            .WithMessage("Enter a valid gallery code.");
    }
}
