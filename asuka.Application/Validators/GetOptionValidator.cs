using asuka.Application.Commandline.Options;
using FluentValidation;

namespace asuka.Application.Validators;

public class GetValidator : AbstractValidator<GetOptions>
{
    public GetValidator()
    {
        RuleForEach(opts => opts.Input)
            .GreaterThan(0)
            .WithMessage("IDs must not be lower than 0.");
    }
}
