using System;
using System.Collections.Generic;
using System.Text;

namespace asuka.Exceptions
{
  public class InvalidSearchQueryException : Exception
  {
    public InvalidSearchQueryException() : base("Invalid Search Query. Make sure you defined queries.")
    { }
  }
}
