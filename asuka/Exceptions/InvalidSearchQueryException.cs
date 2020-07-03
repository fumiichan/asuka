using System;
using System.Collections.Generic;
using System.Text;

namespace asuka.Exceptions
{
  class InvalidSearchQueryException : Exception
  {
    public InvalidSearchQueryException() : base("Invalid Search Query. Make sure you defined queries.")
    { }
  }
}
