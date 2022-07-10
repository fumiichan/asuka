using System.Collections.Generic;
using FluentValidation.Results;

namespace asuka.Output;

public interface IConsoleWriter
{
    void WriteLine(object message);
    void WarningLine(object message);
    void ErrorLine(string message);
    void SuccessLine(string message);
    void ValidationErrors(IEnumerable<ValidationFailure> errors);
}
