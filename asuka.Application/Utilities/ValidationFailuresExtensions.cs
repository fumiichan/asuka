using System.Collections.Generic;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace asuka.Application.Utilities;

public static class ValidationFailuresExtensions
{
    public static void PrintErrors(this IList<ValidationFailure> errors, ILogger logger)
    {
        foreach (var error in errors)
        {
            logger.LogCritical($"{error.ErrorCode}: {error.ErrorMessage}");
        }
    }
}
