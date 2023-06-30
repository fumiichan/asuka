using asuka.Application.Commandline.Options;
using FluentValidation;

namespace asuka.Application.Validators;

public class GetValidator : AbstractValidator<GetOptions>
{
    public GetValidator()
    {
        RuleForEach(opts => opts.Input)
            .NotEmpty()
            .WithMessage("URLs/Code should not be empty.");
    }
}
