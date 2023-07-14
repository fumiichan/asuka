using System.Collections.Generic;
using asuka.Application.Output.ConsoleWriter;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace asuka.Application.Utilities;

public static class ValidationFailuresExtensions
{
    public static void PrintErrors(this IEnumerable<ValidationFailure> errors, IConsoleWriter console)
    {
        foreach (var error in errors)
        {
            console.WriteError($"{error.ErrorCode}: {error.ErrorMessage}");
        }
    }
}
