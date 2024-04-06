using System.ComponentModel.DataAnnotations;
using System.IO;

namespace asuka.Application.Validators;

public class FileWithinSizeLimitsAttribute : ValidationAttribute
{
    private readonly long _maximumSize;

    public FileWithinSizeLimitsAttribute(long maximumSize = 5242880)
    {
        _maximumSize = maximumSize;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string path && !string.IsNullOrEmpty(path) && File.Exists(path))
        {
            var size = new FileInfo(path).Length;
            return size > _maximumSize
                ? new ValidationResult($"The file '{path}' is greater than {_maximumSize} bytes.")
                : ValidationResult.Success;
        }

        return new ValidationResult($"The path '{value}' cannot be found.");
    }
}
