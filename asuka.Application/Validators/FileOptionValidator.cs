using System.IO;
using asuka.Application.Commandline.Options;
using FluentValidation;

namespace asuka.Application.Validators;

public class FileOptionValidator : AbstractValidator<FileCommandOptions>
{
    public FileOptionValidator()
    {
        RuleFor(opts => opts.FilePath)
            .Must(x =>
            {
                if (!File.Exists(x))
                {
                    return false;
                }

                var fileStat = new FileInfo(x).Length;
                return fileStat < 5242880;
            })
            .WithMessage("File doesn't exist or exceeding file size limitation.");
    }
}
