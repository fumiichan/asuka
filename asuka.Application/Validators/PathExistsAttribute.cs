using System.ComponentModel.DataAnnotations;
using System.IO;

namespace asuka.Application.Validators;

public class PathExistsAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string path && !string.IsNullOrEmpty(path) && File.Exists(path))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult($"The path '{value}' is not found.");
    }
}
