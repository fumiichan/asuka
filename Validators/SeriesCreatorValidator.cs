using System.Linq;
using asuka.Commandline.Options;
using FluentValidation;

namespace asuka.Validators;

public class SeriesCreatorValidator : AbstractValidator<SeriesCreatorCommandOptions>
{
    public SeriesCreatorValidator()
    {
        When(opts => opts.FromList.Any(), () =>
        {
            RuleForEach(opts => opts.FromList)
                .Matches(@"^\d{1,6}$")
                .WithMessage("One or more elements on this list contains invalid Ids.");
        });
    }
}
