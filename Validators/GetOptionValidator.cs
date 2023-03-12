using System.Runtime.InteropServices.JavaScript;
using asuka.Commandline.Options;
using FluentValidation;

namespace asuka.Validators;

public class GetValidator : AbstractValidator<GetOptions>
{
    public GetValidator()
    {
        RuleForEach(opts => opts.Input)
            .GreaterThan(0)
            .WithMessage("IDs must not be lower than 0.");
    }
}
