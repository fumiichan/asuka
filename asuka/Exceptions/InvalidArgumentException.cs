using System;

namespace asuka.Exceptions
{
  public class InvalidArgumentException : Exception
  {
    public InvalidArgumentException() : base("You entered a invalid nhentai URL or code.")
    { }
  }
}
