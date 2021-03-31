using FluentValidation;
using FluentValidation.Validators;
using asuka.CommandOptions;

namespace asuka.Validators
{
    public class SearchValidator : AbstractValidator<SearchOptions>
    {
        public SearchValidator()
        {
            RuleForEach(opts => opts.Queries)
                .Custom(QueriesValidator);

            RuleFor(opts => opts.Page)
                .GreaterThan(0);

            RuleForEach(opts => opts.DateRange)
                .Matches(@"(>|<)?(=)?(\d+)(d|m|w|y)")
                .WithMessage("One or more arguments on your date range is wrong.");

            RuleForEach(opts => opts.PageRange)
                .Matches(@"(>|<)?(=)?(\d+)")
                .WithMessage("One or more arguments on your page range is wrong.");
        }

        private static void QueriesValidator(string query, CustomContext context)
        {
            if (query.StartsWith("-"))
            {
                context.AddFailure("Excluded tags should use exclude option.");
            }
        }
    }
}