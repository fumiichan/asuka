using FluentValidation;
using asuka.CommandOptions;

namespace asuka.Validators
{
    public class RequiresInputOptionValidator : AbstractValidator<IRequiresInputOption>
    {
        public RequiresInputOptionValidator()
        {
            RuleFor(opts => opts.Input)
                    .Matches(@"^http(s)?:\/\/(nhentai\.net)\b([//g]*)\b([\d]{1,6})\/?$")
                    .WithMessage("Enter a valid URL.");
        }
    }
}