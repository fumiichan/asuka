using System;
using System.Collections.Generic;
using FluentValidation.Results;

namespace asuka.Output;

public static class ValidationErrorExtensions
{
    public static void PrintValidationExceptions(this List<ValidationFailure> failures)
    {
        foreach (var failure in failures)
        {
            Console.WriteLine(failure.ErrorMessage);
        }
    }
}
