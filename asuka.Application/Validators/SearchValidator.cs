using asuka.Application.Commandline.Options;
using FluentValidation;

namespace asuka.Application.Validators;

public class SearchValidator : AbstractValidator<SearchOptions>
{
    public SearchValidator()
    {
        RuleForEach(opts => opts.Queries)
            .Must(query => !query.StartsWith("-"))
            .WithMessage("Queries should not start with a dash. Use --exclude option instead.");

        RuleFor(opts => opts.Page)
            .GreaterThan(0);

        RuleForEach(opts => opts.DateRange)
            .Matches(@"(>|<)?(=)?(\d+)(d|m|w|y)")
            .WithMessage("One or more arguments on your date range is wrong.");

        RuleForEach(opts => opts.PageRange)
            .Matches(@"(>|<)?(=)?(\d+)")
            .WithMessage("One or more arguments on your page range is wrong.");

        RuleFor(opts => opts.Sort)
            .Matches(@"popular-?(week|today)?|date")
            .WithMessage("Invalid Sort option.");
    }
}
