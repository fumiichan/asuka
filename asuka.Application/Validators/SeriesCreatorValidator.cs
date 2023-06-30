using System.Linq;
using asuka.Application.Commandline.Options;
using FluentValidation;

namespace asuka.Application.Validators;

public class SeriesCreatorValidator : AbstractValidator<SeriesCreatorCommandOptions>
{
    public SeriesCreatorValidator()
    {
        When(opts => opts.FromList.Any(), () =>
        {
            RuleForEach(opts => opts.FromList)
                .NotEmpty()
                .WithMessage("One or more elements on this list contains invalid Ids.");
        });
    }
}
