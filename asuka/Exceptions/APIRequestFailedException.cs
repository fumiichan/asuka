using System;

namespace asuka.Exceptions
{
  public class APIRequestFailedException : Exception
  {
    public APIRequestFailedException() : base("Request Failed due to an Network problem.")
    { }
  }
}
