using System;
using System.Collections.Generic;
using System.Linq;

namespace asuka.Application.Utilities;

public static class ActivateTypes
{
    public static IEnumerable<T> GetAllMatchingTypes<T>(IEnumerable<Type> types)
    {
        return types
            .Select(Activator.CreateInstance)
            .Cast<T>();
    }
}
