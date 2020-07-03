using System.Collections.Generic;
using System.Linq;

namespace asuka.Utils
{
  public static class Validators
  {
    public static bool IsNullOrEmpty<T> (this IEnumerable<T> data)
    {
      return data == null || !data.Any();
    }
  }
}
